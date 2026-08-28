---
name: excel-mcp-ja
description: >
  Excel MCP Server skill for Windows workbook automation. Use when an assistant
  needs rich MCP tools to create, inspect, modify, format, or analyze Excel files.
  Supports Power Query (M), Data Model/DAX, PivotTables, Tables, Ranges, Charts,
  Slicers, formatting, screenshots, VBA macros, connections, and calculation mode.
  Triggers: Excel, spreadsheet, workbook, xlsx, xlsm, Power Query, DAX, PivotTable,
  chart, dashboard, VBA, MCP.
  This is the Japanese localized version of the skill package.
compatibility: Requires Windows, Microsoft Excel 2016 or later, and network access for first-run runtime download.
---

# Excel MCP Server Skill（日本語対応フォーク）

Model Context Protocol（MCP）を介して、Windows 上の Excel を自動化する 326 種類の操作を提供します。MCP Server は ExcelMCP サービスを同一プロセス内でホストし、低レイテンシーで Excel 操作を実行します。ツールは自動検出されます。本ドキュメントでは、日本語環境特有の注意点、推奨ワークフロー、よくある落とし穴をまとめています。

## ワークフローチェックリスト

| 手順 | ツール | アクション | タイミング |
|------|------|--------|------|
| 1. ファイルを開く | `file` | `open` または `create` | 必ず最初 |
| 2. シートを作成 | `worksheet` | `create`, `rename` | 必要な場合 |
| 3. データを書き込む | `range` | `set-values` | 必須（2D 配列） |
| 4. 書式を設定 | `range` | `set-number-format` | 書き込み後 |
| 5. 構造化 | `table` | `create` | データをテーブル化 |
| 6. 保存して閉じる | `file` | `close` with `save: true` | 必ず最後 |

## 前提条件

- Windows に Microsoft Excel がインストールされていること（2016 以降）
- フルパスを使用すること： `C:\Users\名前\Documents\レポート.xlsx`
- 対象の Excel ファイルが他の Excel インスタンスで開かれていないこと
- 日本語ファイル名・パス・シート名・テーブル名が利用可能（本家フォークでは ASCII 制限があります）

## 計算モードワークフロー（一括書き込みのパフォーマンス最適化）

大量の値や数式を書き込む場合は、自動再計算を無効にしてセルごとの再計算を避け、最後に一括計算することでパフォーマンスを最適化します。

```
1. calculation_mode(action: 'set-mode', mode: 'manual')  → 自動再計算を無効化
2. すべての書き込みを実行（range set-values, set-formulas）
3. calculation_mode(action: 'calculate', scope: 'workbook')  → 一度だけ再計算
4. calculation_mode(action: 'set-mode', mode: 'automatic')  → デフォルトに戻す
```

**注:** 数式を読み取る場合は手動モードにする必要はありません。`range get-formulas` は計算モードに関係なく数式テキストを返します。

## CRITICAL: Execution Rules (MUST FOLLOW)

> 以下のルールは、AI エージェントが ExcelMcp のツールを正確に呼び出すために厳守すべき技術ルールです。エージェント向けのため英語のまま記載します。

### Rule 1: NEVER Ask Clarifying Questions

**STOP.** If you're about to ask "Which file?", "What table?", "Where should I put this?" - DON'T.

| Bad (Asking) | Good (Discovering) |
|--------------|-------------------|
| "Which Excel file should I use?" | `file(list)` → use the open session |
| "What's the table name?" | `table(list)` → discover tables |
| "Which sheet has the data?" | `worksheet(list)` → check all sheets |
| "Should I create a PivotTable?" | YES - create it on a new sheet |

**You have tools to answer your own questions. USE THEM.**

### Rule 2: Always End With a Text Summary

**NEVER end your turn with only a tool call.** After completing all operations, always provide a brief text message confirming what was done. Silent tool-call-only responses are incomplete.

### Rule 3: Format Data Professionally

Always apply number formats after setting values:

| Data Type | Format Code | Result |
|-----------|-------------|--------|
| USD | `$#,##0.00` | $1,234.56 |
| EUR | `€#,##0.00` | €1,234.56 |
| Percent | `0.00%` | 15.00% |
| Date (ISO) | `yyyy-mm-dd` | 2025-01-22 |

Write format codes in US notation (`,` grouping, `.` decimal) regardless of the machine's
locale — Excel translates them. The **rendered** separators follow the user's Windows regional
settings, so `$#,##0.00` shows `$1.234,56` on a German system. Don't "fix" that by swapping the
separators in the format code; it would break on every other locale.

**Workflow:**
```
1. range set-values (data is now in cells)
2. range set-number-format (apply format)
3. range_format auto-fit-columns (formatted values are wider than raw ones)
```

