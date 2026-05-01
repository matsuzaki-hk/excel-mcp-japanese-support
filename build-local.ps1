# Local build script for ExcelMcp MCP Server with Japanese support
# This script builds the project and renames the exe to mcp-excel.exe

$ErrorActionPreference = "Stop"

# Configuration
$ProjectPath = "src\ExcelMcp.McpServer\ExcelMcp.McpServer.csproj"
$OutputPath = "C:\work\ExcelMcp-MCP-Server"
$TempOutputPath = "C:\work\ExcelMcp-MCP-Server-temp"

Write-Host "Building ExcelMcp MCP Server with Japanese support..." -ForegroundColor Green

# Clean previous build
Write-Host "Cleaning previous build..." -ForegroundColor Yellow
if (Test-Path $TempOutputPath) {
    Remove-Item -Path $TempOutputPath -Recurse -Force
}

# Build
Write-Host "Building project..." -ForegroundColor Yellow
dotnet publish $ProjectPath `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishTrimmed=false `
    -p:PublishReadyToRun=false `
    --output $TempOutputPath

# Copy MCP files
Write-Host "Copying MCP files..." -ForegroundColor Yellow
if (Test-Path $TempOutputPath) {
    # Check if the exe exists
    $originalExe = Get-ChildItem $TempOutputPath -Filter "*.exe"
    if ($originalExe) {
        Write-Host "Found executable: $($originalExe.Name)" -ForegroundColor Cyan
        
        # Create output directory
        New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
        
        # Copy all files except the exe
        Get-ChildItem $TempOutputPath -Exclude "*.exe" | Copy-Item -Destination $OutputPath -Force -Recurse
        
        # Copy exe with new name
        Copy-Item $originalExe.FullName -Destination "$OutputPath\mcp-excel.exe" -Force
        
        # Copy README and LICENSE if they exist
        if (Test-Path "README.md") {
            Copy-Item "README.md" -Destination $OutputPath -Force
        }
        if (Test-Path "LICENSE") {
            Copy-Item "LICENSE" -Destination $OutputPath -Force
        }
        
        Write-Host "Build completed successfully!" -ForegroundColor Green
        Write-Host "Output: $OutputPath\mcp-excel.exe" -ForegroundColor Cyan
    } else {
        Write-Error "No executable found in $TempOutputPath"
        exit 1
    }
} else {
    Write-Error "Build failed - output directory not found"
    exit 1
}

# Clean up
Write-Host "Cleaning temporary files..." -ForegroundColor Yellow
Remove-Item -Path $TempOutputPath -Recurse -Force

Write-Host "Done!" -ForegroundColor Green
