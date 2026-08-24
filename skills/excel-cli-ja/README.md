# Excel CLI Skill（日本語対応フォーク）

Excel CLI ツール (`excelcli`) を使う AI コーディングアシスタント向けの Agent Skill です。日本語テーブル名、日本語シート名、日本語ファイルパスに対応したフォーク版です。

## 最適な用途

- **コーディングエージェント**（GitHub Copilot、Cursor、Windsurf、Codex、Gemini CLI など）
- トークン効率の良いワークフロー（大きなツールスキーマなし）
- `excelcli --help` で調査可能
- PowerShell パイプライン、CI/CD、バッチ処理でスクリプト化
- クワイエットモード (`-q`) でクリーンな JSON 出力のみ

## なぜ CLI か？

現代的なコーディングエージェントは、CLI ベースのワークフローを好む傾向があります。

```powershell
# トークン効率: スキーマオーバーヘッドなし
excelcli -q session open C:\データ\レポート.xlsx
excelcli -q range set-values --session 1 --sheet Sheet1 --range A1 --values '[["こんにちは"]]'
excelcli -q session close --session 1 --save
```

## インストール

### npx skills によるインストール

```powershell
# 対話式 — excel-cli-ja、excel-mcp-ja、またはその両方を選択
npx skills add matsuzaki-hk/excel-mcp-japanese-support

# または直接指定
npx skills add matsuzaki-hk/excel-mcp-japanese-support --skill excel-cli-ja
```

### その他のプラットフォーム

AI アシスタントの skill ディレクトリに展開してください。

| プラットフォーム | 配置場所 |
|----------|----------|
| **Claude Code** | `.claude/skills/excel-cli-ja/` |
| **Cursor** | `.cursor/skills/excel-cli-ja/` |
| **Windsurf** | `.windsurf/skills/excel-cli-ja/` |
| **Gemini CLI** | `.gemini/skills/excel-cli-ja/` |
| **Codex** | `.codex/skills/excel-cli-ja/` |
| **Goose** | `.goose/skills/excel-cli-ja/` |
| **その他 36+** | `npx skills` 経由 |

## 内容

```
excel-cli-ja/
├── SKILL.md           # CLI コマンドガイド付きメイン skill 定義
├── README.md          # このファイル
└── references/        # 正確な CLI コマンド/アクション/フラグリファレンス（英語）
    └── cli-commands.md
```

## CLI ツールのインストール

**GitHub Copilot `excel-cli` プラグイン**は skill package のみをインストールします。

### スタンドアロン ZIP による手動インストール

[GitHub Releases](https://github.com/matsuzaki-hk/excel-mcp-japanese-support/releases) から `ExcelMcp-CLI-X.Y.Z-ja.N-windows.zip` をダウンロードし、固定パス（例: `C:\Tools\ExcelMcpJa`）に展開して PATH を通してください。

```powershell
# ダウンロード例
$url = "https://github.com/matsuzaki-hk/excel-mcp-japanese-support/releases/latest/download/ExcelMcp-CLI-latest-windows.zip"
Invoke-WebRequest -Uri $url -OutFile ExcelMcp-CLI.zip
Expand-Archive -Path ExcelMcp-CLI.zip -DestinationPath $env:ProgramFiles\ExcelMcpJa
```

### NuGet Package Manager 経由（別途 .NET 10 Runtime または SDK が必要）

```powershell
dotnet tool install --global Sbroenne.ExcelMcp.CLI
```

インストール確認:
```powershell
excelcli --version
excelcli --help
```

## 関連リンク

- [Excel MCP Skill](../excel-mcp-ja/SKILL.md) - 会話型 AI 向け
- [フォーク GitHub Repository](https://github.com/matsuzaki-hk/excel-mcp-japanese-support)
- [本家 GitHub Repository](https://github.com/sbroenne/mcp-server-excel)