Step 3 is not optional. A column sized for `45678` is too narrow once that value renders as
`2025-01-22` or `$1,234.56`, and Excel displays `#####` instead of the number.

### Rule 4: Use Excel Tables (Not Plain Ranges)

Always convert tabular data to Excel Tables:

```
1. range set-values (write data including headers)
2. table create tableName="SalesData" rangeAddress="A1:D100"
```

**Why:** Structured references, auto-expand, required for Data Model/DAX.

### Rule 5: Session Lifecycle

```
1. file(action: 'open', path: '...')  → sessionId
2. All operations use sessionId
3. file(action: 'close', save: true)  → saves and closes
```

**Unclosed sessions leave Excel processes running, locking files.**

### Rule 6: Data Model Prerequisites

DAX operations require tables in the Data Model:

```
Step 1: Create table → Table exists
Step 2: table(action: 'add-to-datamodel') → Table in Data Model
Step 3: datamodel(action: 'create-measure') → NOW this works
```

### Rule 7: Power Query Development Lifecycle

**BEST PRACTICE: Test-First Workflow**

```
1. powerquery(action: 'evaluate', mCode: '...') → Test WITHOUT persisting
2. powerquery(action: 'create', ...) → Store validated query
3. powerquery(action: 'refresh', ...) → Load data
```

**Why evaluate first:**
- Catches syntax errors and missing sources BEFORE creating permanent queries
- Better error messages than COM exceptions from create/update
- See actual data preview (columns + sample rows)
- No cleanup needed - like a REPL for M code
- Skip only for trivial literal tables

**Common mistake:** Creating/updating without evaluate → pollutes workbook with broken queries

### Rule 8: Targeted Updates Over Delete-Rebuild

- **Prefer**: `set-values` on specific range (e.g., `A5:C5` for row 5)
- **Avoid**: Deleting and recreating entire structures

**Why:** Preserves formatting, formulas, and references.

### Rule 9: Follow suggestedNextActions

Error responses include actionable hints:
```json
{
  "success": false,
  "errorMessage": "Table 'Sales' not found in Data Model",
  "suggestedNextActions": ["table(action: 'add-to-data-model', tableName: 'Sales')"]
}
```

## ツール選択クイックリファレンス

| 作業 | ツール | 主要アクション |
|------|------|------------|
| ブックの作成/開く/保存 | `file` | open, create, close |
| セルデータの読み書き | `range` | set-values, get-values |
| セルの書式設定 | `range` | set-number-format |
| データからテーブル作成 | `table` | create |
| Power Pivot へのテーブル追加 | `table` | add-to-data-model |
| DAX 数式の作成 | `datamodel` | create-measure |
| ピボットテーブル作成 | `pivottable` | create, create-from-datamodel |
| スライサーによるフィルタ | `slicer` | set-slicer-selection |
| グラフ作成 | `chart` | create-from-range |
| シミュレーション・what-if 分析 | `analysis` | goal-seek, create-scenario, create-data-table |
| 計算モードの制御 | `calculation_mode` | get-mode, set-mode, calculate |
| 視覚的確認 | `screenshot` | capture, capture-sheet |

## 詳細リファレンス

詳細なガイドラインは `references/` ディレクトリを参照してください。

- [What-if 分析と Solver の制限](./references/analysis.md)
- [コア実行ルールと LLM ガイドライン](./references/behavioral-rules.md)
- [避けるべきよくある間違い](./references/anti-patterns.md)
- [一括書き込みのパフォーマンス最適化](./references/calculation.md)
- [Data Model の制約とパターン](./references/workflows.md)
- [グラフと書式](./references/chart.md)
- [条件付き書式の操作](./references/conditionalformat.md)
- [ダッシュボードとレポートのベストプラクティス](./references/dashboard.md)
- [Data Model / DAX の詳細](./references/datamodel.md)
- [Data Model 分析用 DMV リファレンス](./references/dmv-reference.md)
- [Excel エージェントモードと高度な自動化](./references/excel_agent_mode.md)
- [よくある落とし穴と既知の制限](./references/gotchas.md)
- [Power Query M コード構文リファレンス](./references/m-code-syntax.md)
- [ピボットテーブル操作](./references/pivottable.md)
- [Power Query の詳細](./references/powerquery.md)
- [Range 操作と数値書式](./references/range.md)
- [スクリーンショットと視覚的確認](./references/screenshot.md)
- [スライサー操作](./references/slicer.md)
- [テーブル操作](./references/table.md)
- [ウィンドウと表示操作](./references/window.md)
- [ワークシート操作](./references/worksheet.md)
