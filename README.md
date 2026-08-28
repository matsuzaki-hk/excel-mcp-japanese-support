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

[**Webサイト**](https://excelmcpserver.dev/) ・
[**インストール**](https://excelmcpserver.dev/installation/) ・
[**機能**](https://excelmcpserver.dev/features/) ・
[**トラブルシューティング**](https://excelmcpserver.dev/troubleshooting/) ・
[**1分デモ**](https://youtu.be/B6eIQ5BIbNc)

**AIで実際のMicrosoft Excelを自動化。** ExcelMcpは、GitHub Copilot、Claude、ChatGPTなどのAIアシスタントが自然言語のリクエストでExcelを制御できるMCPサーバーです。MCPまたはトークン効率の高いCLIのいずれかを使用します。

ファイルパーサーツールとは異なり、ExcelMcpは公式COM APIを通じて**実際のExcelアプリケーション**を駆動します。Power Queryの更新、数式の再計算、DAXの評価、VBAとPython `=PY()` の実行、そしてピボットテーブル、グラフ、マクロ、データモデル、ブックの書式を保持できます。

**31の専門ツール、326の操作**で、Excel自動化の端到端をカバーします。

> [!IMPORTANT]
> **Windows**、**Microsoft Excel 2016以降**、および対話型デスクトップが必要です。Linux、macOS、またはサーバー側のバッチ処理を目的としていません。

<!-- upstream-en: "Japanese Support Features" -->
## 🇯🇵 日本語サポート機能

このフォークは元のExcel MCP Serverに日本語サポートを追加します：

- ✅ **日本語テーブル名** - 日本語の名前でExcelテーブルを作成できます（例：「売上データ」「生産計画」）
- ✅ **Unicode文字サポート** - 正規表現 `[\p{L}_][\p{L}\p{N}_]*` を使用してテーブル名に完全なUnicodeサポートを提供
- ✅ **本家 v2.0.0 新機能も日本語対応** - ファイルライフサイクル、Power Query、QueryTable、XmlMap、What-If Analysis（Scenario）、Drawing、Workbook SaveAs/Export など、本家 v1.10.8〜v2.0.0 で追加・変更された機能も日本語名・日本語ファイルパスで動作確認済
- ✅ **自動アップストリーム同期** - 毎日自動的に本家リポジトリと同期
- ✅ **競合解決** - 日本語サポートを保持するためのマージ競合の自動検出と通知

<!-- upstream-en: "Changes from Original" -->
### 元のリポジトリからの変更点

- `src/ExcelMcp.Core/Commands/Table/TableCommands.cs` を修正：
  - テーブル名検証正規表現を `^[a-zA-Z_][a-zA-Z0-9_]*$` から `^[\p{L}_][\p{L}\p{N}_]*$` に変更
  - Unicode文字サポートを示すエラーメッセージを更新

## 🚀 はじめ方

| 用途 | 推奨パス |
|---|---|
| **VS Code** | [拡張機能をインストール](https://marketplace.visualstudio.com/items?itemName=sbroenne.excel-mcp) |
| **Claude Desktop または他のMCPクライアント** | [MCP Serverをインストール](docs/INSTALLATION-MCP-SERVER.md) |
| **コーディングエージェントやスクリプト** | [CLIをインストール](docs/INSTALLATION-CLI.md) |
| **どれを選べばよいか分からない** | [インストール概要を読む](docs/INSTALLATION.md) |

> **フォーク版リリース:** 本フォークの [最新リリース](https://github.com/matsuzaki-hk/excel-mcp-japanese-support/releases/latest) からWindows用ZIPをダウンロードできます。

開始する前に開いているExcelブックを閉じてください。ExcelMcpは自動化中に排他的なアクセスを必要とします。

## 🎯 できること

- **[データと分析](https://excelmcpserver.dev/features/data-analytics/):** Power Query、DAX、Power Pivot、Excelテーブル、ピボットテーブル、データ接続
- **[セルとブック](https://excelmcpserver.dev/features/cells-workbooks/):** 範囲、数式、書式、ワークシート、ファイル、計算、名前付き範囲
- **[グラフとビジュアル](https://excelmcpserver.dev/features/charts-visuals/):** グラフ、スライサー、条件付き書式、スクリーンショット、描画、スパークライン
- **[自動化と高度な機能](https://excelmcpserver.dev/features/automation-advanced/):** VBA、Excel Python、Goal Seek、シナリオ、データテーブル、ウィンドウ、XML Maps

[326の操作の完全なリファレンス →](FEATURES.md)

## 💬 プロンプト例

自然言語で指示できます：

- *"products.csv を Power Query でインポートして、データモデルに読み込んで"*
- *"地域別の売上を示すピボットテーブルを作成して、棒グラフを追加して"*
- *"Goal Seek を使って、利益が 10万円 になる価格を求めて"*
- *"作業中のExcelを表示して"*

Excelが実際に作業を行うため、結果をリアルタイムで確認し、通常通りブックを編集し続けることができます。

## 🤖 MCP Server or CLI?

どちらのエントリーポイントも、同じCoreコマンドと動作を公開します。

| インターフェース | 最適な用途 | 理由 |
|---|---|---|
| **MCP Server** | 会話型アシスタントや探索的作業 | 豊富なスキーマ、ツール検出、永続的セッション |
| **CLI (`excelcli`)** | コーディングエージェント、自動化、スクリプト | コンパクトなツール表面と大幅に低いトークン使用量 |

MCP ServerはExcelMcpサービスをインプロセスで呼び出します。CLIはバックグラウンドデーモンを使用するため、コマンドをまたいでブックセッションが保持されます。

```text
AIアシスタントまたはスクリプト
        ↓   MCP Server / CLI
        ↓   ExcelMcp Core commands
        ↓   実際の Excel COM API
```

[アーキテクチャ](docs/ARCHITECTURE.md)を読むか、[MCP Server](https://excelmcpserver.dev/mcp-server/) および [CLI](https://excelmcpserver.dev/cli/) ガイドを参照してください。

## ⚙️ 技術要件

- ⚠️ **Windows専用** - COM相互運用はWindows固有です
- ⚠️ **Excel必須** - Microsoft Excel 2016以降がインストールされている必要があります
- ⚠️ **デスクトップ環境** - 実際のExcelプロセスを制御します（サーバー側処理には不適）

## 📦 インストール

| 配布形態 | 手順 |
|---|---|
| **MCP Server スタンドアロン** | [フォーク最新リリース](https://github.com/matsuzaki-hk/excel-mcp-japanese-support/releases/latest) から `ExcelMcp-MCP-Server-{version}-windows.zip` をダウンロードし、`mcp-excel.exe` を PATH の通ったフォルダに展開 |
| **CLI スタンドアロン** | [フォーク最新リリース](https://github.com/matsuzaki-hk/excel-mcp-japanese-support/releases/latest) から `ExcelMcp-CLI-{version}-windows.zip` をダウンロードし、`excelcli.exe` を PATH の通ったフォルダに展開 |
| **詳細** | フォークの [リリースページ](https://github.com/matsuzaki-hk/excel-mcp-japanese-support/releases) を参照 |

## 🌟 GitHub Star History

[![GitHub stars over time for ExcelMcp](https://excelmcpserver.dev/assets/images/star-history.svg)](https://github.com/matsuzaki-hk/excel-mcp-japanese-support/stargazers)

## � 追加情報

[ドキュメント](https://excelmcpserver.dev/) ・
[Changelog](CHANGELOG.md) ・
[貢献](docs/CONTRIBUTING.md) ・
[セキュリティ](docs/SECURITY.md) ・
[プライバシー](docs/PRIVACY.md)

**ライセンス:** MIT License - [LICENSE](LICENSE) ファイルを参照

<!-- upstream-en: "About This Fork" -->
## このフォークについて

これは [sbroenne](https://github.com/sbroenne) が管理する元の [sbroenne/mcp-server-excel](https://github.com/sbroenne/mcp-server-excel) リポジトリの**フォーク**です。

**目的:**
元のExcel MCP Serverはテーブル名にASCII文字のみをサポートしており、日本語ユーザーが自然な日本語テーブル名を使用できませんでした。このフォークは完全なUnicodeサポートを追加して、日本語テーブル名やその他の識別子を有効にします。

**原作者:**
[sbroenne](https://github.com/sbroenne) - 元のExcel MCP Server作成者

**フォークの保守:**
- 毎日午前9時（日本時間）に本家リポジトリと自動同期
- 日本語サポートの変更を保持するための競合自動検出とIssue通知
- GitHub Actionsを介した手動同期も可能
