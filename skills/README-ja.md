# Excel MCP Server - 日本語対応 Agent Skills

**Skill を使うと、AI アシスタントが Excel MCP Server を正しく使えるようになります。**
Skill は、コーディングエージェント（GitHub Copilot、Cursor、Windsurf、Claude Code、Devin など）が自動で読み込むガイドと例の小さなパッケージです。毎回細かく指示しなくても、正しいワークフロー、正しいパラメータ、よくある落とし穴をエージェントが把握できるようになります。Skill をインストールすると、AI が Excel を操作する際の信頼性が大幅に向上します。

本家 `sbroenne/mcp-server-excel` の英語版 skill は [こちら](https://github.com/sbroenne/mcp-server-excel/tree/main/skills) を参照してください。

## 日本語対応 skill

Excel への接続方法に合わせて、必要な skill を選んでインストールしてください（両方入れても構いません）。

| Skill | コンポーネント | 配布方法 | 最適な用途 |
|-------|-----------|--------------|----------|
| **excel-cli-ja** | CLI Tool (`excelcli.exe`) | `npx skills add`、手動展開 | コーディングエージェント — トークン効率が良く、`--help` で調査可能 |
| **excel-mcp-ja** | MCP Server (`mcp-excel.exe`) | VSIX、MCPB、`npx skills add` | 会話型 AI — 豊富なツールスキーマ |

**共有ガイド:** `skills/shared/*.md` — 両方の skill で使用される source of truth（各 skill の `references/` フォルダに自動コピーされます）

> **注:** 古い npm パッケージ (`excel-cli-skill`, `excel-mcp-skill`) はもう公開されていません。以下の方法を使用してください。

## インストール方法

### npx skills による直接インストール（推奨）

```powershell
# 対話式 — excel-cli-ja、excel-mcp-ja、またはその両方を選択
npx skills add matsuzaki-hk/excel-mcp-japanese-support

# または直接指定
npx skills add matsuzaki-hk/excel-mcp-japanese-support --skill excel-cli-ja
npx skills add matsuzaki-hk/excel-mcp-japanese-support --skill excel-mcp-ja
```

### GitHub Release から手動インストール

1. [Releases](https://github.com/matsuzaki-hk/excel-mcp-japanese-support/releases) から `excel-skills-ja-vX.Y.Z-ja.N.zip` をダウンロード
2. 展開して、使用する AI エージェントの skill ディレクトリにコピー

### VS Code / Devin 用 VSIX

[GitHub Releases](https://github.com/matsuzaki-hk/excel-mcp-japanese-support/releases) から `excelmcp-ja-X.Y.Z-ja.N.vsix` をダウンロードし、VS Code / Devin に手動インストールしてください。`excel-mcp-ja` skill が `chatSkills` として自動登録されます。

### Claude Desktop 用 MCPB

[GitHub Releases](https://github.com/matsuzaki-hk/excel-mcp-japanese-support/releases) から `excel-mcp-ja-X.Y.Z-ja.N.mcpb` をダウンロードし、Claude Desktop 設定画面にドラッグ＆ドロップしてください。

## Devin への手動導入

Devin では、`%APPDATA%\devin\mcp_config.json`（例: `C:\Users\<ユーザー名>\AppData\Roaming\devin\mcp_config.json`）に以下を追加してください。

```json
{
  "mcpServers": {
    "excel-mcp-ja": {
      "command": "C:\\Tools\\ExcelMcpJa\\mcp-excel.exe"
    }
  }
}
```

- `mcp-excel.exe` は [GitHub Releases](https://github.com/matsuzaki-hk/excel-mcp-japanese-support/releases) から `ExcelMcp-MCP-Server-X.Y.Z-ja.N-windows.zip` をダウンロードして、任意の固定パス（例: `C:\Tools\ExcelMcpJa`）に展開してください。
- VSIX を Devin にインストールすると `chatSkills` として `excel-mcp-ja` skill が利用可能になる場合がありますが、MCP server 実行ファイルのパスは手動で指定する必要があります。
- Devin Cloud（ブラウザ版）では Windows デスクトップ Excel が利用できないため、**ローカルの Devin Next IDE + Windows + Excel** が必要です。

## 動作環境

- Windows 10/11
- Microsoft Excel 2016 以降（デスクトップ版）
- 初回実行時に .NET ランタイムをダウンロードするため、ネットワークアクセスが必要
