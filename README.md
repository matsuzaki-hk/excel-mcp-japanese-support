# ExcelMcp (Japanese Support Fork) - MCP Server for Microsoft Excel

> **🇯🇵 Japanese Support Fork** - This is a fork of [sbroenne/mcp-server-excel](https://github.com/sbroenne/mcp-server-excel) with Japanese language support for Excel table names and other identifiers.

[![VS Code Marketplace Installs](https://img.shields.io/visual-studio-marketplace/i/sbroenne.excel-mcp?label=VS%20Code%20Installs)](https://marketplace.visualstudio.com/items?itemName=sbroenne.excel-mpc)
[![Downloads](https://img.shields.io/github/downloads/sbroenne/mcp-server-excel/total?label=GitHub%20Downloads)](https://github.com/sbroenne/mcp-server-excel/releases/latest)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10-blue.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)](https://github.com/sbroenne/mcp-server-excel)
[![Fork Status](https://img.shields.io/badge/Fork-Japanese%20Support-green.svg)](https://github.com/matsuzaki-hk/mcp-server-excel)

**Automate Excel with AI (Japanese Support)** — A Model Context Protocol (MCP) server for comprehensive Excel automation through conversational AI, with full support for Japanese characters in table names, sheet names, and other Excel identifiers.

## 🇯🇵 Japanese Support Features / 日本語サポート機能

This fork adds Japanese language support to the original Excel MCP Server:

このフォークは元のExcel MCP Serverに日本語サポートを追加します：

- ✅ **Japanese Table Names** - Create Excel tables with Japanese names (e.g., "売上データ", "生産計画")
  - **日本語テーブル名** - 日本語の名前でExcelテーブルを作成できます（例：「売上データ」「生産計画」）
- ✅ **Unicode Character Support** - Full Unicode support for table names using regex pattern `^[\p{L}_][\p{L}\p{N}_]*$`
  - **Unicode文字サポート** - 正規表現 `^[\p{L}_][\p{L}\p{N}_]*$` を使用してテーブル名に完全なUnicodeサポートを提供
- ✅ **Japanese SKILL Files** - All SKILL files translated to Japanese for better Japanese user experience
  - **日本語SKILLファイル** - すべてのSKILLファイルを日本語化し、日本語ユーザーの体験を向上
- ✅ **Branch Structure** - Proper branch structure to track upstream updates while maintaining Japanese support
  - **ブランチ構造** - 本家の更新を追跡しつつ日本語サポートを維持するための適切なブランチ構造
- ✅ **Upstream Sync** - Manual sync with upstream repository using rebase strategy
  - **アップストリーム同期** - リベース戦略を使用して本家リポジトリと手動同期

### Changes from Original / 元のリポジトリからの変更点

- Modified `src/ExcelMcp.Core/Commands/Table/TableCommands.cs`:
  - Changed table name validation regex from `^[a-zA-Z_][a-zA-Z0-9_]*$` to `^[\p{L}_][\p{L}\p{N}_]*$`
  - Updated error messages to indicate Unicode character support
  - Added automatic sync workflow with conflict resolution
  - `src/ExcelMcp.Core/Commands/Table/TableCommands.cs` を修正：
    - テーブル名検証正規表現を `^[a-zA-Z_][a-zA-Z0-9_]*$` から `^[\p{L}_][\p{L}\p{N}_]*$` に変更
    - Unicode文字サポートを示すエラーメッセージを更新
    - 競合解決付きの自動同期ワークフローを追加

### About This Fork / このフォークについて

This is a **fork** of the original [sbroenne/mcp-server-excel](https://github.com/sbroenne/mcp-server-excel) repository maintained by [sbroenne](https://github.com/sbroenne).

これは [sbroenne](https://github.com/sbroenne) が管理する元の [sbroenne/mcp-server-excel](https://github.com/sbroenne/mcp-server-excel) リポジトリの**フォーク**です。

**Purpose / 目的:**
The original Excel MCP Server only supports ASCII characters in table names, which prevents Japanese users from using natural Japanese table names. This fork adds full Unicode support to enable Japanese table names and other identifiers.
元のExcel MCP Serverはテーブル名にASCII文字のみをサポートしており、日本語ユーザーが自然な日本語テーブル名を使用できませんでした。このフォークは完全なUnicodeサポートを追加して、日本語テーブル名やその他の識別子を有効にします。

**Fork Maintenance / フォークの保守:**

### Branch Structure / ブランチ構造

This fork uses a proper branch structure to track upstream updates while maintaining Japanese support:

このフォークは日本語サポートを維持しつつ本家の更新を追跡するために適切なブランチ構造を使用します：

- `main` - Upstream tracking branch (English only) / 本家追従用ブランチ（英語のみ）
- `ja-localization` - Japanese localization branch (includes Japanese SKILL files) / 日本語化ブランチ（日本語SKILLファイルを含む）
- `feature/*` - Feature development branches / 機能開発ブランチ

### Upstream Sync Workflow / アップストリーム同期ワークフロー

To sync with upstream updates:

本家の更新と同期するには：

```bash
# 1. Fetch upstream changes / アップストリームの変更を取得
git fetch upstream

# 2. Reset main to upstream/main / mainをupstream/mainにリセット
git checkout main
git reset --hard upstream/main

# 3. Rebase ja-localization on main / ja-localizationをmainにリベース
git checkout ja-localization
git rebase main

# 4. Resolve conflicts if needed / 必要に応じて競合を解決
# - New files added by upstream: Japanese localization required / 本家が追加した新しいファイル：日本語化が必要
# - Existing files modified by upstream: Review and update Japanese translation if needed / 本家が変更した既存ファイル：必要に応じて日本語訳を更新

# 5. Push changes / 変更をプッシュ
git push origin ja-localization --force-with-lease
```

### Adding New Features / 新機能の追加

To add new features while maintaining Japanese support:

日本語サポートを維持しつつ新機能を追加するには：

```bash
# 1. Create feature branch from ja-localization / ja-localizationから機能ブランチを作成
git checkout ja-localization
git checkout -b feature/new-feature

# 2. Develop feature / 機能を開発
# - Implement new functionality / 新しい機能を実装
# - Update SKILL files if needed / 必要に応じてSKILLファイルを更新
# - Japanese localization if applicable / 該当する場合は日本語化

# 3. Commit changes / 変更をコミット
git add .
git commit -m "feat: new feature description"

# 4. Sync with upstream / 本家と同期
git checkout main
git fetch upstream
git reset --hard upstream/main

git checkout ja-localization
git rebase main

git checkout feature/new-feature
git rebase ja-localization

# 5. Merge to ja-localization / ja-localizationにマージ
git checkout ja-localization
git merge feature/new-feature

# 6. Push / プッシュ
git push origin ja-localization --force-with-lease
```

**Original Author / 原作者:**
[sbroenne](https://github.com/sbroenne) - Original Excel MCP Server creator
[sbroenne](https://github.com/sbroenne) - 元のExcel MCP Server作成者

**MCP Server for Excel**は、自然言語コマンドを通じてAIアシスタント（GitHub Copilot、Claude、ChatGPT）にExcelの自動化を可能にします。Power Query、DAXメジャー、VBAマクロ、PivotTables、チャート、フォーマット、データ変換を自動化します（25ツール、230操作）。

**🛡️ 100%安全 - ExcelのネイティブCOM APIを使用** - ファイル破損のリスクはゼロです。`.xlsx`ファイルを直接操作するサードパーティライブラリとは異なり、このプロジェクトはExcelの公式APIを使用し、完全な安全性と互換性を確保します。

**💡 インタラクティブ開発** - Excelで結果を即座に確認できます。クエリを作成し、実行し、出力を検査し、改良を繰り返します。ExcelがAI搭載ワークスペースとなり、迅速な開発とテストが可能になります。

**🧪 LLMテスト済みの品質** - [pytest-skill-engineering](https://github.com/sbroenne/pytest-skill-engineering)を使用して実際のLLMワークフローでツール動作を検証しています。LLMがツールを正しく理解して使用することをテストしています。

**技術要件:**
- ⚠️ **Windowsのみ** - COM相互運用はWindows固有です
- ⚠️ **Excel必須** - Microsoft Excel 2016以降がインストールされている必要があります
- ⚠️ **デスクトップ環境** - 実際のExcelプロセスを制御します（サーバーサイド処理向けではありません）

## 🎯 可能なこと

**25の専門ツールと230操作:**

- 🔄 **Power Query** (1ツール、12操作) - アトミックワークフロー、Mコード管理、ロード先
- 📊 **Data Model/DAX** (2ツール、19操作) - メジャー、リレーションシップ、モデル構造
- 🎨 **Excelテーブル** (2ツール、27操作) - ライフサイクル、フィルタリング、ソート、構造化参照
- 📈 **PivotTables** (3ツール、30操作) - 作成、フィールド、集計、計算メンバー/フィールド
- 📉 **チャート** (2ツール、29操作) - 作成、設定、系列、フォーマット、データラベル、トレンドライン
- 📝 **VBA** (1ツール、6操作) - モジュール、実行、バージョン管理、UserForm export/import (.frm/.frx)
- 📋 **範囲** (4ツール、46操作) - 値、数式、フォーマット、検証、保護
- 📄 **ワークシート** (2ツール、16操作) - ライフサイクル、色、可視性、ワークブック間移動
- 🔌 **接続** (1ツール、9操作) - OLEDB/ODBC管理と更新
- 🏷️ **名前付き範囲** (1ツール、6操作) - パラメータと設定
- 📁 **ファイル** (1ツール、6操作) - セッション管理、ワークブック作成、IRM/AIP保護ファイルサポート
- 🧮 **計算モード** (1ツール、3操作) - 計算モードの取得/設定と再計算のトリガー
- 🎚️ **スライサー** (1ツール、8操作) - PivotTablesとテーブルのインタラクティブフィルタリング
- 🎨 **条件付き書式** (1ツール、2操作) - ルールとクリア
- 📸 **スクリーンショット** (1ツール、2操作) - LLM視覚検証のための範囲/シートをPNGとしてキャプチャ
- 🪧 **ウィンドウ管理** (1ツール、9操作) - Excelの表示/非表示、配置、位置、ステータスバーフィードバック

📚 **[完全な機能リファレンス →](FEATURES.md)** - すべての230操作の詳細なドキュメント

## 💬 プロンプトの例

**データの作成と入力:**
- *"SalesTracker.xlsxという新しいExcelファイルを作成し、日付、製品、数量、単価、合計のテーブルをサンプルデータで作成してください"*
- *"このデータをA1:C4に入力してください - 名前、年齢、都市 / Alice、30、シアトル / Bob、25、ポートランド"*
- *"数量×単価を計算する数式列を追加してください"*

**分析と視覚化:**
- *"このデータから製品別の総売上を示すPivotTableを作成し、棒グラフを追加してください"*
- *"Power Queryを使用してproducts.csvをインポートし、Data Modelにロードし、総収益のメジャーを作成してください"*
- *"リージョンフィールドのスライサーを作成し、PivotTableをインタラクティブにフィルタリングできるようにしてください"*
- *"ProductIDを使用してOrdersテーブルとProductsテーブルのリレーションシップを作成してください"*

**フォーマットとスタイリング:**
- *"価格列を通貨フォーマットにし、500ドルを超える値を緑色でハイライトしてください"*
- *"この範囲を青色スタイルのExcelテーブルに変換し、合計行を追加してください"*
- *"ヘッダーを太字にし、暗い背景色を付け、列幅を自動調整してください"*
- *"A1:G1、A12:G12、A24:G24に同じセクションヘッダースタイルを一度に適用してください"*

**フォーマットの分割:** 数値表示フォーマットは`range`ツールを使用し、視覚スタイリングと自動調整は`range_format`を使用します。

**自動化:**
- *"すべてのPower Query Mコードをバージョン管理用のファイルにエクスポートしてください"*
- *"UpdatePricesマクロを実行してください"*
- *"作業中Excelを表示してください"* - リアルタイムで変更を確認

**🪟 エージェントモード — ExcelでAIの作業を監視:**
- *"ダッシュボードを構築中Excelを並べて表示してください"* - リアルタイムの可視性
- *"チャートを作成中見せてください"* - AIが好みを尋ね、Excelを表示
- ステータスバーにライブ進捗を表示: *"ExcelMcp: SalesデータからPivotTableを構築中..."*

## 👥 誰が使用すべきか

**最適な対象:**
- ✅ **データアナリスト** - 反復的なExcelワークフローを自動化
- ✅ **開発者** - Excelベースのデータソリューションを構築
- ✅ **ビジネスユーザー** - 複雑なExcelワークブックを管理
- ✅ **チーム** - GitでPower Query/VBA/DAXコードを維持

**不適切な対象:**
- ❌ サーバーサイドデータ処理（代わりにClosedXML、EPPlusなどのライブラリを使用）
- ❌ Linux/macOSユーザー（Windows + Excelインストールが必要）
- ❌ 大量のバッチ操作（Excel不要の代替手段を検討）

## 🚀 クイックスタート

| プラットフォーム | インストール |
|----------|-------------|
| **VS Code** | [拡張機能をインストール](https://marketplace.visualstudio.com/items?itemName=sbroenne.excel-mcp) （ワンクリック、推奨） |
| **Claude Desktop** | [最新リリース](https://github.com/sbroenne/mcp-server-excel/releases/latest)から`.mcpb`をダウンロード |
| **任意のMCPクライアント** | [最新リリース](https://github.com/sbroenne/mcp-server-excel/releases/latest)から`mcp-excel.exe`をダウンロードしPATHに追加 |
| **詳細** | 📖 [インストールガイド](docs/INSTALLATION.md) |

**⚠️ 重要:** 使用前にすべてのExcelファイルを閉じてください。サーバーは自動化中ワークブックへの排他アクセスを必要とします。


## 🔧 CLI vs MCPサーバー

このパッケージは**CLI**と**MCPサーバー**の両方のインターフェースを提供します。使用例に応じて選択してください：

| インターフェース | 最適な対象 | 理由 |
|-----------|----------|-----|
| **CLI** (`excelcli`) | コーディングエージェント（Copilot、Cursor、Windsurf）+ スクリプティング | **64%少ないトークン** - 単一ツール、大きなスキーマなし。Coreコードから自動生成され、1:1機能パリティを確保。excel-cliスキルにバンドル。 |
| **MCPサーバー** | 対話型AI（Claude Desktop、VS Code Chat） | 豊富なツール検出、永続的な接続。インタラクティブ、探索的なワークフローに最適。 |

**インストール:**
- **CLI via Copilot plugin**（Copilot CLI推奨）: スキルガイダンス用の`excel-cli`プラグインをインストールし、`excelcli`を別途インストール
- **CLI Standalone**: [リリース](https://github.com/sbroenne/mcp-server-excel/releases/latest)からZIPをダウンロードまたはNuGet経由でインストール
- **Skill only**: エージェントが既にPATHに`excelcli`を持っている場合、`excel-cli`スキルのみをインストール
- **MCPサーバー**: リリースからダウンロードまたはVS Code拡張機能をインストール

**⚡ CLIコマンド:** Roslynソースジェネレーターを使用してCoreサービス定義から自動生成されます。すべて22のコマンドカテゴリは共有コード生成を通じてMCPツールと正確な1:1パリティを維持します。詳細は[コード生成ドキュメント](docs/DEVELOPMENT.md#-cli-command-code-generation)を参照してください。

### 📦 GitHub Copilotプラグイン

ExcelMcpはCopilotプラグインマーケットプレイスで2つの**GitHub Copilot CLIプラグイン**として利用可能です：

```powershell
# プラグインマーケットプレイスを登録（一度だけ）
copilot plugin marketplace add sbroenne/mcp-server-excel-plugins

# 1つまたは両方のプラグインをインストール
copilot plugin install excel-mcp@mcp-server-excel-plugins      # 対話型AI用
copilot plugin install excel-cli@mcp-server-excel-plugins      # スクリプティング / コーディングエージェント用
```

- **`excel-mcp`** — 対話型ワークフロー用のMCPサーバー
- **`excel-cli`** — コーディングエージェント用のスキル（CLIツールが必要な場合は別途`excelcli`をインストール）

**注意:** 各リリース後、プラグインがマーケットプレイスに表示されるまで少し遅延がある場合があります。更新が同期されるまで数分待つ必要があります。

📖 [完全なインストールガイド →](docs/INSTALLATION.md)

<details>
<summary>📊 ベンチマーク結果（同じタスク、同じモデル）</summary>

| 指標 | CLI | MCPサーバー | 勝者 |
|--------|-----|------------|--------|
| **トークン** | ~59K | ~163K | 🏆 CLI（64%少ない） |

**重要な洞察:** MCPは各リクエストでLLMに23ツールスキーマを送信します（~100K+トークン）。

</details>

**手動インストール:**
```powershell
# プライマリ: 最新リリースからスタンドアロン実行ファイルをダウンロード（.NETランタイム不要）
# https://github.com/sbroenne/mcp-server-excel/releases/latest
# - ExcelMcp-MCP-Server-{version}-windows.zip → mcp-excel.exeを抽出
# - ExcelMcp-CLI-{version}-windows.zip → excelcli.exeを抽出（オプション、スクリプティング用）

# セカンダリ: .NETツール経由でインストール（.NET 10ランタイムが必要）
dotnet tool install --global Sbroenne.ExcelMcp.McpServer
dotnet tool install --global Sbroenne.ExcelMcp.CLI

# どちらの方法でインストールした後、すべてのコーディングエージェントを自動設定:
npx add-mcp "mcp-excel" --name excel-mcp
```

> ⚠️ **ステップ2には[Node.js](https://nodejs.org/)**が必要です（`npx`用）。必要に応じて`winget install OpenJS.NodeJS.LTS`でインストールしてください。

```powershell
# オプション: AIガイダンス用のエージェントスキルをインストール
npx skills add sbroenne/mcp-server-excel --skill excel-cli   # コーディングエージェント用
npx skills add sbroenne/mcp-server-excel --skill excel-mcp   # 対話型AI用
```

> 💡 **スキルはAIガイダンスを提供** - CLIスキルは強く推奨されます（スキルなしではエージェントがCLIで完全には動作しません）。MCPスキルも推奨されます - ワークフローベストプラクティスを追加し、トークン使用量を削減します。


## ⚙️ 動作原理 - COM自動化と統一サービスアーキテクチャ

**ExcelMcpはWindows COM自動化を使用して実際のExcelアプリケーションを制御します（.xlsxファイルだけではありません）。**

**MCPサーバー**と**CLI**の両方がExcelセッションを管理する共有**ExcelMCPサービス**と通信します。この統一アーキテクチャにより以下が可能になります：

```
┌─────────────────────┐     ┌─────────────────────┐
│   MCP Server        │     │   CLI (excelcli)    │
│  (AI assistants)    │     │  (coding agents)    │
└─────────┬───────────┘     └─────────┬───────────┘
          │                           │
          └──────────┬────────────────┘
                     ▼
          ┌─────────────────────────┐
          │   ExcelMCP Service      │
          │  (shared session mgmt)  │
          └─────────┬───────────────┘
                    ▼
          ┌─────────────────────────┐
          │   Excel COM API         │
          │  (Excel.Application)    │
          └─────────────────────────┘
```

**主な利点:**
- ✅ **共有セッション** - CLIとMCPサーバーが同じ開いているワークブックにアクセス可能
- ✅ **単一Excelインスタンス** - 重複するExcelプロセスやファイルロックなし
- ✅ **システムトレイUI** - ExcelMCPトレイアイコン経由でアクティブセッションを監視

**💡 ヒント: AI作業中Excelを監視**
デフォルトでは、Excelは高速自動化のために非表示で実行されます。リアルタイムで変更を確認するには、以下のように依頼してください：
- *"作業中Excelを表示してください"*
- *"何をしているか見せてください"*
- *"変更を確認できるようにExcelを開いてください"*

AIがExcelウィンドウを表示し、すべての操作がライブで行われるのを確認できます - 学習や変更の検証に最適です！

## 📋 追加情報

📚 **[CLIガイド →](src/ExcelMcp.CLI/README.md)** | **[エージェント用CLIスキル →](skills/excel-cli/SKILL.md)** | **[MCPサーバーガイド →](src/ExcelMcp.McpServer/README.md)** | **[すべてのエージェントスキル →](skills/README.md)**

**ライセンス:** MITライセンス - [LICENSE](LICENSE)ファイルを参照

**プライバシー:** プライバシポリシーについては[PRIVACY.md](PRIVACY.md)を参照
**貢献:** ガイドラインについては[CONTRIBUTING.md](docs/CONTRIBUTING.md)を参照

**構築:** このプロジェクト全体はGitHub Copilot AIアシスタンスを使用して開発されました - 主にClaudeですが、最近はAuto-modeを使用しています。

**謝辞:**
- Microsoft Excel Team - 包括的なCOM自動化APIの提供
- Model Context Protocolコミュニティ - AI統合標準の提供
- オープンソースコミュニティ - インスピレーションとベストプラクティスの提供

## 関連プロジェクト

作者による他のプロジェクト：

- [pytest-skill-engineering](https://github.com/sbroenne/pytest-skill-engineering) — AIエージェント用LLM搭載テストフレームワーク
- [Windows MCP Server](https://windowsmcpserver.dev/) — MCP経由のAI搭載Windows自動化
- [OBS Studio MCP Server](https://github.com/sbroenne/mcp-server-obs) — AI-powered OBS Studio automation
- [HeyGen MCP Server](https://github.com/sbroenne/heygen-mcp) — MCP server for HeyGen AI video generation
