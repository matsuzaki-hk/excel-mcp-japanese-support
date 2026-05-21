using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Sbroenne.ExcelMcp.ComInterop;
using Sbroenne.ExcelMcp.ComInterop.Session;
using Sbroenne.ExcelMcp.Core.Models;

namespace Sbroenne.ExcelMcp.Core.Commands;

/// <summary>
/// VBA script operations (Run)
/// </summary>
public partial class VbaCommands
{
    /// <inheritdoc />
    public OperationResult Run(IExcelBatch batch, string procedureName, TimeSpan? timeout, params string[] parameters)
    {
        parameters ??= [];

        var (isValid, validationError) = ValidateVbaFile(batch.WorkbookPath);
        if (!isValid)
        {
            throw new ArgumentException(validationError, nameof(batch));
        }

        return batch.Execute((ctx, ct) =>
        {
            var originalCulture = CultureInfo.CurrentCulture;
            var originalUiCulture = CultureInfo.CurrentUICulture;
            object? originalAutomationSecurity = null;
            try
            {
                var excelCulture = CultureInfo.GetCultureInfo("en-US");
                CultureInfo.CurrentCulture = excelCulture;
                CultureInfo.CurrentUICulture = excelCulture;

                // Explicit macro execution is an opt-in operation. Temporarily switch automation
                // security to low so Application.Run can execute on workbooks reopened by the
                // automation host, then restore the previous setting after the call.
                dynamic app = (dynamic)(object)ctx.App;
                originalAutomationSecurity = app.AutomationSecurity;
                app.AutomationSecurity = 1;

                // Use late-bound COM dispatch via Type.InvokeMember to avoid dependency on
                // Microsoft.Vbe.Interop.dll, which is not available on Click-to-Run Office
                // installations. The early-bound PIA call ctx.App.Run() triggers assembly
                // resolution of VBE types through the embedded Application interface metadata.
                var args = new object[1 + parameters.Length];
                args[0] = procedureName;
                for (int i = 0; i < parameters.Length; i++)
                {
                    args[i + 1] = parameters[i];
                }

                var excelApplicationType = Type.GetTypeFromProgID("Excel.Application")
                    ?? throw new InvalidOperationException("Excel is not installed or not properly registered.");

                excelApplicationType.InvokeMember(
                    "Run",
                    BindingFlags.InvokeMethod,
                    null,
                    ctx.App,
                    args,
                    excelCulture);

                return new OperationResult { Success = true, FilePath = batch.WorkbookPath };
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
            finally
            {
                if (originalAutomationSecurity != null)
                {
                    try
                    {
                        // PIA gap: AutomationSecurity lives in office.dll (Microsoft.Office.Core),
                        // so restoring it must stay late-bound to avoid loading a missing Office core assembly.
                        ((dynamic)(object)ctx.App).AutomationSecurity = originalAutomationSecurity;
                    }
                    catch (COMException)
                    {
                    }
                }

                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        });
    }

    /// <inheritdoc />
    public OperationResult Delete(IExcelBatch batch, string moduleName)
    {
        var (isValid, validationError) = ValidateVbaFile(batch.WorkbookPath);
        if (!isValid)
        {
            throw new InvalidOperationException(validationError);
        }

        // Check VBA trust BEFORE attempting operation
        if (!IsVbaTrustEnabled())
        {
            throw new InvalidOperationException(VbaTrustErrorMessage);
        }

        return batch.Execute((ctx, ct) =>
        {
            dynamic? vbaProject = null;
            dynamic? vbComponents = null;
            dynamic? targetComponent = null;
            try
            {
                // PIA gap: VBProject is in Microsoft.Vbe.Interop, not the Excel PIA.
                // No .NET 5+ compatible NuGet package exists for VBE types.
                vbaProject = ((dynamic)ctx.Book).VBProject;
                vbComponents = vbaProject.VBComponents;

                for (int i = 1; i <= vbComponents.Count; i++)
                {
                    dynamic? component = null;
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
                    throw new InvalidOperationException($"Module '{moduleName}' not found.");
                }

                vbComponents.Remove(targetComponent);

                return new OperationResult { Success = true, FilePath = batch.WorkbookPath };
            }
            catch (COMException comEx) when (IsVbaTrustError(comEx))
            {
                throw new InvalidOperationException(VbaTrustErrorMessage, comEx);
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
    public VbaExportResult Export(IExcelBatch batch, string[]? moduleNames, string? outputDirectory, bool overwrite = false)
    {
        var (isValid, validationError) = ValidateVbaFile(batch.WorkbookPath);
        if (!isValid)
        {
            throw new InvalidOperationException(validationError);
        }

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

        return batch.Execute((ctx, ct) =>
        {
            dynamic? vbaProject = null;
            dynamic? vbComponents = null;
            try
            {
                vbaProject = ((dynamic)ctx.Book).VBProject;
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
                        dynamic? component = null;
                        try
                        {
                            component = vbComponents.Item(i);
                            var componentType = component.Type;
                            // Only export standard modules (type = 1), class modules (type = 2), and forms (type = 3)
                            // Skip document modules (type = 100) as they are tied to worksheets
                            if (componentType is 1 or 2 or 3)
                            {
                                modulesToExport.Add(component.Name);
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
                }
                else
                {
                    modulesToExport.AddRange(moduleNames);
                }

                // Export each module
                foreach (var moduleName in modulesToExport)
                {
                    dynamic? component = null;
                    try
                    {
                        // Find the component
                        component = null;
                        for (int i = 1; i <= vbComponents.Count; i++)
                        {
                            dynamic? comp = null;
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
                        // 1=Standard Module(.bas), 2=Class Module(.cls), 3=UserForm(.frm), 100=Document(skip)
                        int componentType = component.Type;
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
        });
    }
}



