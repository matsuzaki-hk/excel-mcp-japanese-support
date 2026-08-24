# Excel MCP Server Skill（日本語対応フォーク）

Model Context Protocol（MCP）経由で Excel MCP Server を使う AI アシスタント向けの Agent Skill です。日本語テーブル名、日本語シート名、日本語ファイルパスに対応したフォーク版です。

## 最適な用途

- **会話型 AI**（Claude Desktop、VS Code Chat、Devin）
- 対話的な探索と反復的推論
- 自己修復ワークフロー
- 長時間自律タスク

## インストール

### VS Code / Devin 用 VSIX

[GitHub Releases](https://github.com/matsuzaki-hk/excel-mcp-japanese-support/releases) から `excelmcp-ja-X.Y.Z-ja.N.vsix` をダウンロードし、VS Code または Devin に手動インストールしてください。`excel-mcp-ja` skill が `chatSkills` として登録されます。

VS Code 設定で skills を有効化:
```json
{
  "chat.useAgentSkills": true
}
```

### その他のプラットフォーム

AI アシスタントの skill ディレクトリに展開してください。

| プラットフォーム | 配置場所 |
|----------|----------|
| **Claude Code** | `.claude/skills/excel-mcp-ja/` |
| **Cursor** | `.cursor/skills/excel-mcp-ja/` |
| **Windsurf** | `.windsurf/skills/excel-mcp-ja/` |
| **Gemini CLI** | `.gemini/skills/excel-mcp-ja/` |
| **Codex** | `.codex/skills/excel-mcp-ja/` |
| **Goose** | `.goose/skills/excel-mcp-ja/` |
| **その他 36+** | `npx skills` 経由 |

npx を使用:
```powershell
# 対話式 — excel-cli-ja、excel-mcp-ja、またはその両方を選択
npx skills add matsuzaki-hk/excel-mcp-japanese-support

# または直接指定
npx skills add matsuzaki-hk/excel-mcp-japanese-support --skill excel-mcp-ja
```

## 内容

```
excel-mcp-ja/
├── SKILL.md           # MCP ツールガイド付きメイン skill 定義
├── README.md          # このファイル
└── references/        # 詳細なドメイン別ガイド（英語）
    ├── anti-patterns.md
    ├── behavioral-rules.md
    ├── chart.md
    ├── conditionalformat.md
    ├── dashboard.md
    ├── datamodel.md
    ├── dmv-reference.md
    ├── excel_agent_mode.md
    ├── gotchas.md
    ├── m-code-syntax.md
    ├── pivottable.md
    ├── powerquery.md
    ├── range.md
    ├── screenshot.md
    ├── slicer.md
    ├── table.md
    ├── window.md
    └── worksheet.md
```

配布用パッケージはビルド時に `VERSION` ファイルを追加します。カノニカルな skill ソースにはバージョンメタデータを含めていないため、古いビルド入力になることを防ぎます。

## MCP Server セットアップ

この skill は Excel MCP Server と連携します。詳細なセットアップ手順は [skills/README-ja.md](../README-ja.md) を参照してください。

Devin への手動導入:
```json
// %APPDATA%\devin\mcp_config.json
{
  "mcpServers": {
    "excel-mcp-ja": {
      "command": "C:\\Tools\\ExcelMcpJa\\mcp-excel.exe"
    }
  }
}
```

## 関連リンク

- [Excel CLI Skill](../excel-cli-ja/SKILL.md) - コーディングエージェント向け CLI 版
- [フォーク GitHub Repository](https://github.com/matsuzaki-hk/excel-mcp-japanese-support)
- [本家 GitHub Repository](https://github.com/sbroenne/mcp-server-excel)
