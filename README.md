# ExcelMcp (Japanese Support Fork) - MCP Server for Microsoft Excel

> **🇯🇵 Japanese Support Fork** - This is a fork of [sbroenne/mcp-server-excel](https://github.com/sbroenne/mcp-server-excel) with Japanese language support for Excel table names and other identifiers.

[![CI Gate](https://github.com/sbroenne/mcp-server-excel/actions/workflows/ci.yml/badge.svg)](https://github.com/sbroenne/mcp-server-excel/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/v/release/sbroenne/mcp-server-excel)](https://github.com/sbroenne/mcp-server-excel/releases/latest)
[![VS Code Marketplace Installs](https://vsmarketplacebadges.dev/installs-short/sbroenne.excel-mcp.svg?label=VS%20Code%20Installs)](https://marketplace.visualstudio.com/items?itemName=sbroenne.excel-mcp)
[![Downloads](https://img.shields.io/github/downloads/sbroenne/mcp-server-excel/total?label=GitHub%20Downloads)](https://github.com/sbroenne/mcp-server-excel/releases)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10-blue.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)](https://github.com/sbroenne/mcp-server-excel)
[![Fork Status](https://img.shields.io/badge/Fork-Japanese%20Support-green.svg)](https://github.com/matsuzaki-hk/excel-mcp-japanese-support)

**Automate Excel with AI (Japanese Support)** — A Model Context Protocol (MCP) server for comprehensive Excel automation through conversational AI, with full support for Japanese characters in table names, sheet names, and other Excel identifiers.

**AIでExcelを自動化（日本語サポート）** — 会話型AIを通じてExcelを包括的に自動化するModel Context Protocol (MCP) サーバーです。テーブル名、シート名、その他のExcel識別子で日本語文字を完全にサポートしています。

## 🇯🇵 Japanese Support Features / 日本語サポート機能

This fork adds Japanese language support to the original Excel MCP Server:

このフォークは元のExcel MCP Serverに日本語サポートを追加します：

- ✅ **Japanese Table Names** - Create Excel tables with Japanese names (e.g., "売上データ", "生産計画")
  - **日本語テーブル名** - 日本語の名前でExcelテーブルを作成できます（例：「売上データ」「生産計画」）
- ✅ **Unicode Character Support** - Full Unicode support for table names using regex pattern `^[\p{L}_][\p{L}\p{N}_]*$`
  - **Unicode文字サポート** - 正規表現 `^[\p{L}_][\p{L}\p{N}_]*$` を使用してテーブル名に完全なUnicodeサポートを提供
- ✅ **Automatic Upstream Sync** - Daily automatic sync with upstream repository
  - **自動アップストリーム同期** - 毎日自動的に本家リポジトリと同期
- ✅ **Conflict Resolution** - Automatic detection and notification of merge conflicts to preserve Japanese support
  - **競合解決** - 日本語サポートを保持するためのマージ競合の自動検出と通知

### Changes from Original / 元のリポジトリからの変更点

- Modified `src/ExcelMcp.Core/Commands/Table/TableCommands.cs`:
  - Changed table name validation regex from `^[a-zA-Z_][a-zA-Z0-9_]*$` to `^[\p{L}_][\p{L}\p{N}_]*$`
  - Updated error messages to indicate Unicode character support
  - `src/ExcelMcp.Core/Commands/Table/TableCommands.cs` を修正：
    - テーブル名検証正規表現を `^[a-zA-Z_][a-zA-Z0-9_]*$` から `^[\p{L}_][\p{L}\p{N}_]*$` に変更
    - Unicode文字サポートを示すエラーメッセージを更新

### About This Fork / このフォークについて

This is a **fork** of the original [sbroenne/mcp-server-excel](https://github.com/sbroenne/mcp-server-excel) repository maintained by [sbroenne](https://github.com/sbroenne).

これは [sbroenne](https://github.com/sbroenne) が管理する元の [sbroenne/mcp-server-excel](https://github.com/sbroenne/mcp-server-excel) リポジトリの**フォーク**です。

**Purpose / 目的:**
The original Excel MCP Server only supports ASCII characters in table names, which prevents Japanese users from using natural Japanese table names. This fork adds full Unicode support to enable Japanese table names and other identifiers.
元のExcel MCP Serverはテーブル名にASCII文字のみをサポートしており、日本語ユーザーが自然な日本語テーブル名を使用できませんでした。このフォークは完全なUnicodeサポートを追加して、日本語テーブル名やその他の識別子を有効にします。

**Fork Maintenance / フォークの保守:**
- Automatic daily sync with upstream repository (every day 9:00 AM JST)
  - 毎日午前9時（日本時間）に本家リポジトリと自動同期
- Automatic conflict detection with Issue notification to preserve Japanese support changes
  - 日本語サポートの変更を保持するための競合自動検出とIssue通知
- Manual sync also available via GitHub Actions
  - GitHub Actionsを介した手動同期も可能

**Original Author / 原作者:**
[sbroenne](https://github.com/sbroenne) - Original Excel MCP Server creator
[sbroenne](https://github.com/sbroenne) - 元のExcel MCP Server作成者

**MCP Server for Excel** enables AI assistants (GitHub Copilot, Claude, ChatGPT) to automate Excel through natural language commands. Automate Power Query, DAX measures, VBA macros, PivotTables, Charts, formatting, and data transformations (26 tools with 234 operations).

**Excel用MCP Server** は、AIアシスタント（GitHub Copilot、Claude、ChatGPT）が自然言語コマンドでExcelを自動化できるようにします。Power Query、DAXメジャー、VBAマクロ、ピボットテーブル、チャート、書式設定、データ変換を自動化します（26ツール、232操作）。

**⚡ Powered by the real Excel engine** — ExcelMcp drives the actual Excel application through its official COM API, so it does what file-parser tools can't: run live operations (refresh Power Query, recalculate, refresh PivotTables and the Data Model, evaluate DAX, run VBA and Python `=PY()`) and edit your existing workbooks with every formula, PivotTable, chart, macro and format left intact.

**⚡ 実際のExcelエンジンで駆動** — ExcelMcpは公式COM APIを通じて実際のExcelアプリケーションを駆動するため、ファイルパーサーツールにはできない処理を実行できます：Power Queryの更新、再計算、ピボットテーブルとデータモデルの更新、DAXの評価、VBAとPython `=PY()` の実行といったライブ操作を行い、既存のワークブックの数式、ピボットテーブル、チャート、マクロ、書式をすべて保持したまま編集できます。

**💡 Interactive Development** - See results instantly in Excel. Create a query, run it, inspect the output, refine and repeat. Excel becomes your AI-powered workspace for rapid development and testing.

**💡 インタラクティブな開発** - Excelで結果を即座に確認できます。クエリを作成して実行し、出力を検査し、改良して繰り返します。ExcelがAI駆動のワークスペースとなり、迅速な開発とテストが可能になります。

**🧪 LLM-Tested Quality** - Tool behavior validated with real LLM workflows using [pytest-skill-engineering](https://github.com/sbroenne/pytest-skill-engineering). We test that LLMs correctly understand and use our tools.

**🧪 LLMテスト済みの品質** - ツールの動作は[pytest-skill-engineering](https://github.com/sbroenne/pytest-skill-engineering)を使用した実際のLLMワークフローで検証されています。LLMがツールを正しく理解し使用することをテストしています。

> [!TIP]
> **Also building PowerPoint decks?** Check out [PowerPoint MCP Server](https://powerpointmcpserver.dev/) —
> the sister project, built the same way.
> 
> **PowerPointの資料作成もしていますか？** [PowerPoint MCP Server](https://powerpointmcpserver.dev/) をチェックしてみてください — 同じ方法で構築された姉妹プロジェクトです。

**Technical Requirements / 技術要件:**
- ⚠️ **Windows Only** - COM interop is Windows-specific / Windows専用 - COM相互運用はWindows固有です
- ⚠️ **Excel Required** - Microsoft Excel 2016 or later must be installed / Excel必須 - Microsoft Excel 2016以降がインストールされている必要があります
- ⚠️ **Desktop Environment** - Controls actual Excel process (not for server-side processing) / デスクトップ環境 - 実際のExcelプロセスを制御します（サーバー側処理には不適）

## 🎯 What You Can Do / できること

**26 specialized tools with 234 operations:**

- 🔄 **Power Query** (1 tool, 12 ops) - Atomic workflows, M code management, load destinations
- 📊 **Data Model/DAX** (2 tools, 19 ops) - Measures, relationships, model structure
- 🎨 **Excel Tables** (2 tools, 27 ops) - Lifecycle, filtering, sorting, structured references
- 📈 **PivotTables** (3 tools, 30 ops) - Creation, fields, aggregations, calculated members/fields
- 📉 **Charts** (2 tools, 29 ops) - Create, configure, series, formatting, data labels, trendlines
- 📝 **VBA** (1 tool, 6 ops) - Modules, execution, version control
- 📋 **Ranges** (4 tools, 46 ops) - Values, formulas, formatting, validation, protection
- 📄 **Worksheets** (2 tools, 16 ops) - Lifecycle, colors, visibility, cross-workbook moves
- 🔌 **Connections** (1 tool, 9 ops) - OLEDB/ODBC management and refresh
- 🏷️ **Named Ranges** (1 tool, 6 ops) - Parameters and configuration
- 📁 **Files** (1 tool, 6 ops) - Session management, workbook creation, IRM/AIP-protected file support
- 🧮 **Calculation Mode** (1 tool, 3 ops) - Get/set calculation mode and trigger recalculation
- 🎚️ **Slicers** (1 tool, 8 ops) - Interactive filtering for PivotTables and Tables
- 🎨 **Conditional Formatting** (1 tool, 4 ops) - Add, clear, and inspect rules
- 📸 **Screenshot** (1 tool, 2 ops) - Capture ranges/sheets as PNG for LLM visual verification
- 🪧 **Window Management** (1 tool, 9 ops) - Show/hide Excel, arrange, position, status bar feedback

📚 **[Complete Feature Reference →](FEATURES.md)** - Detailed documentation of all 234 operations


## 💬 Example Prompts / プロンプト例

**Create & Populate Data / データの作成と入力:**
- *"Create a new Excel file called SalesTracker.xlsx with a table for Date, Product, Quantity, Unit Price, and Total with sample data"*
- *"Put this data in A1:C4 - Name, Age, City / Alice, 30, Seattle / Bob, 25, Portland"*
- *"Add a formula column that calculates Quantity times Unit Price"*

**Analysis & Visualization / 分析と可視化:**
- *"Create a PivotTable from this data showing total sales by Product, then add a bar chart"*
- *"Use Power Query to import products.csv, load it to the Data Model, and create a measure for Total Revenue"*
- *"Create a slicer for the Region field so I can filter the PivotTable interactively"*
- *"Create a relationship between the Orders and Products tables using ProductID"*

**Formatting & Styling / 書式設定とスタイリング:**
- *"Format the Price column as currency and highlight values over $500 in green"*
- *"Convert this range to an Excel Table with a blue style and add a totals row"*
- *"Make the headers bold with a dark background and auto-fit column widths"*
- *"Apply the same section-header styling to A1:G1, A12:G12, and A24:G24 in one step"*

Formatting split: number display formats use the `range` tool, while visual styling and auto-fit use `range_format`.

**Automation / 自動化:**
- *"Export all Power Query M code to files for version control"*
- *"Run the UpdatePrices macro"*
- *"Show me Excel while you work"* - watch changes in real-time

**🪟 Agent Mode — Watch AI Work in Excel / エージェントモード — AIの作業をExcelで確認:**
- *"Show me Excel side-by-side while you build this dashboard"* - real-time visibility
- *"Let me watch while you create the chart"* - AI asks your preference, then shows Excel
- Status bar shows live progress: *"ExcelMcp: Building PivotTable from Sales data..."*

## 👥 Who Should Use This? / 想定ユーザー

**Perfect for / 適しているユーザー:**
- ✅ **Data analysts** automating repetitive Excel workflows / 繰り返しのExcelワークフローを自動化するデータアナリスト
- ✅ **Developers** building Excel-based data solutions / Excelベースのデータソリューションを構築する開発者
- ✅ **Business users** managing complex Excel workbooks / 複雑なExcelワークブックを管理するビジネスユーザー
- ✅ **Teams** maintaining Power Query/VBA/DAX code in Git / GitでPower Query/VBA/DAXコードを保守するチーム

**Not suitable for / 適していない用途:**
- ❌ Server-side data processing (use libraries like ClosedXML, EPPlus instead) / サーバー側のデータ処理（ClosedXML、EPPlus等のライブラリを使用してください）
- ❌ Linux/macOS users (Windows + Excel installation required) / Linux/macOSユーザー（Windows + Excelのインストールが必要）
- ❌ High-volume batch operations (consider Excel-free alternatives) / 大量のバッチ処理（Excel不要の代替を検討してください）


## 🚀 Quick Start / クイックスタート

| Platform | Installation |
|----------|-------------|
| **VS Code** | [Install Extension](https://marketplace.visualstudio.com/items?itemName=sbroenne.excel-mcp) (one-click, recommended) |
| **Claude Desktop** | Download `.mcpb` from [latest release](https://github.com/sbroenne/mcp-server-excel/releases/latest) |
| **Any MCP Client** | Download `mcp-excel.exe` from [latest release](https://github.com/sbroenne/mcp-server-excel/releases/latest) and add to PATH |
| **Details** | 📖 [MCP Server Installation Guide](docs/INSTALLATION-MCP-SERVER.md) |

**⚠️ Important:** Close all Excel files before using. The server requires exclusive access to workbooks during automation.

**⚠️ 重要:** 使用前にすべてのExcelファイルを閉じてください。サーバーは自動化中にワークブックへの排他アクセスを必要とします。


## 🔧 CLI vs MCP Server / CLIとMCP Serverの比較

This package provides both **CLI** and **MCP Server** interfaces. Choose based on your use case:

このパッケージは**CLI**と**MCP Server**の両方のインターフェースを提供します。ユースケースに応じて選択してください：

| Interface | Best For | Why |
|-----------|----------|-----|
| **CLI** (`excelcli`) | Coding agents (Copilot, Cursor, Windsurf) + Scripting | **64% fewer tokens** - single tool, no large schemas. Auto-generated from Core code, ensuring 1:1 feature parity. Bundled with excel-cli skill. |
| **MCP Server** | Conversational AI (Claude Desktop, VS Code Chat) | Rich tool discovery, persistent connection. Better for interactive, exploratory workflows. |

**Installation / インストール:**
- **CLI via Copilot plugin** (Recommended for Copilot CLI): Install the `excel-cli` plugin for skill guidance, then install `excelcli` separately
- **CLI Standalone**: Download ZIP from [releases](https://github.com/sbroenne/mcp-server-excel/releases/latest) or install via NuGet — see [CLI Installation Guide](docs/INSTALLATION-CLI.md)
- **Skill only**: Install the `excel-cli` skill separately when your agent already has `excelcli` available on PATH
- **MCP Server**: Download from releases or install VS Code Extension — see [MCP Server Installation Guide](docs/INSTALLATION-MCP-SERVER.md)

**⚡ CLI Commands:** Generated automatically from Core service definitions using Roslyn source generators. All CLI commands maintain exact 1:1 parity with MCP tools through shared code generation. See [code generation docs](docs/DEVELOPMENT.md#-cli-command-code-generation) for details.

### 📦 GitHub Copilot Plugins / GitHub Copilot プラグイン

ExcelMcp is available as two **GitHub Copilot CLI plugins** in the Copilot plugin marketplace:

ExcelMcpはCopilotプラグインマーケットプレースで2つの**GitHub Copilot CLIプラグイン**として利用できます：

```powershell
# Register the plugin marketplace (one-time)
copilot plugin marketplace add sbroenne/mcp-server-excel-plugins

# Install one or both plugins
copilot plugin install excel-mcp@mcp-server-excel-plugins      # For conversational AI
copilot plugin install excel-cli@mcp-server-excel-plugins      # For scripting / coding agents
```

- **`excel-mcp`** — MCP server for conversational workflows / 会話型ワークフロー用MCPサーバー
- **`excel-cli`** — Skill for coding agents (install `excelcli` separately if you want the CLI tool) / コーディングエージェント用スキル（CLIツールが必要な場合は`excelcli`を別途インストール）

**Note:** After each release, there may be a short delay before plugins appear in the marketplace. You may need to wait a few moments for updates to sync.

**注意:** 各リリース後、プラグインがマーケットプレースに表示されるまで少し遅延がある場合があります。更新が同期されるまで少しお待ちください。

📖 [Full Installation Guide →](docs/INSTALLATION.md) / [インストールガイド →](docs/INSTALLATION.md)

<details>
<summary>📊 Benchmark Results (same task, same model) / ベンチマーク結果</summary>

| Metric | CLI | MCP Server | Winner |
|--------|-----|------------|--------|
| **Tokens** | ~59K | ~163K | 🏆 CLI (64% fewer) |

**Key insight:** MCP sends 26 tool schemas to the LLM on each request (~100K+ tokens).

</details>

**Manual Installation / 手動インストール:**
```powershell
# Primary: Download standalone executables from latest release (no .NET runtime required)
# https://github.com/sbroenne/mcp-server-excel/releases/latest
# - ExcelMcp-MCP-Server-{version}-windows.zip → extract mcp-excel.exe
# - ExcelMcp-CLI-{version}-windows.zip → extract excelcli.exe (optional, for scripting)

# Secondary: Install via .NET tool (requires .NET 10 runtime)
dotnet tool install --global Sbroenne.ExcelMcp.McpServer
dotnet tool install --global Sbroenne.ExcelMcp.CLI

# After installing either way, auto-configure all your coding agents:
npx add-mcp "mcp-excel" --name excel-mcp
```

> ⚠️ **Step 2 requires [Node.js](https://nodejs.org/)** for `npx`. Install with `winget install OpenJS.NodeJS.LTS` if needed.

```powershell
# Optional: Install agent skills for better AI guidance
npx skills add sbroenne/mcp-server-excel --skill excel-cli   # Coding agents
npx skills add sbroenne/mcp-server-excel --skill excel-mcp   # Conversational AI
```

> 💡 **Skills provide AI guidance** - The CLI skill is highly recommended (agents don't work perfectly with CLI without it). The MCP skill is recommended - it adds workflow best practices and reduces token usage.


## ⚙️ How It Works - COM Automation & Unified Service Architecture / 仕組み - COM自動化と統合サービスアーキテクチャ

**ExcelMcp uses Windows COM automation to control the actual Excel application (not just .xlsx files).**

**ExcelMcpはWindows COM自動化を使用して実際のExcelアプリケーションを制御します（.xlsxファイルだけでなく）。**

The **MCP Server** and **CLI** are two equal, first-class entry points. Each hosts its own **ExcelMCP Service** that manages Excel sessions — the MCP Server runs it **in-process** (direct calls, no pipe), while the CLI uses a **background daemon** over a named pipe so sessions persist across CLI invocations:

**MCP Server**と**CLI**は2つの同等の第一級エントリポイントです。それぞれが独自の**ExcelMCP Service**をホストし、Excelセッションを管理します — MCP Serverは**インプロセス**で実行し（直接呼び出し、パイプなし）、CLIは名前付きパイプ経由で**バックグラウンドデーモン**を使用し、CLI呼び出し間でセッションが永続します：

```
┌──────────────────────┐        ┌──────────────────────┐
│  MCP Server          │        │  CLI (excelcli)      │
│  (AI assistants)     │        │  (coding agents)     │
└──────────┬───────────┘        └──────────┬───────────┘
           │ in-process                     │ named pipe →
           │ (direct calls)                 │ background daemon
           ▼                                ▼
┌──────────────────────┐        ┌──────────────────────┐
│  ExcelMCP Service    │        │  ExcelMCP Service    │
│  (session mgmt)      │        │  (daemon; sessions   │
│                      │        │   persist across     │
│                      │        │   CLI invocations)   │
└──────────┬───────────┘        └──────────┬───────────┘
           ▼                                ▼
      Core Commands                    Core Commands
           ▼                                ▼
┌──────────────────────┐        ┌──────────────────────┐
│  Excel COM API       │        │  Excel COM API       │
│  (Excel.Application) │        │  (Excel.Application) │
└──────────────────────┘        └──────────────────────┘
```

Both entry points share the same Core Commands codebase, so every operation behaves identically. They are separate processes, though: each runs its own ExcelMCP Service and its own Excel instance, and they do **not** share live sessions with each other.

両方のエントリポイントは同じCore Commandsコードベースを共有するため、すべての操作は同一に動作します。ただし、別々のプロセスです：それぞれが独自のExcelMCP ServiceとExcelインスタンスを実行し、ライブセッションを相互に共有**しません**。

**Key Benefits / 主な利点:**
- ✅ **Two equal entry points** - Every operation works identically through the MCP Server and the CLI / 2つの同等のエントリポイント - すべての操作がMCP ServerとCLIで同一に動作
- ✅ **Persistent CLI sessions** - The CLI daemon keeps workbooks open across multiple `excelcli` calls, so scripts don't re-open files each time / 永続的CLIセッション - CLIデーモンが複数の`excelcli`呼び出し間でワークブックを開いたまま保持し、スクリプトが毎回ファイルを開き直す必要がありません
- ✅ **In-process MCP calls** - The MCP Server runs the service in-process (no pipe) for low-latency automation / インプロセスMCP呼び出し - MCP Serverはサービスをインプロセスで実行し（パイプなし）、低レイテンシ自動化を実現
- ✅ **Real Excel automation** - Drives the actual Excel.Application via COM, not just file parsing / 実際のExcel自動化 - ファイル解析だけでなく、COM経由で実際のExcel.Applicationを駆動
- ✅ **System Tray UI** - The CLI daemon shows a tray icon to monitor and stop active sessions / システムトレイUI - CLIデーモンがトレイアイコンを表示し、アクティブセッションの監視と停止が可能

**💡 Tip: Watch Excel While AI Works / ヒント: AIの作業をExcelで確認**
By default, Excel runs hidden for faster automation. To see changes in real-time, just ask:
- *"Show me Excel while you work"*
- *"Let me watch what you're doing"*
- *"Open Excel so I can see the changes"*

The AI will display the Excel window so you can watch every operation happen live - great for learning or verifying changes!

> The AI will display the Excel window so you can watch every operation happen live - great for learning or verifying changes!

## ⭐ GitHub Star History

[![GitHub stars over time for ExcelMcp](https://excelmcpserver.dev/assets/images/star-history.svg)](https://github.com/sbroenne/mcp-server-excel/stargazers)

Updated daily from GitHub's stargazer data.

## 📋 Additional Information / 追加情報

📚 **[CLI Guide →](src/ExcelMcp.CLI/README.md)** | **[CLI Skill for Agents →](skills/excel-cli/SKILL.md)** | **[MCP Server Guide →](src/ExcelMcp.McpServer/README.md)** | **[All Agent Skills →](skills/README.md)**

**License:** MIT License - see [LICENSE](LICENSE) file

**Privacy:** See [PRIVACY.md](PRIVACY.md) for our privacy policy
**Contributing:** See [CONTRIBUTING.md](docs/CONTRIBUTING.md) for guidelines

**Built With:** This entire project was developed using GitHub Copilot AI assistance - mainly with Claude but lately with Auto-mode.

**Acknowledgments / 謝辞:**
- Microsoft Excel Team - For comprehensive COM automation APIs / 包括的なCOM自動化APIを提供してくれたMicrosoft Excelチーム
- Model Context Protocol community - For the AI integration standard / AI統合標準を提供してくれたModel Context Protocolコミュニティ
- Open Source Community - For inspiration and best practices / インスピレーションとベストプラクティスを提供してくれたオープンソースコミュニティ

## Related Projects / 関連プロジェクト

Other projects by the author:
原作者のその他のプロジェクト：

- [PowerPoint MCP Server](https://powerpointmcpserver.dev/) — AI-powered PowerPoint automation via MCP, the sister project to this one
- [pytest-skill-engineering](https://github.com/sbroenne/pytest-skill-engineering) — LLM-powered testing framework for AI agents
- [Windows MCP Server](https://windowsmcpserver.dev/) — AI-powered Windows automation via MCP
- [OBS Studio MCP Server](https://github.com/sbroenne/mcp-server-obs) — AI-powered OBS Studio automation
