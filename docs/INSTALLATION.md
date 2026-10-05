# インストールガイド - ExcelMcp

ExcelMcpをインストールして、Windows 上の実際の Microsoft Excel を AI アシスタントまたはコマンドラインから自動化します。ExcelMcpは2つの**同等のエントリポイント**を提供します — MCP クライアント用の**MCP Server**と、コーディングエージェント・スクリプト用の**CLI** (`excelcli`) です。用途に合ったガイドを選んでください（両方読んでも構いません。それぞれ独立しています）：

| ガイド | 最適な用途 |
|-------|----------|
| 📖 **[MCP Serverのインストール](https://excelmcpserver.dev/installation-mcp-server/)** | AIアシスタント — GitHub Copilot、Claude Desktop、Cursor、Windsurf、その他のMCPクライアント |
| 📖 **[CLIのインストール](https://excelmcpserver.dev/installation-cli/)** | スクリプト作成、RPA、CI/CDパイプライン、トークン効率の良い単一ツールを好むコーディングエージェント |

両方とも **Windows OS**、**Microsoft Excel 2016以降**、および**対話型デスクトップ**が必要です。スタンドアロン exe 配布版には .NET ランタイムは不要です。

| 作業環境 | 推奨インストール方法 |
|---|---|
| VS Code + GitHub Copilot | VS Code 拡張機能 (VSIX)。サーバーとスキルを同梱 — [フォークリリース](https://github.com/matsuzaki-hk/excel-mcp-japanese-support/releases/latest) の `excelmcp-ja-*.vsix` |
| Claude Desktop | MCPB バンドル — フォークリリースの `excel-mcp-ja-*.mcpb` (exe 同梱) |
| その他の MCP クライアント | フォークリリースの `ExcelMcp-MCP-Server-*-windows.zip` を展開し `mcp-excel.exe` を PATH に配置 |
| コーディングエージェント・スクリプト | フォークリリースの `ExcelMcp-CLI-*-windows.zip` を展開し `excelcli.exe` を PATH に配置 |

> このフォークは npm / NuGet / Marketplace には公開していません。配布は GitHub Releases のみです。upstream の npm パッケージ (`@sbroenne/mcp-server-excel`) は日本語対応を含まない upstream 版です。

> **ヒント:** **VS Code拡張機能**はMCP Serverのみをバンドルしています（スクリプト用にCLIが必要な場合は別途インストールしてください）。**GitHub Copilotプラグイン**は別々です — 必要なエントリポイントに応じて`excel-mcp`や`excel-cli`をインストールしてください — ワンクリックパスについてはMCP Serverガイドのクイックスタートを参照してください。

The Copilot plugins are also listed in
[Awesome Copilot](https://github.com/github/awesome-copilot), the default
marketplace in current Copilot clients. Each installation guide includes that
install route and our direct marketplace alternative.

---

## エージェントスキルのインストール（クロスプラットフォーム）

**最適な用途:** コーディングエージェント（Copilot、Cursor、Windsurf、Claude Code、Gemini、Codexなど）への日本語AIガイダンス追加

このフォークは日本語スキル `excel-mcp-ja` と `excel-cli-ja` を配布します。VS Code 拡張機能は `excel-mcp-ja` と `excel-mcp-report-formatting` を同梱します。直接インストールするには：

```powershell
# 日本語 CLI スキル（コーディングエージェント用 - トークン効率の良いワークフロー）
npx skills add matsuzaki-hk/excel-mcp-japanese-support --skill excel-cli-ja

# 日本語 MCP スキル（会話型AI用 - 豊富なツールスキーマ）
npx skills add matsuzaki-hk/excel-mcp-japanese-support --skill excel-mcp-ja

# インタラクティブインストール - excel-cli-ja、excel-mcp-ja、または両方を選択
npx skills add matsuzaki-hk/excel-mcp-japanese-support

# 特定のエージェント向けにインストール
npx skills add matsuzaki-hk/excel-mcp-japanese-support --skill excel-cli-ja -a cursor
npx skills add matsuzaki-hk/excel-mcp-japanese-support --skill excel-mcp-ja -a claude-code

# 両方のスキルをインストール
npx skills add matsuzaki-hk/excel-mcp-japanese-support --skill '*'

# グローバルにインストール（ユーザー全体）
npx skills add matsuzaki-hk/excel-mcp-japanese-support --skill excel-cli-ja --global
```

**43以上のエージェント**に対応しています。claude-code、github-copilot、cursor、windsurf、gemini-cli、codex、goose、cline、continue、replitなど。

**手動インストール:**

1. [フォークの GitHub Releases](https://github.com/matsuzaki-hk/excel-mcp-japanese-support/releases/latest) から `excel-skills-ja-v{version}.zip` をダウンロード
2. パッケージには両方の日本語スキルが含まれています：
   - `skills/excel-cli-ja/` - コーディングエージェント用（Copilot、Cursor、Windsurf）
   - `skills/excel-mcp-ja/` - 会話型AI用（Claude Desktop、VS Code Chat、Devin）
3. 必要なスキルをAIアシスタントのスキルディレクトリに展開します：
   - Copilot: `~/.copilot/skills/<skill-name>/`
   - Claude Code: `.claude/skills/<skill-name>/`
   - Cursor: `.cursor/skills/<skill-name>/`
   - Devin/Windsurf: `~/.codeium/windsurf-next/skills/<skill-name>/`

**参照:** [エージェントスキルドキュメント](../docs/AGENT-SKILLS.md)

---

## ヘルプ・サポート

- **ドキュメント:** [フォークの GitHub Pages](https://matsuzaki-hk.github.io/excel-mcp-japanese-support/)
- **Issues:** [フォークの GitHub Issues](https://github.com/matsuzaki-hk/excel-mcp-japanese-support/issues)
- **コントリビューション:** [コントリビューションガイド](https://matsuzaki-hk.github.io/excel-mcp-japanese-support/contributing/)

**自動化を楽しみましょう！ 🚀**
