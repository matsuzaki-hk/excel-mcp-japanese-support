using Sbroenne.ExcelMcp.ComInterop.Session;
using Sbroenne.ExcelMcp.Core.Attributes;
using Sbroenne.ExcelMcp.Core.Models;

namespace Sbroenne.ExcelMcp.Core.Commands;

/// <summary>
/// VBA module and procedure operations for macro-enabled workbooks (.xlsm).
///
/// PREREQUISITES:
/// - Workbook must be macro-enabled (.xlsm)
/// - VBA trust must be enabled manually in Excel for project access
///
/// SCOPE:
/// - List and view existing VBA components and their procedures
/// - Import creates new standard modules from inline code or a file
/// - Update/delete works on existing VBA components by name
/// - Run executes a procedure by name
///
/// RUN: procedureName format is 'Module.Procedure' (e.g., 'Module1.MySub').
/// ExcelMcp does not configure VBA trust settings for you.
/// </summary>
[ServiceCategory("vba", "Vba")]
[McpTool("vba", Title = "VBA Operations", Destructive = true, Category = "automation",
    Description = "VBA module and procedure operations for macro-enabled workbooks (.xlsm). Lists and views existing VBA components, imports new standard modules, updates or deletes module code, and runs procedures. VBA trust must be enabled manually in Excel; ExcelMcp does not configure Trust Center settings. IMPORTANT: session_id is ALWAYS required. Call file(action:'open') first to get a session_id, then pass it to every vba operation.")]
public interface IVbaCommands
{
    /// <summary>
    /// Lists all VBA modules and procedures in the workbook
    /// </summary>
    [ServiceAction("list")]
    VbaListResult List(IExcelBatch batch);

    /// <summary>
    /// Views VBA module code without exporting to file
    /// </summary>
    /// <param name="moduleName">(required for: view) Name of the VBA module</param>
    [ServiceAction("view")]
    VbaViewResult View(IExcelBatch batch, [RequiredParameter] string moduleName);

    /// <summary>
    /// Imports VBA code to create a new standard module, or imports a .bas/.cls/.frm file directly.
    /// To import a UserForm from a file, use vbaFilePath instead of vbaCode.
    /// When importing from a file path, moduleName is ignored (module name is taken from the file).
    /// </summary>
    /// <param name="moduleName">(required for: import with inline vbaCode) Name for the new module. Ignored when vbaFilePath is provided.</param>
    /// <param name="vbaCode">Inline VBA code string for standard modules. Use vbaFilePath instead for .frm/.bas/.cls files.</param>
    /// <param name="vbaFilePath">Absolute path to a .bas/.cls/.frm file to import directly. Supports UserForms. Takes precedence over vbaCode when provided.</param>
    [ServiceAction("import")]
    OperationResult Import(IExcelBatch batch, [RequiredParameter] string moduleName, [FileOrValue] string? vbaCode, string? vbaFilePath = null);

    /// <summary>
    /// Updates an existing VBA module with new code
    /// </summary>
    /// <param name="moduleName">(required for: update) Name of the module to update</param>
    /// <param name="vbaCode">(required for: update) New VBA code</param>
    [ServiceAction("update")]
    OperationResult Update(IExcelBatch batch, [RequiredParameter] string moduleName, [RequiredParameter][FileOrValue] string vbaCode);

    /// <summary>
    /// Runs a VBA procedure with optional parameters
    /// </summary>
    /// <param name="procedureName">(required for: run) Name of the procedure to run (for example "Module1.MySub")</param>
    /// <param name="timeout">Optional timeout for execution</param>
    /// <param name="parameters">Optional parameters to pass to the procedure</param>
    [ServiceAction("run")]
    OperationResult Run(IExcelBatch batch, [RequiredParameter] string procedureName, TimeSpan? timeout, params string[] parameters);

    /// <summary>
    /// Deletes a VBA module
    /// </summary>
    /// <param name="moduleName">(required for: delete) Name of the module to delete</param>
    [ServiceAction("delete")]
    OperationResult Delete(IExcelBatch batch, [RequiredParameter] string moduleName);

    /// <summary>
    /// Exports one or more VBA modules to .bas files
    /// </summary>
    /// <param name="moduleNames">Array of module names to export. If null or empty, exports all modules</param>
    /// <param name="outputDirectory">Output directory path. If null, creates a folder next to the workbook</param>
    /// <param name="overwrite">Whether to overwrite existing files. If false, appends timestamp to avoid conflicts</param>
    [ServiceAction("export")]
    VbaExportResult Export(IExcelBatch batch, string[]? moduleNames, string? outputDirectory, bool overwrite = false);
}



