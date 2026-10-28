#Requires -Version 7.4
<#
.SYNOPSIS
    Packages the self-contained Windows MCP server for Claude Desktop.

.DESCRIPTION
    Fork: the Japanese fork is not published on npm, so the MCPB ships the
    self-contained excel-mcp-server.exe instead of an npx launcher.

    The bundle contains manifest.json, icon-512.png, README.md, LICENSE,
    CHANGELOG.md and server/excel-mcp-server.exe.
#>
[CmdletBinding()]
param(
    [string]$Version,
    [string]$OutputDir = './artifacts',
    [string]$McpRuntimeExecutable
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $root 'scripts\PackageHelpers.ps1')
if (-not $Version) { $Version = ([xml][IO.File]::ReadAllText((Join-Path $root 'Directory.Build.props'))).Project.PropertyGroup.Version | Select-Object -First 1 }
$Version = Get-PackageVersion $Version
$output = [IO.Path]::GetFullPath($OutputDir, $PSScriptRoot)
Assert-PackageOutputPath $output $root
if ($output -eq $PSScriptRoot) { throw "Unsafe package output directory: $output" }
Invoke-PackageStage 'mcpb' {
    param($stage)
    $executable = $null
    if ($McpRuntimeExecutable) {
        $executable = [IO.Path]::GetFullPath($McpRuntimeExecutable, $root)
        if (-not [IO.File]::Exists($executable)) { throw "MCP runtime executable not found: $executable" }
    }
    else {
        $runtimeDir = Join-Path $stage 'runtime'
        Invoke-PackageCommand $root 'dotnet' @('publish', (Join-Path $root 'src\ExcelMcp.McpServer\ExcelMcp.McpServer.csproj'),
            '-c', 'Release', '-r', 'win-x64', '--self-contained', 'true', '-p:PublishSingleFile=true',
            '-p:IncludeNativeLibrariesForSelfExtract=true', '-p:PublishTrimmed=false', '-p:PublishReadyToRun=false',
            '-p:NuGetAudit=false', "-p:Version=$Version", '-o', $runtimeDir, '--verbosity', 'minimal') | Out-Host
        $executable = Join-Path $runtimeDir 'Sbroenne.ExcelMcp.McpServer.exe'
        Invoke-PackageCommand $root $executable @('--version') | Out-Host
    }
    Assert-PackageRuntimeArchitecture $executable 'x64'
    $product = [Diagnostics.FileVersionInfo]::GetVersionInfo($executable).ProductVersion
    if (-not $product -or $product.Split('+')[0] -cne $Version) { throw "Runtime version $product does not match $Version." }
    $serverDir = Join-Path $stage 'server'
    [IO.Directory]::CreateDirectory($serverDir) | Out-Null
    Copy-PackageItem $executable (Join-Path $serverDir 'excel-mcp-server.exe')
    $manifest = Read-PackageJson (Join-Path $PSScriptRoot 'manifest.json')
    $manifest.version = $Version
    Write-PackageJson (Join-Path $stage 'manifest.json') $manifest
    foreach ($name in 'icon-512.png', 'README.md', 'LICENSE', 'CHANGELOG.md') {
        $source = if ($name -in @('LICENSE', 'CHANGELOG.md')) { $root } else { $PSScriptRoot }
        Copy-PackageItem (Join-Path $source $name) (Join-Path $stage $name)
    }
    $payload = Join-Path $stage 'payload'
    [IO.Directory]::CreateDirectory($payload) | Out-Null
    foreach ($name in 'manifest.json', 'icon-512.png', 'README.md', 'LICENSE', 'CHANGELOG.md', 'server') {
        Move-PackageItem (Join-Path $stage $name) (Join-Path $payload $name)
    }
    $archive = Join-Path $stage 'bundle.zip'
    [IO.Compression.ZipFile]::CreateFromDirectory($payload, $archive)
    $zip = [IO.Compression.ZipFile]::OpenRead($archive)
    try {
        foreach ($name in 'manifest.json', 'icon-512.png', 'README.md', 'LICENSE', 'CHANGELOG.md', 'server/excel-mcp-server.exe') {
            if (-not $zip.GetEntry($name)) { throw "MCPB is missing required content: $name" }
        }
        if ($manifest.server.type -cne 'binary') { throw 'MCPB manifest must keep the binary server type.' }
        if ($manifest.name -cne 'excel-mcp-ja') { throw 'MCPB manifest must keep the fork name excel-mcp-ja.' }
    } finally { $zip.Dispose() }
    [IO.Directory]::CreateDirectory($output) | Out-Null
    $destination = Join-Path $output "excel-mcp-ja-$Version.mcpb"
    Install-PackageOutput $archive $destination
    Install-PackageOutput (Join-Path $payload 'manifest.json') (Join-Path $output 'manifest.json')
    $destination
}
