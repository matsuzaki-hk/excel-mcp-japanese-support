# インストールガイド - ExcelMcp

ExcelMcpは2つの**同等のエントリポイント**を提供します — AIアシスタント用の**MCP Server**と、スクリプト作成・RPA・CI/CD用の**CLI**です。用途に合ったガイドを選んでください（両方読んでも構いません。それぞれ独立しています）：

| ガイド | 最適な用途 |
|-------|----------|
| 📖 **[MCP Serverのインストール](https://excelmcpserver.dev/installation-mcp-server/)** | AIアシスタント — GitHub Copilot、Claude Desktop、Cursor、Windsurf、その他のMCPクライアント |
| 📖 **[CLIのインストール](https://excelmcpserver.dev/installation-cli/)** | スクリプト作成、RPA、CI/CDパイプライン、トークン効率の良い単一ツールを好むコーディングエージェント |

両方とも**Windows OS**と**Microsoft Excel 2016以降**が必要です — スタンドアロンexe配布版には.NETランタイムは不要です。

> **ヒント:** **VS Code拡張機能**はMCP Serverのみをバンドルしています（スクリプト用にCLIが必要な場合は別途インストールしてください）。**GitHub Copilotプラグイン**は別々です — 必要なエントリポイントに応じて`excel-mcp`や`excel-cli`をインストールしてください — ワンクリックパスについてはMCP Serverガイドのクイックスタートを参照してください。

---

## エージェントスキルのインストール（クロスプラットフォーム）

**最適な用途:** コーディングエージェント（Copilot、Cursor、Windsurf、Claude Code、Gemini、Codexなど）へのAIガイダンス追加

VS Code拡張機能は`excel-mcp`スキルのみを自動インストールします。プラグインとスキルは異なるものです：プラグインはパッケージ化された統合ですが、スキルは再利用可能なAIガイダンスです。`excel-cli`スキル、またはスキルを直接利用したい環境では、以下のコマンドを使用してください：

```powershell
# CLIスキル（コーディングエージェント用 - トークン効率の良いワークフロー）
npx skills add sbroenne/mcp-server-excel --skill excel-cli

# MCPスキル（会話型AI用 - 豊富なツールスキーマ）
npx skills add sbroenne/mcp-server-excel --skill excel-mcp

# インタラクティブインストール - excel-cli、excel-mcp、または両方を選択
npx skills add sbroenne/mcp-server-excel

# 特定のエージェント向けにインストール
npx skills add sbroenne/mcp-server-excel --skill excel-cli -a cursor
npx skills add sbroenne/mcp-server-excel --skill excel-mcp -a claude-code

# 両方のスキルをインストール
npx skills add sbroenne/mcp-server-excel --skill '*'

# グローバルにインストール（ユーザー全体）
npx skills add sbroenne/mcp-server-excel --skill excel-cli --global
```

**43以上のエージェント**に対応しています。claude-code、github-copilot、cursor、windsurf、gemini-cli、codex、goose、cline、continue、replitなど。

**手動インストール:**
1. [GitHub Releases](https://github.com/sbroenne/mcp-server-excel/releases/latest)から`excel-skills-v{version}.zip`をダウンロード
2. パッケージには両方のスキルが含まれています：
   - `skills/excel-cli/` - コーディングエージェント用（Copilot、Cursor、Windsurf）
   - `skills/excel-mcp/` - 会話型AI用（Claude Desktop、VS Code Chat）
3. 必要なスキルをAIアシスタントのスキルディレクトリに展開します：
   - Copilot: `~/.copilot/skills/excel-cli/` または `~/.copilot/skills/excel-mcp/`
   - Claude Code: `.claude/skills/excel-cli/` または `.claude/skills/excel-mcp/`
   - Cursor: `.cursor/skills/excel-cli/` または `.cursor/skills/excel-mcp/`

**参照:** [エージェントスキルドキュメント](https://excelmcpserver.dev/skills/)

---

## ヘルプ・サポート

- **ドキュメント:** [GitHubリポジトリ](https://github.com/sbroenne/mcp-server-excel)
- **Issues:** [GitHub Issues](https://github.com/sbroenne/mcp-server-excel/issues)
- **コントリビューション:** [コントリビューションガイド](https://excelmcpserver.dev/contributing/)

**自動化を楽しみましょう！ 🚀**
