# インストールガイド - ExcelMcp

ExcelMcp MCPサーバーとCLIツールの完全なインストール手順。

## システム要件

### 必須
- **Windows OS**（Windows 10以降）
- **Microsoft Excel 2016以降**（デスクトップ版 - Office 365、Professional Plus、またはスタンドアロン）

> **.NETランタイムは不要** - すべての配布は自己完結型です。

### オプション（特定機能用）
- **Microsoft Analysis Services OLE DB Provider（MSOLAP）** - DAXクエリ実行に必要（`evaluate`、`execute-dmv`アクション）
  - 最も簡単: [Power BI Desktop](https://powerbi.microsoft.com/desktop)をインストール（MSOLAPを含む）
  - 代替: [Microsoft OLE DB Driver for Analysis Services](https://learn.microsoft.com/analysis-services/client-libraries)
- **Node.js** - `npx`コマンドにのみ必要（`add-mcp`自動設定、エージェントスキル）。`winget install OpenJS.NodeJS.LTS`または[nodejs.org](https://nodejs.org/)からインストール

### 推奨
- Windows 11で最高のパフォーマンス
- 最新の更新を適用したOffice 365
- 最小8GB RAM

---

## クイックスタート（推奨）

セットアップの混乱を避けるためにこの順序を使用してください：

1. **1つのプライマリセットアップパスを選択**:
   - **VS Code拡張機能**（GitHub Copilotユーザー） - すべてを自動設定
   - **Claude Desktop MCPB** - ワンクリックMCPインストール
   - **GitHub Copilotプラグイン**（Copilot CLIユーザー） - マーケットプレイスインストール
   - **手動MCPセットアップ**（Cursor、Windsurfなどの他のMCPクライアント）
2. **MCPセットアップを検証**（手動セットアップのステップ4のクイックテストプロンプトを実行、または拡張機能/MCPB/プラグインインストール後にクライアントでテスト）
3. **オプション:** スクリプティング/RPA用にCLI（`excelcli`）をインストール
4. **オプション:** 拡張機能以外の環境用にエージェントスキルを別途インストール

### GitHub Copilotプラグイン

**最適な対象:** プラグインマーケットプレイスインストールを希望するGitHub Copilot CLIユーザー

```powershell
# プラグインマーケットプレイスを登録（一度だけ）
copilot plugin marketplace add sbroenne/mcp-server-excel-plugins

# 1つまたは両方のプラグインをインストール
copilot plugin install excel-mcp@mcp-server-excel-plugins      # 対話型AI用
copilot plugin install excel-cli@mcp-server-excel-plugins      # スクリプティング / コーディングエージェント用
```

- **`excel-mcp`** — 対話型AIワークフロー用
- **`excel-cli`** — コーディングエージェント用のスキル（CLIツールが必要な場合は別途`excelcli`をインストール）

**注意:** 各リリース後、プラグインがマーケットプレイスに表示されるまで少し遅延がある場合があります。

---

### VS Code拡張機能（最も簡単 - ワンクリックセットアップ）

1. **拡張機能のインストール**
   - VS Codeを開く
   - `Ctrl+Shift+X`（拡張機能）を押す
   - **"ExcelMcp"**を検索
   - **インストール**をクリック

2. **これで完了！**
   - 自己完結型MCPサーバーとCLIがバンドルされています（.NETランタイム不要）
   - GitHub Copilotを自動設定
   - `chatSkills`経由でエージェントスキルを登録（excel-mcp + excel-cli）
   - 初回起動時にクイックスタートガイドを表示

**マーケットプレイスリンク:** [Excel MCP VS Code拡張機能](https://marketplace.visualstudio.com/items?itemName=sbroenne.excel-mcp)

---

### Claude Desktop（ワンクリックインストール）

**最適な対象:** 最も簡単なインストールを希望するClaude Desktopユーザー

1. [最新リリース](https://github.com/sbroenne/mcp-server-excel/releases/latest)から`excel-mcp-{version}.mcpb`をダウンロード
2. `.mcpb`ファイルをダブルクリック（またはClaude Desktopにドラッグアンドドロップ）
3. Claude Desktopを再起動

これで完了！MCPBバンドルには必要なすべてが含まれています - .NETインストールは不要です。

---

## 手動MCPセットアップ（すべてのMCPクライアント）

**最適な対象:** 他のMCPクライアント（Cursor、Windsurf、Cline、Claude Code、Codex）、上級ユーザー

### ステップ1: MCPサーバーのダウンロード

1. [最新リリース](https://github.com/sbroenne/mcp-server-excel/releases/latest)に移動
2. **`ExcelMcp-MCP-Server-{version}-windows.zip`**をダウンロード
3. ZIPを永続的な場所に解凍（例: `C:\Tools\ExcelMcp\`）

```powershell
# 解凍例
Expand-Archive "ExcelMcp-MCP-Server-1.x.x-windows.zip" -DestinationPath "C:\Tools\ExcelMcp"
```

ZIPには`mcp-excel.exe`が含まれています - 完全に自己完結型の実行ファイルです（.NETランタイム不要）。

### ステップ2: PATHに追加（推奨）

フルパスを指定せずに`mcp-excel`をコマンドとして使用するには：

```powershell
# ユーザーPATHに追加（永続的）
$toolsDir = "C:\Tools\ExcelMcp"
$userPath = [Environment]::GetEnvironmentVariable("PATH", "User")
if ($userPath -notlike "*$toolsDir*") {
    [Environment]::SetEnvironmentVariable("PATH", "$userPath;$toolsDir", "User")
    Write-Host "Added $toolsDir to user PATH. Restart your terminal to apply."
}
```

または手動で: **設定 → システム → 詳細情報 → システムの詳細設定 → 環境変数 → ユーザー変数 → Path → 編集 → 新規** → `C:\Tools\ExcelMcp`を追加

### ステップ3: MCPクライアントの設定

#### オプションA: すべてのエージェントを自動設定（推奨）
```powershell
npx add-mcp "mcp-excel" --name excel-mcp
```

これは**Cursor、VS Code、Claude Code、Claude Desktop、Codex、Zed、Gemini CLI**などを自動検出して設定します。フラグを使用してカスタマイズ：

```powershell
# 特定のエージェントのみを設定
npx add-mcp "mcp-excel" --name excel-mcp -a cursor -a claude-code

# グローバルに設定（ユーザー全体、すべてのプロジェクト）
npx add-mcp "mcp-excel" --name excel-mcp -g

# 非インタラクティブ（プロンプトをスキップ）
npx add-mcp "mcp-excel" --name excel-mcp --all -y
```

> **必要:** `npx`用の[Node.js](https://nodejs.org/)。まだ利用可能でない場合は`winget install OpenJS.NodeJS.LTS`でインストール。永続的な`add-mcp`インストールは不要 - `npx`が自動的にダウンロード、実行、クリーンアップします。

> **注意:** `mcp-excel`がPATHにない場合は、代わりにフルパスを使用: `npx add-mcp "C:\Tools\ExcelMcp\mcp-excel.exe" --name excel-mcp`

#### オプションB: 手動設定

**クイックスタート:** すべてのクライアントの使用可能な設定ファイルが[`examples/mcp-configs/`](https://github.com/sbroenne/mcp-server-excel/tree/main/examples/mcp-configs/)にあります

**GitHub Copilot（VS Code）用:**

ワークスペースに`.vscode/mcp.json`を作成:

```json
{
  "servers": {
    "excel-mcp": {
      "command": "mcp-excel"
    }
  }
}
```

> `mcp-excel`がPATHにない場合は、フルパスを使用: `"command": "C:\\Tools\\ExcelMcp\\mcp-excel.exe"`

**GitHub Copilot（Visual Studio）用:**

ソリューションディレクトリまたは`%USERPROFILE%\.mcp.json`に`.mcp.json`を作成:

```json
{
  "servers": {
    "excel-mcp": {
      "command": "mcp-excel"
    }
  }
}
```

**Claude Desktop用:**

1. 設定ファイルを探す: `%APPDATA%\Claude\claude_desktop_config.json`
2. ファイルが存在しない場合は、以下の内容で作成
3. ファイルが存在する場合は、`excel-mcp`エントリを既存の`mcpServers`セクションにマージ

```json
{
  "mcpServers": {
    "excel-mcp": {
      "command": "mcp-excel",
      "args": [],
      "env": {}
    }
  }
}
```

4. 保存してClaude Desktopを再起動

**Cursor用:**

1. Cursor設定を開く（Ctrl+,）
2. 設定で"MCP"を検索
3. "settings.jsonで編集"をクリックまたは以下に設定を作成: `%APPDATA%\Cursor\User\globalStorage\mcp\mcp.json`
4. この設定を追加:

```json
{
  "mcpServers": {
    "excel-mcp": {
      "command": "mcp-excel",
      "args": [],
      "env": {}
    }
  }
}
```

5. 保存してCursorを再起動

**Cline（VS Code拡張機能）用:**

1. VS CodeでCline拡張機能をインストール
2. Clineパネルを開き、MCP設定の歯車アイコンをクリック
3. この設定を追加:

```json
{
  "mcpServers": {
    "excel-mcp": {
      "command": "mcp-excel",
      "args": [],
      "env": {}
    }
  }
}
```

4. 保存してVS Codeを再起動

**Windsurf用:**

1. Windsurf設定を開く
2. MCPサーバー設定に移動
3. この設定を追加:

```json
{
  "mcpServers": {
    "excel-mcp": {
      "command": "mcp-excel",
      "args": [],
      "env": {}
    }
  }
}
```

4. 保存してWindsurfを再起動

### ステップ4: MCPセットアップの検証

MCPクライアントを再起動してから、以下を依頼:
```
"test.xlsx"という空のExcelファイルを作成してください
```

動作すれば、準備完了です！🎉

**💡 ヒント:** AIの作業を監視したいですか？以下を依頼:
```
test.xlsxの作業中Excelを表示してください
```
これでExcelが表示されてリアルタイムですべての変更を確認できます - デバッグとデモに最適！

---

## オプション: CLIインストール（AI不要）

**最適な対象:** スクリプティング、RPA、CI/CDパイプライン、AIなしの自動化

`excelcli.exe`ツールは**excel-cli GitHub Copilotプラグイン**またはVS Code拡張機能をインストールすると既に含まれています。スキルのみのインストールでは、CLIを別途使用可能にする必要があります。

### プラグインまたはVS Code拡張機能経由でまだインストールされていない場合

スタンドアロンCLIをダウンロードして解凍:

1. [最新リリース](https://github.com/sbroenne/mcp-server-excel/releases/latest)に移動
2. **`ExcelMcp-CLI-{version}-windows.zip`**をダウンロード
3. 永続的な場所に解凍（例: `C:\Tools\ExcelMcp\`）

```powershell
Expand-Archive "ExcelMcp-CLI-1.x.x-windows.zip" -DestinationPath "C:\Tools\ExcelMcp"
```

### CLIをPATHに追加

```powershell
$toolsDir = "C:\Tools\ExcelMcp"
$userPath = [Environment]::GetEnvironmentVariable("PATH", "User")
if ($userPath -notlike "*$toolsDir*") {
    [Environment]::SetEnvironmentVariable("PATH", "$userPath;$toolsDir", "User")
    Write-Host "Added $toolsDir to user PATH. Restart your terminal to apply."
}
```

### クイックテスト

```powershell
excelcli --version
excelcli --help

# セッションでテスト
excelcli -q session open test.xlsx
excelcli -q session list
excelcli -q session close --session <id>
```

---

## GitHub Copilotプラグイン（代替インストール）

**最適な対象:** サポートされているプラグインサーフェスを通じてパッケージ化されたExcel自動化プラグインを希望するGitHub Copilotユーザー

ExcelMcpは2つの**GitHub Copilotマーケットプレイスプラグイン**を提供:

- **`excel-mcp`** — MCPサーバーを通した対話型Excelワークフローに最適
- **`excel-cli`** — トークン効率的なスクリプティングとコーディングエージェントワークフローに最適
- **どちらかのプラグインのみ、または両方をインストール可能**

### Copilot CLIプラグインインストール

```powershell
copilot plugin marketplace add sbroenne/mcp-server-excel-plugins
copilot plugin install excel-mcp@mcp-server-excel-plugins
copilot plugin install excel-cli@mcp-server-excel-plugins
```

**インストール後:**
- **`excel-mcp`** — 使用可能です。MCP設定をCopilot設定にマージする場合は、プラグインREADMEに従ってください。
- **`excel-cli`** — PATHに必要な場合は`excelcli`を別途インストール:

```powershell
dotnet tool install --global Sbroenne.ExcelMcp.CLI
excelcli --version
```

> **注意:** 上記のCopilot CLIインストールコマンドはGitHub Copilotプラグインマーケットプレイスに固有です。VS CodeとClaudeには独自のプラグインシステムがあり、インストールフローが別々です。

プラグインは各ExcelMcpリリース後に自動的に公開されますが、マーケットプレイスに更新が表示されるまで少し待つ必要がある場合があります。

---

## 代替: NuGet .NETツールインストール（二次）

**パッケージマネージャーを好むユーザー、または既に.NETがインストールされているユーザー向け**

NuGetは二次配布チャネルです。**.NET 10ランタイムまたはSDK**がインストールされている必要があります。

```powershell
# .NET 10ランタイムまたはSDKが必要
dotnet tool install --global Sbroenne.ExcelMcp.McpServer
dotnet tool install --global Sbroenne.ExcelMcp.CLI
```

インストール後、MCPクライアントを`"command": "mcp-excel"`で設定（スタンドアロンexeと同じ）。

**NuGet経由で更新:**
```powershell
dotnet tool update --global Sbroenne.ExcelMcp.McpServer
dotnet tool update --global Sbroenne.ExcelMcp.CLI
```

**アンインストール:**
```powershell
dotnet tool uninstall --global Sbroenne.ExcelMcp.McpServer
dotnet tool uninstall --global Sbroenne.ExcelMcp.CLI
```

> **NuGetが二次である理由:** スタンドアロンexe配布は.NETランタイムを必要としないため、ほとんどのユーザーにとってインストールが容易です。NuGetは、パッケージマネージャーを好むユーザーやワークフローに既に.NETがインストールされているユーザー向けの代替として利用可能です。

---

## エージェントスキルインストール（クロスプラットフォーム）

**最適な対象:** コーディングエージェント（Copilot、Cursor、Windsurf、Claude Code、Gemini、Codexなど）にAIガイダンスを追加

スキルはVS Code拡張機能によって自動インストールされます。プラグインとスキルは異なるものです: プラグインはパッケージ化されたサーフェス統合、スキルは再利用可能なAIガイダンスです。スキルを直接使用したい環境では、以下のコマンドを使用:

```powershell
# CLIスキル（コーディングエージェント用 - トークン効率的なワークフロー）
npx skills add sbroenne/mcp-server-excel --skill excel-cli

# MCPスキル（対話型AI用 - リッチなツールスキーマ）
npx skills add sbroenne/mcp-server-excel --skill excel-mcp

# 特定のエージェント用にインストール
npx skills add sbroenne/mcp-server-excel --skill excel-cli -a cursor
npx skills add sbroenne/mcp-server-excel --skill excel-mcp -a claude-code

# グローバルにインストール（ユーザー全体）
npx skills add sbroenne/mcp-server-excel --skill excel-cli --global
```

**43以上のエージェントをサポート** - claude-code、github-copilot、cursor、windsurf、gemini-cli、codex、goose、cline、continue、replitなどを含む。

**📚 [エージェントスキルガイド →](../skills/README.md)**

---

## ExcelMcpの更新

### 現在のバージョンを確認

```powershell
# MCPサーバーバージョンを確認
mcp-excel --version

# CLIバージョンを確認
excelcli --version
```

### 新しいバージョンに更新

**スタンドアロンexe（プライマリ）:**

1. [最新リリース](https://github.com/sbroenne/mcp-server-excel/releases/latest)に移動
2. 新しいZIPをダウンロード: `ExcelMcp-MCP-Server-{version}-windows.zip`および/または`ExcelMcp-CLI-{version}-windows.zip`
3. 解凍してインストールディレクトリの既存のファイルを上書き

```powershell
# 更新例
Expand-Archive "ExcelMcp-MCP-Server-1.x.x-windows.zip" -DestinationPath "C:\Tools\ExcelMcp" -Force
```

4. MCPクライアントを再起動（VS Code、Claude Desktop、Cursorなど）

**NuGet（二次）:**

```powershell
dotnet tool update --global Sbroenne.ExcelMcp.McpServer
dotnet tool update --global Sbroenne.ExcelMcp.CLI
```

### 新着情報を確認

更新前にリリースノートを確認:
- **GitHubリリース:** https://github.com/sbroenne/mcp-server-excel/releases
- **Changelog:** https://github.com/sbroenne/mcp-server-excel/blob/main/CHANGELOG.md

---

## トラブルシューティング

### 一般的な問題

#### 1. "'mcp-excel' は内部コマンドまたは外部コマンドとして認識されません"

**解決策:** `mcp-excel.exe`がPATHにありません。

以下のいずれか:
- `mcp-excel.exe`を含むディレクトリをPATHに追加（上記ステップ2を参照）
- またはMCPクライアント設定でフルパスを使用: `"command": "C:\\Tools\\ExcelMcp\\mcp-excel.exe"`

#### 2. MCPサーバーが応答しない

**exeが存在するか確認:**
```powershell
where.exe mcp-excel
# またはフルパスで:
Test-Path "C:\Tools\ExcelMcp\mcp-excel.exe"
```

**動作するか確認:**
```powershell
mcp-excel --version
```

#### 3. "ワークブックがロックされています" または "ファイルを開けません"

**解決策:** ExcelMcpを実行する前にすべてのExcelウィンドウを閉じてください

ExcelMcpはワークブックへの排他アクセスを必要とします（Excel COMの制限）。

#### 4. MCPサーバーが古いバージョンで動作している

**解決策:** MCPクライアントを完全に再起動
- VS Codeを完全に閉じる（ターミナルウィンドウを含む）
- Claude Desktopを完全に閉じる
- アプリケーションを再開

## アンインストール

### MCPサーバーのアンインストール
```powershell
# スタンドアロンexe: 解凍したファイルを削除するだけ
Remove-Item "C:\Tools\ExcelMcp\mcp-excel.exe" -Force

# PATHに追加した場合は削除
# 設定 → システム → 詳細情報 → システムの詳細設定 → 環境変数
# PATHを編集してExcelMcpディレクトリを削除

# NuGet（dotnet tool経由でインストールした場合）:
dotnet tool uninstall --global Sbroenne.ExcelMcp.McpServer
```

### CLIのアンインストール
```powershell
# スタンドアロンexe:
Remove-Item "C:\Tools\ExcelMcp\excelcli.exe" -Force

# NuGet（dotnet tool経由でインストールした場合）:
dotnet tool uninstall --global Sbroenne.ExcelMcp.CLI
```

---

## ヘルプの取得

- **ドキュメント:** [GitHubリポジトリ](https://github.com/sbroenne/mcp-server-excel)
- **問題:** [GitHub Issues](https://github.com/sbroenne/mcp-server-excel/issues)
- **貢献:** [貢献ガイド](https://github.com/sbroenne/mcp-server-excel/blob/main/docs/CONTRIBUTING.md)

---

## 次のステップ

インストール後:

1. **基本を学ぶ:** ワークシートの作成、値の設定などの簡単なコマンドを試す
2. **機能を探索する:** 完全な機能リストについては[README](https://github.com/sbroenne/mcp-server-excel#readme)を参照
3. **ガイドを読む:**
   - [MCPサーバーガイド](https://github.com/sbroenne/mcp-server-excel/blob/main/src/ExcelMcp.McpServer/README.md)
   - [CLIガイド](https://github.com/sbroenne/mcp-server-excel/blob/main/src/ExcelMcp.CLI/README.md)
   - [エージェントスキル](https://github.com/sbroenne/mcp-server-excel/blob/main/skills/excel-mcp/SKILL.md) - クロスプラットフォームAIガイダンス
4. **コミュニティに参加:** リポジトリにスターを付け、問題を報告、改善に貢献

---

## エージェントスキル（オプション）

エージェントスキルはAIコーディングアシスタントにドメイン固有のガイダンスを提供し、Excel MCPサーバーをより効果的に使用できるようにします。

> **注意:** エージェントスキルは**コーディングエージェント**（GitHub Copilot、Claude Code、Cursor）用です。**Claude Desktop**は代わりにMCPプロンプトを使用します（MCPサーバー経由で自動的に含まれます）。

### 異なる使用例向けの2つのスキル

| スキル | 対象 | 最適な用途 |
|-------|--------|----------|
| **excel-cli** | CLIツール | **コーディングエージェント**（Copilot、Cursor、Windsurf） - トークン効率的、`excelcli --help`で発見可能 |
| **excel-mcp** | MCPサーバー | **対話型AI**（Claude Desktop、VS Code Chat） - リッチなツールスキーマ、探索的ワークフロー |

**VS Code拡張機能:** スキルは自動的に`~/.copilot/skills/`にインストールされます。

**他のプラットフォーム（Claude Code、Cursor、Windsurf、Gemini、Codexなど）:**

```powershell
# CLIスキルをインストール（コーディングエージェント用に推奨 - Copilot、Cursor、Windsurf、Codexなど）
npx skills add sbroenne/mcp-server-excel --skill excel-cli

# MCPスキルをインストール（対話型AI用 - Claude Desktop、VS Code Chat）
npx skills add sbroenne/mcp-server-excel --skill excel-mcp

# インタラクティブインストール - excel-cli、excel-mcp、または両方を選択するプロンプト
npx skills add sbroenne/mcp-server-excel

# 特定のスキルを直接インストール
npx skills add sbroenne/mcp-server-excel --skill excel-cli   # コーディングエージェント
npx skills add sbroenne/mcp-server-excel --skill excel-mcp   # 対話型AI

# 両方のスキルをインストール
npx skills add sbroenne/mcp-server-excel --skill '*'

# 特定のエージェントを対象（オプション - 省略すると自動検出）
npx skills add sbroenne/mcp-server-excel --skill excel-cli -a cursor
npx skills add sbroenne/mcp-server-excel --skill excel-mcp -a claude-code
```

**手動インストール:**
1. [GitHubリリース](https://github.com/sbroenne/mcp-server-excel/releases/latest)から`excel-skills-v{version}.zip`をダウンロード
2. パッケージには両方のスキルが含まれています:
   - `skills/excel-cli/` - コーディングエージェント用（Copilot、Cursor、Windsurf）
   - `skills/excel-mcp/` - 対話型AI用（Claude Desktop、VS Code Chat）
3. 必要なスキルをAIアシスタントのスキルディレクトリに解凍:
   - Copilot: `~/.copilot/skills/excel-cli/` または `~/.copilot/skills/excel-mcp/`
   - Claude Code: `.claude/skills/excel-cli/` または `.claude/skills/excel-mcp/`
   - Cursor: `.cursor/skills/excel-cli/` または `.cursor/skills/excel-mcp/`

**参照:** [エージェントスキルドキュメント](https://github.com/sbroenne/mcp-server-excel/blob/main/skills/README.md)

---

**ハッピーオートメーション！🚀**
