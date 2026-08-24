# ExcelMcp v2.0.0 日本語対応手動検証

本家 v1.10.8〜v2.0.0 で追加・変更された機能（Canonical file lifecycle、`file test`、QueryTable、XmlMap、What-If Analysis、Drawing、Workbook SaveAs 等）が日本語環境で正しく動作するかを検証する手動テスト手順です。

## 検証環境

- Windows 11
- Microsoft Excel 2016 以降
- ExcelMcp CLI v2.0.0-ja.1 以降

## テスト概要

以下の操作を日本語ファイルパス・日本語名前で実行し、すべて成功することを確認します。

| # | 検証項目 | 日本語要素 |
|---|---|---|
| 1 | ファイルテスト | 日本語ファイルパス |
| 2 | 新規ブック作成 | 日本語ファイルパス |
| 3 | 日本語シート作成 | 日本語シート名 `売上データ` |
| 4 | QueryTable 作成 | 日本語 CSV、日本語 QueryTable 名 `売上クエリ` |
| 5 | XmlMap 追加 | 日本語 XSD、日本語 Map 名 `商品マップ`、日本語要素名 |
| 6 | Scenario 作成 | 日本語 Scenario 名 `楽観シナリオ` |
| 7 | Drawing（画像追加） | 日本語ファイルパスの BMP、日本語名 `ロゴ画像` |
| 8 | SaveAs | 日本語出力ファイルパス |

## 前提条件

1. GitHub Release から `ExcelMcp-CLI-{version}-ja.1-windows.zip` をダウンロード・展開
2. `excelcli.exe` へのパスを確認

## 自動実行

```powershell
pwsh -ExecutionPolicy Bypass -File Run-JapaneseTest.ps1 -CliPath "C:\Temp\ExcelMcpCLI\excelcli.exe" -Show
```

オプション:

- `-CliPath`: `excelcli.exe` のパス（既定: `C:\Temp\ExcelMcpCLI\excelcli.exe`）
- `-Show`: Excel ウィンドウを表示
- `-KeepFiles`: テスト後も `work/` 内のファイルを削除しない

## 判定基準

`work/test-result.json` の `Success` が `true` になっていれば合格です。

```json
{
  "Success": true,
  "Message": "すべての日本語テストに成功しました。",
  "Steps": [
    { "Name": "QueryTable作成", "Success": true, ... },
    ...
  ]
}
```

個別ステップで `Success: false` または `COMException`/`ArgumentException` が返った場合は日本語環境で問題が発生しています。該当するステップ名とエラーメッセージを記録してください。

## 手順（個別実行）

```powershell
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$cli = "C:\Temp\ExcelMcpCLI\excelcli.exe"
$assets = "C:\Users\avalo\projects\EXCELMCP日本語対応\mcp-server-excel\tests\manual\japanese-v2.0.0\assets"
$work = "C:\Temp\ExcelMcpJapaneseTest"
New-Item -ItemType Directory -Force -Path $work | Out-Null
$workbook = "$work\日本語テスト_売上.xlsx"

# 1. 日本語ファイルパスをテスト
& $cli file test $workbook

# 2. 新規ブック作成
$created = & $cli session create $workbook
$sessionId = ($created | ConvertFrom-Json).sessionId

# 3. 日本語シート作成
& $cli sheet create --session $sessionId --sheet 売上データ

# 4. QueryTable 作成
& $cli querytable create-text --session $sessionId --sheet 売上データ --query-table-name 売上クエリ --source-path "$assets\売上データ.csv" --destination-address A1 --delimiter , --has-headers true

# 5. XmlMap 追加
& $cli xmlmap add --session $sessionId --schema-file "$assets\XSD_商品マスタ.xsd" --map-name 商品マップ --root-element-name 商品一覧

# 6. セル B2 に値を設定
& $cli range set-values --session $sessionId --sheet 売上データ --range B2 --values '[[1000000]]'

# 7. Scenario 作成
& $cli analysis create-scenario --session $sessionId --sheet 売上データ --scenario-name 楽観シナリオ --changing-cells B2 --values '[1000000]'

# 8. 画像追加
& $cli drawing add-image --session $sessionId --sheet 売上データ --image-path "$assets\画像_テスト.bmp" --name ロゴ画像 --left 10 --top 10 --width 100 --height 100

# 9. SaveAs
& $cli workbook save-as --session $sessionId --target-path "$work\保存テスト_売上.xlsx"

# 10. セッション終了
& $cli session close --session $sessionId --save
```

## 注意点

- すべてのコマンドは UTF-8 エンコーディングで実行してください。PowerShell では `[Console]::OutputEncoding = [System.Text.Encoding]::UTF8` を設定します。
- `--changing-cells` は JSON 配列ではなく、セル範囲文字列（例: `B2` または `B2:B3`）を指定してください。
- 画像ファイルは `画像_テスト.bmp` を使用します。PNG/JPEG でも代替可能です。
- v2.0.0 から `file test` が新設され、Excel を開かずに日本語ファイルパスの有効性を確認できます。

## 検証結果の報告

テスト実施後、`work/test-result.json` と、問題があればエラーメッセージを共有してください。
