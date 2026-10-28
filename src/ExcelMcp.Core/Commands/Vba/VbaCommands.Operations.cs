using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Sbroenne.ExcelMcp.ComInterop;
using Sbroenne.ExcelMcp.ComInterop.Session;
using Sbroenne.ExcelMcp.Core.Models;
using Vbe = Microsoft.Vbe.Interop;

namespace Sbroenne.ExcelMcp.Core.Commands;

/// <summary>
/// VBA script operations (Run)
/// </summary>
public partial class VbaCommands
{
    /// <inheritdoc />
    public OperationResult Run(IExcelBatch batch, string procedureName, TimeSpan? timeout, params string[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(procedureName);
        if (procedureName.Contains('!'))
        {
            throw new ArgumentException(
                "Use an unqualified procedure name such as Module1.MySub. The selected session determines the workbook.",
                nameof(procedureName));
        }
        parameters ??= [];
        if (parameters.Length > 30)
        {
            throw new ArgumentException("Excel accepts at most 30 macro parameters.", nameof(parameters));
        }

        EnsureVbaFile(batch.WorkbookPath);

        using var timeoutCts = timeout.HasValue
            ? new CancellationTokenSource(timeout.Value)
            : null;

        Func<OperationResult> execute = () => batch.Execute((ctx, ct) =>
        {
            var owner = batch is AttachedWorkbookBatch attached ? attached.Owner : batch as ExcelBatch;
            if (owner?.RequiresVbaApproval(ctx.Book) == true)
                throw new InvalidOperationException(
                    batch is AttachedWorkbookBatch
                    ? "VBA macros were disabled when this workbook opened. Use Excel to review macro permissions " +
                    "and run the procedure. This workbook was already open in Excel, so this session cannot reopen it " +
                    "to enable macros. No procedure was run."
                    : "VBA macros were disabled when this workbook opened. Obtain approval for file enable-macros " +
                    "with approved:true and allow_reopen:true (CLI: session enable-macros --approved --allow-reopen) " +
                    "before running a procedure. The saved workbook must be reopened; unsaved changes are not discarded.");
            var originalCulture = CultureInfo.CurrentCulture;
            var originalUiCulture = CultureInfo.CurrentUICulture;
            try
            {
                var excelCulture = CultureInfo.GetCultureInfo("en-US");
                CultureInfo.CurrentCulture = excelCulture;
                CultureInfo.CurrentUICulture = excelCulture;

                string qualifiedName = $"'{ctx.Book.Name.Replace("'", "''", StringComparison.Ordinal)}'!{procedureName}";
                var args = new object[30];
                Array.Fill(args, Type.Missing);
                for (int i = 0; i < parameters.Length; i++)
                {
                    args[i] = parameters[i];
                }

                ctx.App.Run(qualifiedName,
                    args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9],
                    args[10], args[11], args[12], args[13], args[14], args[15], args[16], args[17], args[18], args[19],
                    args[20], args[21], args[22], args[23], args[24], args[25], args[26], args[27], args[28], args[29]);

                return new OperationResult { Success = true, FilePath = batch.WorkbookPath };
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }, timeoutCts?.Token ?? default);

        return ExecuteWithTimeout(execute, timeoutCts);
    }

    private static OperationResult ExecuteWithTimeout(
        Func<OperationResult> operation,
        CancellationTokenSource? timeoutCts)
    {
        try
        {
            return operation();
        }
        catch (OperationCanceledException ex) when (timeoutCts?.IsCancellationRequested == true)
        {
            throw new TimeoutException("VBA procedure execution exceeded its requested timeout.", ex);
        }
    }

    /// <inheritdoc />
    public OperationResult Delete(IExcelBatch batch, string moduleName)
    {
        EnsureVbaFile(batch.WorkbookPath);

        // Check VBA trust BEFORE attempting operation
        if (!IsVbaTrustEnabled())
        {
            throw new OperationFailureException(OperationFailureCategory.Permissions, VbaTrustErrorMessage);
        }

        return batch.Execute((ctx, ct) =>
        {
            Vbe.VBProject? vbaProject = null;
            Vbe.VBComponents? vbComponents = null;
            Vbe.VBComponent? targetComponent = null;
            try
            {
                vbaProject = ctx.Book.VBProject;
                EnsureProjectUnlocked(vbaProject);
                vbComponents = vbaProject.VBComponents;

                for (int i = 1; i <= vbComponents.Count; i++)
                {
                    Vbe.VBComponent? component = null;
                    try
                    {
                        component = vbComponents.Item(i);
                        if (component.Name == moduleName)
                        {
                            targetComponent = component;
                            component = null; // Don't release - we're keeping it
                            break;
                        }
                    }
                    finally
                    {
                        if (component != null)
                        {
                            ComUtilities.Release(ref component);
                        }
                    }
                }

                if (targetComponent == null)
                {
                    throw new OperationFailureException(OperationFailureCategory.NotFound, $"Module '{moduleName}' not found.");
                }

                vbComponents.Remove(targetComponent);

                return new OperationResult { Success = true, FilePath = batch.WorkbookPath };
            }
            catch (COMException comEx) when (IsVbaTrustError(comEx))
            {
                throw new OperationFailureException(OperationFailureCategory.Permissions, VbaTrustErrorMessage, comEx);
            }
            catch (COMException comEx) when (comEx.ErrorCode == GenericOfficeAutomationError)
            {
                // Trust passed IsVbaTrustError, so this generic 0x800A03EC is a real,
                // non-trust failure. Surface it with COM environment diagnostics (issue #671).
                throw new InvalidOperationException(BuildGenericComErrorMessage(comEx), comEx);
            }
            finally
            {
                ComUtilities.Release(ref targetComponent);
                ComUtilities.Release(ref vbComponents);
                ComUtilities.Release(ref vbaProject);
            }
        });
    }

    /// <inheritdoc />
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Interface implementation requires instance method")]
    public VbaExportResult Export(IExcelBatch batch, string[]? moduleNames, string? outputDirectory, bool overwrite = false)
    {
        EnsureVbaFile(batch.WorkbookPath);

        // Check VBA trust BEFORE attempting operation
        if (!IsVbaTrustEnabled())
        {
            throw new InvalidOperationException(VbaTrustErrorMessage);
        }

        // Determine output directory
        if (string.IsNullOrEmpty(outputDirectory))
        {
            // Create a folder in the workspace
            outputDirectory = Path.Combine(Path.GetDirectoryName(batch.WorkbookPath) ?? Directory.GetCurrentDirectory(), "vba_export");
            Directory.CreateDirectory(outputDirectory);
        }
        else if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        return batch.Execute(ExportModules);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ErrorHandling", "EXL002",
            Justification = "Per-module export failures are transported as FailedModules/ErrorMessage entries so the remaining modules still export.")]
        VbaExportResult ExportModules(ExcelContext ctx, CancellationToken ct)
        {
            Vbe.VBProject? vbaProject = null;
            Vbe.VBComponents? vbComponents = null;
            try
            {
                vbaProject = ctx.Book.VBProject;
                vbComponents = vbaProject.VBComponents;

                var result = new VbaExportResult
                {
                    Success = true,
                    FilePath = batch.WorkbookPath,
                    OutputDirectory = outputDirectory,
                    ModulesExported = 0,
                    ExportedFiles = [],
                    FailedModules = []
                };

                // Determine which modules to export
                var modulesToExport = new List<string>();
                if (moduleNames == null || moduleNames.Length == 0)
                {
                    // Export all modules
                    for (int i = 1; i <= vbComponents.Count; i++)
                    {
                        Vbe.VBComponent? component = null;
                        try
                        {
                            component = vbComponents.Item(i);
                            // Only export standard modules, class modules, and forms.
                            // Skip document modules as they are tied to worksheets.
                            if (component.Type is Vbe.vbext_ComponentType.vbext_ct_StdModule
                                or Vbe.vbext_ComponentType.vbext_ct_ClassModule
                                or Vbe.vbext_ComponentType.vbext_ct_MSForm)
                            {
                                modulesToExport.Add(component.Name);
                            }
                        }
                        finally
                        {
                            ComUtilities.Release(ref component);
                        }
                    }
                }
                else
                {
                    modulesToExport.AddRange(moduleNames);
                }

                // Export each module
                foreach (var moduleName in modulesToExport)
                {
                    Vbe.VBComponent? component = null;
                    try
                    {
                        // Find the component
                        for (int i = 1; i <= vbComponents.Count; i++)
                        {
                            Vbe.VBComponent? comp = null;
                            try
                            {
                                comp = vbComponents.Item(i);
                                if (comp.Name == moduleName)
                                {
                                    component = comp;
                                    comp = null;
                                    break;
                                }
                            }
                            finally
                            {
                                if (comp != null)
                                {
                                    ComUtilities.Release(ref comp);
                                }
                            }
                        }

                        if (component == null)
                        {
                            result.FailedModules.Add(moduleName);
                            continue;
                        }

                        // Determine file extension based on component type
                        // StdModule=.bas, ClassModule=.cls, MSForm=.frm, Document=skip
                        int componentType = (int)component.Type;
                        string extension = componentType switch
                        {
                            2 => ".cls",
                            3 => ".frm",
                            _ => ".bas"
                        };

                        // Generate output file path
                        var fileName = $"{moduleName}{extension}";
                        var filePath = Path.Combine(outputDirectory, fileName);

                        // Handle overwrite
                        if (!overwrite && File.Exists(filePath))
                        {
                            // Append timestamp to avoid overwrite
                            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
                            fileName = $"{moduleName}_{timestamp}{extension}";
                            filePath = Path.Combine(outputDirectory, fileName);
                        }

                        // Export the module (for UserForms, VBE also exports a .frx binary alongside .frm)
                        component.Export(filePath);

                        // Read the file to get line count
                        var lines = File.ReadAllLines(filePath);
                        var lineCount = lines.Length;

                        result.ExportedFiles.Add(new ExportedModuleInfo
                        {
                            ModuleName = moduleName,
                            FilePath = filePath,
                            Overwritten = overwrite && File.Exists(Path.Combine(outputDirectory, $"{moduleName}{extension}")),
                            LineCount = lineCount
                        });

                        result.ModulesExported++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedModules.Add(moduleName);
                        result.Success = false;
                        result.ErrorMessage = $"Failed to export module '{moduleName}': {ex.Message}";
                    }
                    finally
                    {
                        if (component != null)
                        {
                            ComUtilities.Release(ref component);
                        }
                    }
                }

                if (result.FailedModules.Count > 0)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Failed to export {result.FailedModules.Count} module(s): {string.Join(", ", result.FailedModules)}";
                }

                return result;
            }
            catch (COMException comEx) when (comEx.Message.Contains("programmatic access", StringComparison.OrdinalIgnoreCase) ||
                                             comEx.ErrorCode == unchecked((int)0x800A03EC))
            {
                throw new InvalidOperationException(VbaTrustErrorMessage, comEx);
            }
            finally
            {
                ComUtilities.Release(ref vbComponents);
                ComUtilities.Release(ref vbaProject);
            }
        }
    }
}
