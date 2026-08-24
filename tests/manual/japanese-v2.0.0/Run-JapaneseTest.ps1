#Requires -Version 7.0
[CmdletBinding()]
param(
    [string]$CliPath = "C:\Temp\ExcelMcpCLI\excelcli.exe",
    [string]$TestDir = $PSScriptRoot,
    [switch]$Show,
    [switch]$KeepFiles
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$assetsDir = Join-Path $TestDir "assets"
$workDir = Join-Path $TestDir "work"
$workbookPath = Join-Path $workDir "日本語テスト_売上.xlsx"
$savePath = Join-Path $workDir "保存テスト_売上.xlsx"
$resultPath = Join-Path $workDir "test-result.json"

New-Item -ItemType Directory -Force -Path $workDir | Out-Null
Remove-Item $workbookPath -ErrorAction SilentlyContinue
Remove-Item $savePath -ErrorAction SilentlyContinue
Remove-Item $resultPath -ErrorAction SilentlyContinue

$csvPath = Join-Path $assetsDir "売上データ.csv"
$xsdPath = Join-Path $assetsDir "XSD_商品マスタ.xsd"
$bmpPath = Join-Path $assetsDir "画像_テスト.bmp"

function Invoke-Cli {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments)
    $output = & $CliPath $Arguments 2>&1
    $exitCode = $LASTEXITCODE
    return @{ Output = $output -join "`n"; ExitCode = $exitCode }
}

function Step {
    param(
        [string]$Name,
        [string[]]$Arguments
    )
    Write-Host "== $Name =="
    $r = Invoke-Cli $Arguments
    Write-Host $r.Output
    if ($r.ExitCode -ne 0) {
        throw "FAILED: $Name`n$r.Output"
    }
    return $r.Output | ConvertFrom-Json
}

$results = @{
    Version = "v2.0.0-ja.1"
    StartedAt = [DateTime]::Now.ToString("o")
    Steps = @()
    Success = $false
}

function Add-Result {
    param([string]$Name, [bool]$Success, [string]$Message)
    $results.Steps += [PSCustomObject]@{
        Name = $Name
        Success = $Success
        Message = $Message
    }
}

try {
    # 1. 日本語ファイルパスをテスト
    Step -Name "日本語ファイルパステスト" -Arguments @("file", "test", $workbookPath) | Out-Null
    Add-Result "日本語ファイルパステスト" $true "file test for '$workbookPath'"

    # 2. 新規ブック作成
    $showFlag = if ($Show) { @("--show") } else { @() }
    $openArgs = @("session", "create", $workbookPath) + $showFlag
    $session = Step -Name "新規ブック作成" -Arguments $openArgs
    $sessionId = $session.sessionId
    Add-Result "新規ブック作成" $true "sessionId=$sessionId"

    # 3. 日本語シート作成
    Step -Name "日本語シート作成" -Arguments @("sheet", "create", "--session", $sessionId, "--sheet", "売上データ") | Out-Null
    Add-Result "日本語シート作成" $true "シート '売上データ' を作成"

    # 4. QueryTable: 日本語CSVから日本語名で作成
    Step -Name "QueryTable作成" -Arguments @(
        "querytable", "create-text",
        "--session", $sessionId,
        "--sheet", "売上データ",
        "--query-table-name", "売上クエリ",
        "--source-path", $csvPath,
        "--destination-address", "A1",
        "--delimiter", ",",
        "--has-headers", "true"
    ) | Out-Null
    Add-Result "QueryTable作成" $true "QueryTable '売上クエリ' を日本語CSVから作成"

    # 5. XmlMap: 日本語XSD/日本語名で追加
    Step -Name "XmlMap追加" -Arguments @(
        "xmlmap", "add",
        "--session", $sessionId,
        "--schema-file", $xsdPath,
        "--map-name", "商品マップ",
        "--root-element-name", "商品一覧"
    ) | Out-Null
    Add-Result "XmlMap追加" $true "XmlMap '商品マップ' を追加"

    # 6. シナリオ用にセル B2 に値を設定
    Step -Name "セル値設定" -Arguments @(
        "range", "set-values",
        "--session", $sessionId,
        "--sheet", "売上データ",
        "--range", "B2",
        "--values", "[[1000000]]"
    ) | Out-Null
    Add-Result "セル値設定" $true "B2 に 1000000 を設定"

    # 7. Scenario: 日本語シナリオ名で作成
    Step -Name "Scenario作成" -Arguments @(
        "analysis", "create-scenario",
        "--session", $sessionId,
        "--sheet", "売上データ",
        "--scenario-name", "楽観シナリオ",
        "--changing-cells", "B2",
        "--values", "[1000000]"
    ) | Out-Null
    Add-Result "Scenario作成" $true "Scenario '楽観シナリオ' を作成"

    # 8. Drawing: 日本語名で画像追加
    Step -Name "画像追加" -Arguments @(
        "drawing", "add-image",
        "--session", $sessionId,
        "--sheet", "売上データ",
        "--image-path", $bmpPath,
        "--name", "ロゴ画像",
        "--left", "10",
        "--top", "10",
        "--width", "100",
        "--height", "100"
    ) | Out-Null
    Add-Result "画像追加" $true "Drawing 'ロゴ画像' を追加"

    # 9. Workbook: 日本語パスへ SaveAs
    Step -Name "SaveAs日本語パス" -Arguments @(
        "workbook", "save-as",
        "--session", $sessionId,
        "--target-path", $savePath
    ) | Out-Null
    Add-Result "SaveAs日本語パス" $true "SaveAs to '$savePath'"

    # 10. セッション保存・終了
    Step -Name "セッション終了" -Arguments @("session", "close", "--session", $sessionId, "--save") | Out-Null
    Add-Result "セッション終了" $true "保存してセッション終了"

    $results.Success = $true
    $results.Message = "すべての日本語テストに成功しました。"
}
catch {
    $results.Success = $false
    $results.Message = $_.Exception.Message
    Write-Error $_.Exception.Message

    try {
        $sessionList = Invoke-Cli "session", "list"
        Write-Host "Open sessions:`n$($sessionList.Output)"
    }
    catch {
        # ignore
    }
}
finally {
    $results.EndedAt = [DateTime]::Now.ToString("o")
    $results | ConvertTo-Json -Depth 3 | Set-Content -Encoding UTF8 -Path $resultPath

    if (-not $KeepFiles) {
        Remove-Item $workbookPath -ErrorAction SilentlyContinue
        Remove-Item $savePath -ErrorAction SilentlyContinue
    }

    Write-Host "`n結果: $resultPath"
}
