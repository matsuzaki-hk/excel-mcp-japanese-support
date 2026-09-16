## Agent skills

### Repository rules

Follow `.github/copilot-instructions.md` and the path-specific files under `.github/instructions/`.

### Issue tracker

Work requests are tracked in this repository's GitHub Issues. See `docs/agents/issue-tracker.md`.

### Domain docs

Before exploring code, read `CONTEXT.md` and relevant decisions matching
`docs/ADR-*.md`. Use the context glossary and the current decision status; if a
proposal conflicts with an accepted decision, state the conflict rather than
silently overriding it.

## 日本語対応フォーク固有ルール

このリポジトリは `sbroenne/mcp-server-excel` の日本語対応フォーク
(`matsuzaki-hk/excel-mcp-japanese-support`)。作業ブランチは `ja-localization`
(`main` ではない)。upstream リモートは
`https://github.com/sbroenne/mcp-server-excel.git`。

### upstream マージ時に必ず維持するもの

- `src/ExcelMcp.Core/Commands/Table/TableCommands.cs`: テーブル名検証は Unicode
  正規表現 `^[\p{L}_][\p{L}\p{N}_]*$`。upstream の ASCII 版
  `^[a-zA-Z_][a-zA-Z0-9_]*$` に戻さない。
- `mcpb/manifest.json`: `name: excel-mcp-ja`、日本語 `display_name`、fork の
  `author` / `repository` / `homepage` / `documentation` / `support` /
  `privacy_policies`。
- `vscode-extension/package.json` / `package-lock.json`: `name: excel-mcp-ja`、
  `publisher: matsuzaki-hk`、fork の `repository` / `bugs` / `homepage`。
- `src/ExcelMcp.McpServer/.mcp/server.json`: fork の `name`
  (`io.github.matsuzaki-hk/excel-mcp-japanese-support`) と `websiteUrl`。
- `Directory.Build.props`: `PackageProjectUrl` / `RepositoryUrl` は fork 向け。
- `gh-pages/**`: `SITE_URL`・JSON-LD・生成リンクは
  `https://matsuzaki-hk.github.io/excel-mcp-japanese-support/`。
  `excelmcpserver.dev` は upstream 専用ドメインなので使わない。
- `.github/instructions/readme-management.instructions.md` はフォーク専用。
  upstream では削除されているが、こちらでは維持する。
- バージョン番号は upstream に合わせる (例: `2.0.8`)。`-ja.N` サフィックスは
  `.github/workflows/release-fork.yml` が付与するのでソースには書かない。

### カウントと監査

- 公開サーフェスは 31 tools / 326 operations が正準値。
  `scripts/check-doc-counts.ps1` がコード生成マニフェストから導出し、全
  ドキュメントの表記を検証する。カウント変更時は README / FEATURES /
  docs / gh-pages / mcpb / vscode-extension / skills すべてを同期する。
- `((dynamic))` キャストには直前に正当化コメント (PIA gap 等) が必須。
  `scripts/check-dynamic-casts.ps1` で強制される。

### CI / ワークフロー

- CI Gate は `main` への push/PR と `workflow_dispatch` でのみ起動する。
  `ja-localization` では `gh workflow run ci.yml --ref ja-localization` で
  手動起動する。
- Link Check は `_site` を `excel-mcp-japanese-support/` プレフィックス下に
  ミラーリングしてから lychee の `--root-dir` で検査する。確認済みの外部
  404 (MCP Registry、削除済みリポジトリ等) は workflow の除外リストで管理。
- GitHub Pages へのデプロイは `deploy-gh-pages.yml`。`gh-pages/audit_site.py`
  が canonical / オフサイトリンク / 生成物 (llms.txt, tools.json 等) を監査。

### リリース

- `release-fork.yml` は GitHub リリースのみ作成し、NuGet / VS Code
  Marketplace / MCP Registry には公開しない。成果物名: `excelmcp-ja-*.vsix`、
  `excel-mcp-ja-*.mcpb`、`ExcelMcp-*-{version}-windows.zip`。
- `.upstream-version` は同期ワークフローが upstream バージョンを release
  ジョブへ渡すための一時ファイルで、リリース成功後に削除される。

### 検証環境の注意

- ローカルの .NET SDK が `global.json` の要求バージョンを満たさない場合、
  完全なビルドは GitHub Actions (CI Gate) で確認する。
- Excel 依存テストはローカルのみ可能。GitHub-hosted runner に Excel はない。

### upstream 更新・競合報告時の対応手順

ユーザーが本家更新や競合を報告した場合、以下の順で対応する。

1. `gh run list` / `gh run view --log` でワークフロー実行履歴とログを
   確認し、発生している事象に対応する。
2. 修正を `ja-localization` に push し、CI Gate
   (`gh workflow run ci.yml --ref ja-localization`) と Link Check /
   Deploy GitHub Pages で検証する。
3. 処置完了後、`gh workflow run release-fork.yml --ref ja-localization`
   でリリースビルドを作成する (バージョンは自動で `X.Y.Z-ja.N`)。
4. リリース完了後、`ExcelMcp-MCP-Server-{version}-ja.{N}-windows.zip` を
   `C:\work\ExcelMcp-MCP-Server` へダウンロードする:

   ```powershell
   gh release download "vX.Y.Z-ja.N" --repo matsuzaki-hk/excel-mcp-japanese-support `
     -p "ExcelMcp-MCP-Server-*-windows.zip" -D "C:\work\ExcelMcp-MCP-Server" --clobber
   ```

5. リリースに `excel-skills-ja-*.zip` があり配布用スキルの更新があれば、
   `C:\Users\avalo\.codeium\windsurf-next\skills\excel-mcp` を差し替える。
   zip 内の `skills/excel-mcp-ja/` の中身を `excel-mcp` 名で配置し、旧版は
   `excel-mcp-backup-v{旧VERSION}` にリネームして退避する:

   ```powershell
   gh release download "vX.Y.Z-ja.N" --repo matsuzaki-hk/excel-mcp-japanese-support `
     -p "excel-skills-ja-*.zip" -D "C:\work\ExcelMcp-MCP-Server" --clobber
   Expand-Archive "C:\work\ExcelMcp-MCP-Server\excel-skills-ja-vX.Y.Z-ja.N.zip" `
     -DestinationPath "C:\work\ExcelMcp-MCP-Server\excel-skills-ja-vX.Y.Z-ja.N" -Force
   $skills = "C:\Users\avalo\.codeium\windsurf-next\skills"
   $oldVer = Get-Content "$skills\excel-mcp\VERSION"
   Rename-Item "$skills\excel-mcp" "excel-mcp-backup-v$oldVer"
   Copy-Item "C:\work\ExcelMcp-MCP-Server\excel-skills-ja-vX.Y.Z-ja.N\skills\excel-mcp-ja" `
     "$skills\excel-mcp" -Recurse
   ```

6. 完了後、ユーザーへ「Devin を閉じて exe を差し替えて MCP を更新して
   ほしい」旨を通知する (実行中セッションの MCP は旧 exe のままなので、
   ユーザー側で Devin 終了 → exe 差し替え → 再起動が必要。スキルは
   差し替え済みなので次のセッションから新版が読み込まれる)。
