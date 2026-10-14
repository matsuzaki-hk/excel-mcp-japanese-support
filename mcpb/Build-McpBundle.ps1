<#
.SYNOPSIS
    Creates the MCPB (MCP Bundle) package for Claude Desktop.

.DESCRIPTION
    This script builds the MCP Server as a self-contained Windows x64 executable
    and packages it as an .mcpb file for one-click installation in Claude Desktop.
    The packaging itself is implemented by the typed build tool
    (tools/ExcelMcp.Build, Operation 'Mcpb'); this script is a thin wrapper.

.PARAMETER Version
    The version number for the package (e.g., "1.0.0"). If not specified,
    reads from Directory.Build.props.

.PARAMETER OutputDir
    The output directory for the MCPB package. Defaults to ./artifacts

.EXAMPLE
    .\Build-McpBundle.ps1
    Creates MCPB package with version from Directory.Build.props

.EXAMPLE
    .\Build-McpBundle.ps1 -Version "1.2.0"
    Creates MCPB package with specified version

.NOTES
    Requirements:
    - .NET 10 SDK
    - Windows to execute the packaged server verification; other platforms cross-compile it

    Output:
    mcpb/artifacts/excel-mcp-ja-{version}.mcpb

    Contents:
    ├── manifest.json
    ├── icon-512.png
    ├── README.md
    ├── LICENSE
    ├── CHANGELOG.md
    └── server/
        └── excel-mcp-server.exe
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$Version,

    [Parameter()]
    [string]$OutputDir = "./artifacts",

    [Parameter()]
    [string]$McpRuntimeExecutable
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $root 'scripts\PackageHelpers.ps1')
Invoke-TypedPackage -SourceRoot $root -Options @{
    Operation = 'Mcpb'; Version = $Version
    OutputDirectory = [IO.Path]::GetFullPath($OutputDir, $PSScriptRoot)
    McpRuntimeExecutable = $McpRuntimeExecutable
}
