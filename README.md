# ExcelMcp (Japanese Support Fork) - MCP Server for Microsoft Excel

> **🇯🇵 日本語対応フォーク** - 本リポジトリは [sbroenne/mcp-server-excel](https://github.com/sbroenne/mcp-server-excel) をフォークし、Excelのテーブル名やその他の識別子に日本語文字をサポートしたものです。

[![CI Gate](https://github.com/sbroenne/mcp-server-excel/actions/workflows/ci.yml/badge.svg)](https://github.com/sbroenne/mcp-server-excel/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/v/release/sbroenne/mcp-server-excel)](https://github.com/sbroenne/mcp-server-excel/releases/latest)
[![VS Code Marketplace Installs](https://vsmarketplacebadges.dev/installs-short/sbroenne.excel-mcp.svg?label=VS%20Code%20Installs)](https://marketplace.visualstudio.com/items?itemName=sbroenne.excel-mcp)
[![Downloads](https://img.shields.io/github/downloads/sbroenne/mcp-server-excel/total?label=GitHub%20Downloads)](https://github.com/sbroenne/mcp-server-excel/releases)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10-blue.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)](https://github.com/sbroenne/mcp-server-excel)
[![Fork Status](https://img.shields.io/badge/Fork-Japanese%20Support-green.svg)](https://github.com/matsuzaki-hk/excel-mcp-japanese-support)

**AIでExcelを自動化（日本語サポート）** — 会話型AIを通じてExcelを包括的に自動化するModel Context Protocol (MCP) サーバーです。テーブル名、シート名、その他のExcel識別子で日本語文字を完全にサポートしています。

<!-- upstream-en: "Japanese Support Features" -->
## 🇯🇵 日本語サポート機能

このフォークは元のExcel MCP Serverに日本語サポートを追加します：

- ✅ **日本語テーブル名** - 日本語の名前でExcelテーブルを作成できます（例：「売上データ」「生産計画」）
- ✅ **Unicode文字サポート** - 正規表現 `^[\p{L}_][\p{L}\p{N}_]*$` を使用してテーブル名に完全なUnicodeサポートを提供
- ✅ **自動アップストリーム同期** - 毎日自動的に本家リポジトリと同期
- ✅ **競合解決** - 日本語サポートを保持するためのマージ競合の自動検出と通知

<!-- upstream-en: "Changes from Original" -->
### 元のリポジトリからの変更点

- `src/ExcelMcp.Core/Commands/Table/TableCommands.cs` を修正：
  - テーブル名検証正規表現を `^[a-zA-Z_][a-zA-Z0-9_]*$` から `^[\p{L}_][\p{L}\p{N}_]*$` に変更
  - Unicode文字サポートを示すエラーメッセージを更新

<!-- upstream-en: "About This Fork" -->
### このフォークについて

これは [sbroenne](https://github.com/sbroenne) が管理する元の [sbroenne/mcp-server-excel](https://github.com/sbroenne/mcp-server-excel) リポジトリの**フォーク**です。

**目的:**
元のExcel MCP Serverはテーブル名にASCII文字のみをサポートしており、日本語ユーザーが自然な日本語テーブル名を使用できませんでした。このフォークは完全なUnicodeサポートを追加して、日本語テーブル名やその他の識別子を有効にします。

**フォークの保守:**
- 毎日午前9時（日本時間）に本家リポジトリと自動同期
- 日本語サポートの変更を保持するための競合自動検出とIssue通知
- GitHub Actionsを介した手動同期も可能

**原作者:**
[sbroenne](https://github.com/sbroenne) - 元のExcel MCP Server作成者

**Excel用MCP Server** は、AIアシスタント（GitHub Copilot、Claude、ChatGPT）が自然言語コマンドでExcelを自動化できるようにします。Power Query、DAXメジャー、VBAマクロ、ピボットテーブル、チャート、書式設定、データ変換を自動化します（26ツール、232操作）。

**⚡ 実際のExcelエンジンで駆動** — ExcelMcpは公式COM APIを通じて実際のExcelアプリケーションを駆動するため、ファイルパーサーツールにはできない処理を実行できます：Power Queryの更新、再計算、ピボットテーブルとデータモデルの更新、DAXの評価、VBAとPython `=PY()` の実行といったライブ操作を行い、既存のワークブックの数式、ピボットテーブル、チャート、マクロ、書式をすべて保持したまま編集できます。

**💡 インタラクティブな開発** - Excelで結果を即座に確認できます。クエリを作成して実行し、出力を検査し、改良して繰り返します。ExcelがAI駆動のワークスペースとなり、迅速な開発とテストが可能になります。

**🧪 LLMテスト済みの品質** - ツールの動作は[pytest-skill-engineering](https://github.com/sbroenne/pytest-skill-engineering)を使用した実際のLLMワークフローで検証されています。LLMがツールを正しく理解し使用することをテストしています。

> [!TIP]
> **PowerPointの資料作成もしていますか？** [PowerPoint MCP Server](https://powerpointmcpserver.dev/) をチェックしてみてください — 同じ方法で構築された姉妹プロジェクトです。

**技術要件:**
- ⚠️ **Windows専用** - COM相互運用はWindows固有です
- ⚠️ **Excel必須** - Microsoft Excel 2016以降がインストールされている必要があります
- ⚠️ **デスクトップ環境** - 実際のExcelプロセスを制御します（サーバー側処理には不適）

<!-- upstream-en: "What You Can Do" -->
## 🎯 できること

**26の専門ツール、234の操作：**

-  **Power Query**（1ツール、12操作） — ワークフロー、Mコード管理、読み込み先
- 📊 **Data Model/DAX**（2ツール、19操作） — メジャー、リレーションシップ、モデル構造
- 🎨 **Excel Tables**（2ツール、27操作） — ライフサイクル、フィルタリング、並べ替え、構造化参照
- 📈 **PivotTables**（3ツール、30操作） — 作成、フィールド、集計、計算メンバー/フィールド
- 📉 **Charts**（2ツール、29操作） — 作成、構成、系列、書式、データラベル、トレンドライン
- 📝 **VBA**（1ツール、6操作） — モジュール、実行、バージョン管理
- 📋 **Ranges**（4ツール、46操作） — 値、数式、書式、入力規則、保護
- 📄 **Worksheets**（2ツール、16操作） — ライフサイクル、色、表示、ブック間移動
- 🔌 **Connections**（1ツール、9操作） — OLEDB/ODBC管理と更新
- 🏷️ **Named Ranges**（1ツール、6操作） — パラメーターと構成
- 📁 **Files**（1ツール、6操作） — セッション管理、ブック作成、IRM/AIP保護ファイル対応
- 🧮 **Calculation Mode**（1ツール、3操作） — 計算モードの取得/設定と再計算のトリガー
- 🎚️ **Slicers**（1ツール、8操作） — ピボットテーブルとテーブルの対話的フィルタリング
- 🎨 **Conditional Formatting**（1ツール、4操作） — 追加、クリア、ルールの検査
- 📸 **Screenshot**（1ツール、2操作） — 範囲/シートをPNGでキャプチャしてLLMの視覚的検証に使用
- 🪧 **Window Management**（1ツール、9操作） — Excelの表示/非表示、配置、位置、ステータスバーへのフィードバック

📚 **[完全な機能リファレンス →](FEATURES.md)** - 234の操作の詳細なドキュメント


<!-- upstream-en: "Example Prompts" -->
## 💬 プロンプト例

**データの作成と入力：**
- *"SalesTracker.xlsx という新しいExcelファイルを作成し、日付、商品、数量、単価、合計のテーブルにサンプルデータを入れて"*
- *"A1:C4 に 名前、年齢、都市 / Alice, 30, Seattle / Bob, 25, Portland のデータを入力して"*
- *"数量×単価を計算する数式列を追加して"*

**分析と可視化：**
- *"このデータから商品別の合計売上を示すピボットテーブルを作成し、棒グラフを追加して"*
- *"Power Query を使って products.csv をインポートし、データモデルに読み込んで、Total Revenue のメジャーを作成して"*
- *"Region フィールドのスライサーを作成して、ピボットテーブルを対話的にフィルタリングできるようにして"*
- *"ProductID を使って Orders テーブルと Products テーブルにリレーションシップを作成して"*

**書式設定とスタイリング：**
- *"Price 列を通貨形式にして、500ドル以上を緑色で強調表示して"*
- *"この範囲を青色のスタイルのExcelテーブルに変換し、集計行を追加して"*
- *"ヘッダーを太字にし、背景を暗くして、列幅を自動調整して"*
- *"同じセクション見出しのスタイルを A1:G1、A12:G12、A24:G24 に一括適用して"*

書式設定の分担：数値表示書式は `range` ツールを、視覚的スタイルと幅自動調整は `range_format` を使用します。

**自動化：**
- *"すべてのPower Query Mコードをファイルにエクスポートしてバージョン管理に備えて"*
- *"UpdatePrices マクロを実行して"*
- *"作業中にExcelを表示して"* — リアルタイムで変更を確認

**🪟 エージェントモード — AIの作業をExcelで確認：**
- *"このダッシュボードを作成しながらExcelを横に表示して"* — リアルタイムの可視性
- *"チャートを作成するところを見させて"* — AIが好みを確認してからExcelを表示
- ステータスバーに *"ExcelMcp: Sales データからピボットテーブルを構築中..."* のようなライブ進捗を表示

<!-- upstream-en: "Who Should Use This?" -->
## 👥 想定ユーザー

**適しているユーザー：**
- ✅ **データアナリスト** — 繰り返しのExcelワークフローを自動化
- ✅ **開発者** — Excelベースのデータソリューションを構築
- ✅ **ビジネスユーザー** — 複雑なExcelワークブックを管理
- ✅ **チーム** — GitでPower Query/VBA/DAXコードを保守

**適していない用途：**
- ❌ サーバー側のデータ処理（ClosedXML、EPPlus等のライブラリを使用してください）
- ❌ Linux/macOSユーザー（Windows + Excelのインストールが必要）
- ❌ 大量のバッチ処理（Excel不要の代替を検討してください）


<!-- upstream-en: "Quick Start" -->
## 🚀 クイックスタート

| 入手方法 | インストール |
|----------|-------------|
| **MCP Server** | [最新リリース](https://github.com/matsuzaki-hk/excel-mcp-japanese-support/releases/latest) から `ExcelMcp-MCP-Server-{version}-windows.zip` をダウンロードし、`mcp-excel.exe` を展開 |
| **CLI** | [最新リリース](https://github.com/matsuzaki-hk/excel-mcp-japanese-support/releases/latest) から `ExcelMcp-CLI-{version}-windows.zip` をダウンロードし、`excelcli.exe` を展開 |
| **詳細** | フォークの [リリースページ](https://github.com/matsuzaki-hk/excel-mcp-japanese-support/releases) を参照 |

**⚠️ 重要:** 使用前にすべてのExcelファイルを閉じてください。サーバーは自動化中にワークブックへの排他アクセスを必要とします。


<!-- upstream-en: "CLI vs MCP Server" -->
## 🔧 CLIとMCP Serverの比較

このパッケージは**CLI**と**MCP Server**の両方のインターフェースを提供します。ユースケースに応じて選択してください：

| インターフェース | 最適な用途 | 理由 |
|-----------|----------|-----|
| **CLI** (`excelcli`) | コーディングエージェント（Copilot、Cursor、Windsurf）＋スクリプト | **トークン削減64%** — 単一ツールで大きなスキーマなし。Coreコードから自動生成され、1:1の機能整合を保証。excel-cli skillと同梱。 |
| **MCP Server** | 会話型AI（Claude Desktop、VS Code Chat） | 豊富なツール発見、永続的接続。対話的・探索的なワークフローに最適。 |

**インストール：**
- **MCP Server スタンドアロン**：[フォーク最新リリース](https://github.com/matsuzaki-hk/excel-mcp-japanese-support/releases/latest) から `ExcelMcp-MCP-Server-{version}-windows.zip` をダウンロードし、`mcp-excel.exe` を PATH の通ったフォルダに展開
- **CLI スタンドアロン**：[フォーク最新リリース](https://github.com/matsuzaki-hk/excel-mcp-japanese-support/releases/latest) から `ExcelMcp-CLI-{version}-windows.zip` をダウンロードし、`excelcli.exe` を PATH の通ったフォルダに展開
- 本家では VS Code 拡張、Claude Desktop (`.mcpb`)、NuGet、Copilot プラグイン等も提供されていますが、**本フォークは上記スタンドアロン実行ファイルのみをリリースしています**

**⚡ CLIコマンド：** Coreサービス定義からRoslynソースジェネレーターで自動生成されます。すべてのCLIコマンドは共有コード生成を通じてMCPツールと正確に1:1の整合性を維持します。詳細は[code generation docs](docs/DEVELOPMENT.md#-cli-command-code-generation)を参照してください。

### MCP クライアント設定

`mcp-excel.exe` は標準入出力（stdio）で動作します。MCP クライアントに登録する方法は 2 通りです。

#### A. フルパスで指定する方法

ZIP を任意の場所（例：`C:\Tools\ExcelMcp`）に展開し、MCP クライアントの設定ファイルの `command` にフルパスを指定してください。

```json
{
  "mcpServers": {
    "excel-mcp": {
      "command": "C:\\Tools\\ExcelMcp\\mcp-excel.exe",
      "args": []
    }
  }
}
```

#### B. ファイル名だけで指定する方法（PATH を通す）

展開先フォルダを PATH に追加すると、`command` に `mcp-excel.exe` だけを指定できます。

**B-1. スタートメニューから設定する**

1. Windows キーを押して `環境変数` と入力
2. `システム環境変数の編集` または `環境変数を編集` を選択
3. 開いた `システムのプロパティ` ウィンドウで `詳細設定` タブを開き、`環境変数(N)` ボタンをクリック
4. `ユーザー環境変数`（または `システム環境変数`）エリアで `Path` を選択し、`編集` をクリック
5. `新規` をクリックし、`mcp-excel.exe` を展開したフォルダのパス（例：`C:\Tools\ExcelMcp`）を追加
6. `OK` を複数回押してウィンドウを閉じる
7. 新しい PowerShell / ターミナルを開き、`mcp-excel.exe` と入力してパスが通っているか確認

**B-2. PowerShell コマンドから設定する**

```powershell
# 例：C:\Tools\ExcelMcp を PATH に追加（ユーザー環境変数）
[Environment]::SetEnvironmentVariable("Path", "$env:Path;C:\Tools\ExcelMcp", "User")
```

新しい PowerShell / ターミナルで `mcp-excel.exe` が実行できることを確認してから、以下のように設定してください。

```json
{
  "mcpServers": {
    "excel-mcp": {
      "command": "mcp-excel.exe",
      "args": []
    }
  }
}
```

> **注意:** 設定変更後は MCP クライアントの再起動が必要です。

### 📦 本家でのその他の配布形態

本家 [sbroenne/mcp-server-excel](https://github.com/sbroenne/mcp-server-excel) では以下も提供されていますが、**本フォークはスタンドアロン実行ファイルのみをリリースしています**。

- **VS Code 拡張**（`.vsix` / Marketplace）
- **Claude Desktop バンドル**（`.mcpb`）
- **NuGet (.NET Tool)**：`Sbroenne.ExcelMcp.*`
- **GitHub Copilot プラグイン**：`sbroenne/mcp-server-excel-plugins`
- **Agent Skills**：`npx skills add sbroenne/mcp-server-excel --skill excel-cli|excel-mcp`

これらの詳細は [本家リリースページ](https://github.com/sbroenne/mcp-server-excel/releases) を参照してください。

<details>
<summary>📊 ベンチマーク結果</summary>

| 指標 | CLI | MCP Server | 勝者 |
|--------|-----|------------|--------|
| **トークン数** | ~59K | ~163K | 🏆 CLI（64%削減） |

**重要なポイント：** MCPは各リクエストで26個のツールスキーマをLLMに送信します（約10万トークン以上）。

</details>

**手動インストール：**
```powershell
# 1. 最新リリースからスタンドアロン実行ファイルをダウンロード
# https://github.com/matsuzaki-hk/excel-mcp-japanese-support/releases/latest
# - ExcelMcp-MCP-Server-{version}-windows.zip → mcp-excel.exe を展開
# - ExcelMcp-CLI-{version}-windows.zip       → excelcli.exe を展開（オプション）

# 2. 任意のフォルダに展開し、必要に応じて PATH を通す
# 例: C:\Tools\ExcelMcp
```

> ⚠️ **本フォークはスタンドアロン実行ファイルのみを提供しています。** 本家の VS Code 拡張、Claude Desktop (`.mcpb`)、NuGet、Copilot プラグイン等は本家リリースを参照してください。


<!-- upstream-en: "How It Works - COM Automation & Unified Service Architecture" -->
## ⚙️ 仕組み - COM自動化と統合サービスアーキテクチャ

ExcelMcpはWindows COM自動化を使用して実際のExcelアプリケーションを制御します（.xlsxファイルだけでなく）。

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

両方のエントリポイントは同じCore Commandsコードベースを共有するため、すべての操作は同一に動作します。ただし、別々のプロセスです：それぞれが独自のExcelMCP ServiceとExcelインスタンスを実行し、ライブセッションを相互に共有**しません**。

**主な利点：**
- ✅ **2つの同等のエントリポイント** - すべての操作がMCP ServerとCLIで同一に動作
- ✅ **永続的CLIセッション** - CLIデーモンが複数の`excelcli`呼び出し間でワークブックを開いたまま保持し、スクリプトが毎回ファイルを開き直す必要がありません
- ✅ **インプロセスMCP呼び出し** - MCP Serverはサービスをインプロセスで実行し（パイプなし）、低レイテンシ自動化を実現
- ✅ **実際のExcel自動化** - ファイル解析だけでなく、COM経由で実際のExcel.Applicationを駆動
- ✅ **システムトレイUI** - CLIデーモンがトレイアイコンを表示し、アクティブセッションの監視と停止が可能

**💡 ヒント: AIの作業をExcelで確認**

既定では、高速な自動化のためExcelは非表示で動作します。リアルタイムで変更を確認するには、次のように頼んでください：
- *"作業中にExcelを表示して"*
- *"何をしているか見せて"*
- *"変更を確認できるようにExcelを開いて"*

AIがExcelウィンドウを表示するため、すべての操作がライブで確認できます — 学習や変更の確認に最適です！

> AIがExcelウィンドウを表示するため、すべての操作がライブで確認できます — 学習や変更の確認に最適です！


<!-- upstream-en: "Additional Information" -->
## 📋 追加情報

📚 **[CLIガイド →](src/ExcelMcp.CLI/README.md)** | **[エージェント向けCLI Skill →](skills/excel-cli/SKILL.md)** | **[MCP Serverガイド →](src/ExcelMcp.McpServer/README.md)** | **[すべてのエージェントSkill →](skills/README.md)**

**ライセンス：** MIT License - [LICENSE](LICENSE)ファイルを参照

**プライバシー：** プライバシーポリシーは [PRIVACY.md](PRIVACY.md) を参照
**貢献：** ガイドラインは [CONTRIBUTING.md](docs/CONTRIBUTING.md) を参照

**使用した技術：** 本プロジェクトは、Claudeを中心に最近はAuto-modeも活用したGitHub CopilotのAI支援で開発されました。

**謝辞：**
- Microsoft Excel Team - 包括的なCOM自動化APIを提供してくれたMicrosoft Excelチーム
- Model Context Protocol community - AI統合標準を提供してくれたModel Context Protocolコミュニティ
- Open Source Community - インスピレーションとベストプラクティスを提供してくれたオープンソースコミュニティ

<!-- upstream-en: "Related Projects" -->
## 関連プロジェクト

原作者のその他のプロジェクト：

- [PowerPoint MCP Server](https://powerpointmcpserver.dev/) — MCP経由でAIがPowerPointを自動化する姉妹プロジェクト
- [pytest-skill-engineering](https://github.com/sbroenne/pytest-skill-engineering) — AIエージェント向けのLLM駆動テストフレームワーク
- [Windows MCP Server](https://windowsmcpserver.dev/) — MCP経由でAIがWindowsを自動化
- [OBS Studio MCP Server](https://github.com/sbroenne/mcp-server-obs) — MCP経由でAIがOBS Studioを自動化
