---
name: excel-cli-ja
description: >
  Excel CLI automation skill for Windows workbooks. Use when a coding agent needs
  token-efficient, scriptable, or unattended Excel automation via excelcli commands.
  Best for CI/CD, scheduled jobs, batch processing, PowerShell workflows, and bulk
  workbook edits. Supports Power Query, DAX, PivotTables, Tables, Ranges, Charts,
  VBA, Data Models, screenshots, and formatting. Triggers: excelcli, Excel CLI,
  command line, batch, script, automation, CI/CD, scheduled, PowerShell, unattended,
  coding agent, workbook processing.
  This is the Japanese localized version of the skill package.
compatibility: Requires Windows, Microsoft Excel 2016 or later, and network access for first-run runtime download.
---

# excelcli による Excel 自動化（日本語対応フォーク）

## 前提条件

- Windows に Microsoft Excel がインストールされていること（2016 以降）
- COM interop を使用するため、**macOS や Linux では動作しません**
- 以下の各コマンドは `excelcli` を直接呼び出します。`excelcli` が PATH で解決できる必要があります。
  - `excel-cli` プラグインのインストールだけでは PATH に追加されません。`com.github.copilot\bin\install-global.ps1` をインストールされたプラグインフォルダから一度実行すると、`excelcli.cmd` / `excelcli.ps1` が `~\.copilot\bin` に書き込まれ、ユーザー PATH に追加されます。
  - または、スタンドアロン版のリリース ZIP、または `dotnet tool install --global Sbroenne.ExcelMcp.CLI` でランタイムを独立してインストールしてください。
  - `excelcli` が見つからない場合は、パスを推測せず、その旨を報告して停止してください。
- 初回使用時にランタイムがダウンロードされ、`~\.copilot\plugin-runtime\mcp-server-excel\excel-cli` にキャッシュされます。初回のみネットワークアクセスが必要です。

## ワークフローチェックリスト

| 手順 | コマンド | タイミング |
|------|---------|------|
| 1. セッション | `session create/open` | 必ず最初 |
| 2. シート | `sheet create/rename` | 必要な場合 |
| 3. データ書き込み | 下記参照 | 値を書き込む場合 |
| 4. 保存と閉じる | `session close --save` | 必ず最後 |

> **コマンドが 10 個以上？** `excelcli -q batch --input commands.json` を使用してください。単一プロセス内ですべてのコマンドを送信し、セッションを自動管理します。Rule 8 を参照。

**データの書き込み（手順 3）:**
- `--values` は JSON 2D 配列文字列を受け取ります： `--values '[["Header1","Header2"],[1,2]]'`
- 信頼性のため、**1 行ずつ**書き込んでください： `--range A1:B1 --values '[["Name","Age"]]'`
- JSON 内の文字列は必ずダブルクォートで囲みます： `"text"`。数値はそのまま： `42`
- 特殊文字を保護するため、JSON 値全体をシングルクォートで囲みます

## CRITICAL RULES (MUST FOLLOW)

> 以下のルールは、AI エージェントが `excelcli` を正確に呼び出すために厳守すべき技術ルールです。エージェント向けのため英語のまま記載します。

> **⚡ Building dashboards or bulk operations?** Skip to **Rule 8: Batch Mode** — it eliminates per-command process overhead and auto-manages session IDs.

### Rule 1: NEVER Ask Clarifying Questions

Execute commands to discover the answer instead:

| DON'T ASK | DO THIS INSTEAD |
|-----------|-----------------|
| "Which file should I use?" | `excelcli -q session list` |
| "What table should I use?" | `excelcli -q table list --session <id>` |
| "Which sheet has the data?" | `excelcli -q sheet list --session <id>` |

**You have commands to answer your own questions. USE THEM.**

### Rule 2: Always End With a Text Summary

**NEVER end your turn with only a command execution.** After completing all operations, always provide a brief text message confirming what was done. Silent command-only responses are incomplete.

### Rule 3: Session Lifecycle

**Creating vs Opening Files:**
```powershell
# NEW file - use session create
excelcli -q session create C:\path\newfile.xlsx  # Creates file + returns session ID

# EXISTING file - use session open
excelcli -q session open C:\path\existing.xlsx   # Opens file + returns session ID
```

**CRITICAL: Use `session create` for new files. `session open` on non-existent files will fail!**

**CRITICAL: ALWAYS use the session ID returned by `session create` or `session open` in subsequent commands. NEVER guess or hardcode session IDs. The session ID is in the JSON output (e.g., `{"sessionId":"abc123"}`). Parse it and use it.**

```powershell
# Example: capture session ID from output, then use it
excelcli -q session create C:\path\file.xlsx     # Returns JSON with sessionId
excelcli -q range set-values --session <returned-session-id> ...
excelcli -q session close --session <returned-session-id> --save
```

**Unclosed sessions leave Excel processes running, locking files.**

### Rule 4: Data Model Prerequisites

DAX operations require tables in the Data Model:

```powershell
excelcli -q table add-to-data-model --session <id> --table-name Sales  # Step 1
excelcli -q datamodel create-measure --session <id> ...               # Step 2 - NOW works
```

### Rule 5: Power Query Development Lifecycle

**BEST PRACTICE: Test M code before creating permanent queries**

```powershell
# Step 1: Create/open a session and capture the session ID
$session = excelcli -q session create C:\path\file.xlsx | ConvertFrom-Json
$sessionId = $session.sessionId

# Step 2: Test M code without persisting (catches errors early)
excelcli -q powerquery evaluate --session $sessionId --m-code-file query.m

# Step 3: Create permanent query with validated code
excelcli -q powerquery create --session $sessionId --query-name Q1 --m-code-file query.m

# Step 4: Load data to destination
excelcli -q powerquery refresh --session $sessionId --query-name Q1

# Step 5: Close session
excelcli -q session close --session $sessionId --save
```

