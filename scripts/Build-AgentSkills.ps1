<#
.SYNOPSIS
    Generates and packages the Excel MCP and CLI agent skills.

.DESCRIPTION
    GenerateOnly prepares CLI discovery and the two report-formatting skills.
    Packaging consumes that prepared output without regenerating it.
    CLI syntax and action lists are discovered through native --help, not copied
    into the skill package.

    This fork adds -Language ja: the Japanese package ships the source-managed
    excel-mcp-ja / excel-cli-ja skills as excel-skills-ja-v{version}.zip
    instead of the upstream generated set.

.EXAMPLE
    .\Build-AgentSkills.ps1 -GenerateOnly

.EXAMPLE
    .\Build-AgentSkills.ps1 -Version 1.2.0

.EXAMPLE
    .\Build-AgentSkills.ps1 -Version 1.2.0 -Language ja
#>
[CmdletBinding()]
param(
    [string]$OutputDir,
    [string]$Version,
    [switch]$GenerateOnly,
    [string]$SkillsDirectory,
    [ValidateSet("en", "ja")]
    [string]$Language = "en"
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'PackageHelpers.ps1')
if (-not $OutputDir) { $OutputDir = $GenerateOnly ? 'artifacts\generated-skills' : 'artifacts\skills' }
Invoke-TypedPackage -SourceRoot (Split-Path $PSScriptRoot -Parent) -Options @{
    Operation = 'Skills'; Version = $Version; OutputDirectory = $OutputDir
    SkillsDirectory = $SkillsDirectory; GenerateOnly = [bool]$GenerateOnly
    Language = $Language
}