### Rule 6: Report File Errors Immediately

If you see "File not found" or "Path not found" - STOP and report to user. Don't retry.

### Rule 7: Use Calculation Mode for Bulk Writes

When writing many values/formulas (10+ cells), disable auto-recalc for performance:

```powershell
# 1. Create/open a session and capture the session ID
$session = excelcli -q session create C:\path\file.xlsx | ConvertFrom-Json
$sessionId = $session.sessionId

# 2. Set manual mode
excelcli -q calculationmode set-mode --session $sessionId --mode manual

# 3. Write data row by row for reliability
excelcli -q range set-values --session $sessionId --sheet Sheet1 --range A1:B1 --values '[["Name","Amount"]]'
excelcli -q range set-values --session $sessionId --sheet Sheet1 --range A2:B2 --values '[["Salary",5000]]'

# 4. Recalculate once at end
excelcli -q calculationmode calculate --session $sessionId --scope workbook

# 5. Restore automatic mode
excelcli -q calculationmode set-mode --session $sessionId --mode automatic

# 6. Close session
excelcli -q session close --session $sessionId --save
```

### Rule 8: Use Batch Mode for Bulk Operations (10+ commands)

When executing 10+ commands on the same file, use `excelcli batch` to send all commands in a single process launch. This avoids per-process startup overhead and terminal buffer saturation.

```powershell
# Create a JSON file with all commands
@'
[
  {"command": "session.open", "args": {"filePath": "C:\\path\\file.xlsx"}},
  {"command": "range.set-values", "args": {"sheetName": "Sheet1", "rangeAddress": "A1", "values": [["Hello"]]}},
  {"command": "range.set-values", "args": {"sheetName": "Sheet1", "rangeAddress": "A2", "values": [["World"]]}},
  {"command": "session.close", "args": {"save": true}}
]
'@ | Set-Content commands.json

# Execute all commands at once
excelcli -q batch --input commands.json
```

**Key features:**
- **Session auto-capture**: `session.open`/`create` result sessionId auto-injected into subsequent commands — no need to parse and pass session IDs
- **NDJSON output**: One JSON result per line: `{"index": 0, "command": "...", "success": true, "result": {...}}`
- **`--stop-on-error`**: Exit on first failure (default: continue all)
- **`--session <id>`**: Pre-set session ID for all commands (skip session.open)

**Input formats:**
- JSON array from file: `excelcli -q batch --input commands.json`
- NDJSON from stdin: `Get-Content commands.ndjson | excelcli -q batch`

## CLI コマンドリファレンス

**詳細リファレンス:** [CLI コマンドリファレンスとよくある落とし穴](./references/cli-commands.md)、またはインストール済みランタイムから `excelcli <command> --help` を実行してください。

**構文ルール:** CLI コマンドは `excelcli -q <command> <action> --session <id> --kebab-case-flags ...` を使用します。MCP コール構文 `range(action: ...)` や snake_case パラメータ、下線付きツール名は使用しないでください。CLI コマンド名ではアンダースコアを削除します：`calculation_mode` → `calculationmode`、`range_format` → `rangeformat`、`chart_config` → `chartconfig`、`data_model` → `datamodel`。

利用可能なコマンドグループ:

`session`, `batch`, `service`, `analysis`, `calculationmode`, `chart`, `chartconfig`, `conditionalformat`, `connection`, `datamodel`, `datamodelrelationship`, `drawing`, `namedrange`, `pivottable`, `pivottablecalc`, `pivottablefield`, `powerquery`, `pythoninexcel`, `querytable`, `range`, `rangeedit`, `rangeformat`, `rangelink`, `screenshot`, `sheet`, `worksheetstyle`, `slicer`, `table`, `tablecolumn`, `vba`, `window`, `workbook`, `xmlmap`

## よくある落とし穴

詳細は [CLI コマンドリファレンスとよくある落とし穴](./references/cli-commands.md#common-pitfalls) を参照してください。主なポイント:

- `--values-file` は既存ファイルのパスを期待します。インライン JSON には `--values` を使用します。
- `--timeout` の範囲はアクションによります。`session open/create` は 10-3600；Power Query `refresh`/`refresh-all` は 0-2147483（0 はデフォルトを維持）；その他の生成されたタイムアウトアクションは 1-2147483 です。
- `--values` は 2D JSON 配列を受け取ります。例： `'[["Name","Age"],["Alice",30]]'`
- `--selected-items` などのリストパラメータは JSON 配列が必要です。
- Power Query 操作は 30 秒以上かかることがあります。意図的なデータ操作タイムアウト、または 0（デフォルト）を使用してください。

## 詳細リファレンス

- [CLI コマンドリファレンスとよくある落とし穴](./references/cli-commands.md)
- [Behavioral rules](./references/behavioral-rules.md)
- [Anti-patterns](./references/anti-patterns.md)
- [Common workflows](./references/workflows.md)
- [Ranges](./references/range.md)
- [Worksheets](./references/worksheet.md)
- [Charts](./references/chart.md)
- [Power Query](./references/powerquery.md)
- [Data Model and DAX](./references/datamodel.md)
- [PivotTables](./references/pivottable.md)
- [Tables](./references/table.md)
- [Screenshots](./references/screenshot.md)
- [Window management](./references/window.md)
