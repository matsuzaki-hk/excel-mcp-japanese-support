# ExcelMcp agent instructions

ExcelMcp automates installed desktop Excel through COM on Windows. Use
PowerShell and the SDK selected by `global.json`. Desktop Excel is required
for COM tests; GitHub-hosted runners do not have Excel.

MCP Server and `excelcli` are equal entry points: behavior, defaults, validation,
results, and documentation must agree.

For an unfamiliar area, start with [CONTEXT.md](CONTEXT.md) for the system map
and terminology. [Architecture decisions](docs/DECISIONS.md) explain current
choices and tradeoffs; read the records relevant to an architectural change,
not the entire collection for every task. Surface conflicts before changing a
decision. Actionable rules belong here and in the applicable guides below.

Work requests are tracked in GitHub Issues; see
[issue tracker](docs/agents/issue-tracker.md). Contributor setup and client
instruction discovery are in [agent development](docs/agents/development.md).

## Task-specific guidance

Before changing files, read the matching guides below, including nested
`AGENTS.md` files. Native discovery differs between Copilot, Claude Code, and
Codex; launching at the root does not guarantee every nested file is loaded.
Paths are relative to the repository root.

| Work | Required guidance |
| --- | --- |
| `src/**/*.cs` | [Runtime boundaries](docs/agents/rules/architecture-patterns.md) |
| Core commands/action models, Service, CLI, MCP, generators, or `.github/usage-analytics-weights.json` | [Generated contracts](docs/agents/rules/coverage-prevention-strategy.md) |
| Core or ComInterop C# | [COM safety](docs/agents/rules/excel-com-interop.md) |
| Connection commands/sanitizer or connection tests/helpers/fixtures | [Connections](docs/agents/rules/excel-connection-types-guide.md) |
| `tests/**/*.cs` | [Testing strategy](tests/AGENTS.md) |
| Workflows, project files, SDK/build configuration, PowerShell scripts, `.changeset/**` | [Build and release](docs/agents/rules/development-workflow.md) |
| README/index files, FEATURES, CHANGELOG, SECURITY, PRIVACY, docs, specs, skills, or website | [Documentation](docs/agents/rules/documentation-structure.md) |
| Skill sources, Core command metadata, generators, MCP, or Build-AgentSkills.ps1 | [Product agent guidance](docs/agents/rules/mcp-llm-guidance.md) |
| MCP Server or MCP generator | [MCP boundaries](docs/agents/rules/mcp-server-guide.md) |
| `llm-tests/**` | [LLM evaluations](llm-tests/AGENTS.md) |
| Repository/agent instructions, CONTEXT, or `docs/agents/**` | [Instruction maintenance](docs/agents/rules/meta.md) |
| `vscode-extension/**` | [Extension](vscode-extension/AGENTS.md) |
| `videos/excel-mcp-intro/**` | [Video](videos/excel-mcp-intro/AGENTS.md) |
| `videos/agentic-world-bank-briefing/**` | [Agentic workflow video](videos/agentic-world-bank-briefing/AGENTS.md) |

## Implementation

- Core `[ServiceCategory]` interfaces drive generated Service, CLI, and MCP
  routing. Change contracts/generators, not emitted code. Follow a changed
  contract through both entry points, tests, and shared guidance.
- Keep agent-facing metadata, server instructions, skills, and recovery messages
  consistent with implemented behavior. Follow the product agent guidance above;
  do not edit generated or installed skill copies.
- Preserve exact tool, action, parameter, and flag names in agent guidance.
  When MCP and CLI spellings differ, show both explicitly or use native examples;
  do not replace identifiers with vague descriptions.
- Do not tell agents to create or retain workbook backup/recovery copies, or to
  work on duplicate workbooks as a prerequisite to requested edits. Explain
  destructive consequences without adding file-copy steps. Copying or exporting
  is appropriate only when part of the user's request.
- Production code must access workbook contents through Excel COM. Never open
  an Excel file as a ZIP/OOXML package or parse/modify its internal XML outside
  tests. Pre-open binary container detection may read only IRM/AIP protection
  metadata; it must not parse workbook content.
- Behavioral changes require a focused failing regression test before the fix.
  Documentation/configuration-only changes do not need synthetic tests.
- Do not automatically clean up or roll back workbook changes when an
  operation fails. Report the exact partial state and error so the agent can
  inspect the workbook and decide whether cleanup is appropriate.
- `Success == true` requires an empty or null `ErrorMessage`.
- MCP Server and `excelcli` are the only supported product entry points. Do not
  preserve or add public Core/ComInterop compatibility APIs for hypothetical
  external consumers when neither entry point uses them.
- Keep customer/workbook data, credentials, connection strings, and private
  paths out of public artifacts. Keep temporary notes outside the repository.

## Build and validation

```powershell
dotnet restore Sbroenne.ExcelMcp.sln
dotnet build Sbroenne.ExcelMcp.sln -c Release --no-restore
```

Build with zero warnings. Use targeted tests; see the
[testing strategy](tests/AGENTS.md). Run every Excel-dependent test command
sequentially; never overlap Excel test fixtures, test hosts, or E2E runs.
Runtime changes in Core, ComInterop, Service, CLI, MCP, or their generators also
require `scripts\Test-E2E.ps1` locally with Excel, once against the final PR
source, after the last runtime-affecting change. Rerun it if later commits
change runtime behavior. During development, use focused checks; the commit hook
does not repeat full E2E on every commit. Report final E2E as not run when Excel is unavailable;
build-only checks do not cover COM.

The Python evaluation suite under `llm-tests/` is on-demand only, not part of the
normal development lifecycle. This includes offline checks, Excel fixture checks,
SDK discovery, and live agent comparisons. Run it and install its dependencies
only when the user explicitly requests evaluation work. Unrun evaluations or
unavailable Python/SDK dependencies are not implementation, commit, PR, or merge blockers.
This does not waive required .NET/COM/E2E validation or normal Git hooks.

Run applicable existing checks, not replacement audits:

```powershell
& .\scripts\check-com-leaks.ps1
& .\scripts\Invoke-ExcelFreeTests.ps1 -Local -Contracts
& .\scripts\check-success-flag.ps1
& .\scripts\check-doc-counts.ps1 -SkipBuild
& .\scripts\check-dynamic-casts.ps1
& .\scripts\check-workbook-package-access.ps1
```

`-SkipBuild` requires a successful Release solution build in this worktree.
Otherwise omit it. PRs record the root cause, affected contracts, and validation.

## Code Review Rules

Report high-confidence defects introduced by the change, not style or unrelated
cleanup. These checks apply to review tasks; they do not require installing
dependencies, running builds, or editing code unless requested.

When `ponytail-review` is available, use it as an additional review pass for
concrete, behavior-preserving simplifications introduced by the change. It must
not replace correctness or security review. Required Excel COM cleanup,
validation, error handling, tests, generated contracts, and matching MCP/CLI
behavior are not unnecessary complexity.

- Typed PIAs first, except documented runtime gaps (`Application.Run`, VBE,
  Office-core). Do not reintroduce unavailable dependencies. Dynamic COM numeric
  values need `Convert.*`, not direct casts.
- Acquired COM references require `finally` cleanup, excluding session-owned
  objects. Process cleanup uses PID/start-time identity, never process names.
- `0x800A03EC` is not a unique diagnosis. Preserve batch/Service error context,
  cancellation, and session recovery; a timeout must not become success.
- Core contracts must agree across generated Service, CLI options/batch JSON,
  MCP schemas, and manual tool exceptions, including defaults and timeouts.
- Added, renamed, or removed MCP actions and CLI commands must update
  `.github/usage-analytics-weights.json`. Check that each new effort level
  matches the work: reads are light, edits are medium, and refresh, evaluation,
  macro runs, and model or PivotTable building are heavy.
- MCP stdout, including bootstrap output, is JSON-RPC only.
- Agent-facing descriptions, server instructions, skills, and recovery messages
  must agree with actual defaults and advertised capabilities. Flag stale input
  names, nonexistent actions, missing parameter documentation, emojis, forced
  unrequested work, and screenshots required without an interactive desktop.
  Consent advice is not proof that server-side elicitation is implemented.
  Distinguish MCP input names from nested JSON keys, outputs, and CLI batch keys.
- Tests establish actual Excel state and returned fields, not only `Success`.
  Generated artifacts must change through their source; use code-derived counts
  rather than copied literals.

## Git and release

- Never commit directly or force-push to `main`.
- Never skip, disable, suppress, or bypass a Git hook for any reason, including
  transient failures or previously passing validation. Fix the failure or
  report the blocker, then rerun the normal hooked command.
- Rewriting a feature branch's remote history requires explicit user
  authorization. Use `--force-with-lease` with the expected remote commit;
  never use plain `--force`. If the lease fails, stop and inspect the remote
  changes before retrying.
- Coding-agent assignments requesting repository changes authorize delivery
  commits and a PR. Otherwise ask before commit/push. Merging and publishing
  require separate authorization.
- Before finalizing a PR, resolve every review thread after addressing it, or
  dismiss it with a clear recorded reason when no change is appropriate. Never
  leave review comments unanswered or unresolved.
- User-visible changes require a changeset; internal/docs/tests/CI changes use
  the `skip-changelog` PR label. Versions and `CHANGELOG.md` are release-generated.
  Follow [.changeset/README.md](.changeset/README.md) for package names and
  validate the fragment against the PR's actual base before pushing.
- Plugin publication changes must follow
  `.github/workflows/docs/publish-plugins-setup.md#maintenance-and-updates`;
  the published repository is output-only.
- Unchanged plugin output must not create publication commits/tags. Marketplace
  updates are opt-in and maintain one owned PR; follow
  `.github/workflows/docs/awesome-copilot-update-setup.md` for comparison,
  guarded writes, catch-up, and pinned workflow compilation.

## 日本語対応フォーク固有ルール

このリポジトリは `sbroenne/mcp-server-excel` の日本語対応フォーク
(`matsuzaki-hk/excel-mcp-japanese-support`)。作業ブランチは `ja-localization`
(`main` ではない)。upstream リモートは
`https://github.com/sbroenne/mcp-server-excel.git`。

### upstream マージ時に必ず維持するもの

- `mcpb/manifest.json`: `name: excel-mcp-ja`、日本語 `display_name`、fork の
  `author` / `repository` / `homepage` / `documentation` / `support` /
  `privacy_policies`。`server` セクションは exe 同梱の `binary` 型を維持
  (upstream は npm 公開済みのため `node`/`npx` 型だが、フォークは npm 未公開)。
- `vscode-extension/package.json` / `package-lock.json`: `name: excel-mcp-ja`、
  `publisher: matsuzaki-hk`、fork の `repository` / `bugs` / `homepage`。
  `chatSkills` は `excel-mcp-ja` (ソース管理の日本語 skill) に加えて
  upstream の `excel-mcp-report-formatting` を同梱する。
- `vscode-extension/src/extension.ts`: provider ID は `excel-mcp-ja`、
  `userGuideUrl` は fork の GitHub Pages。
- `src/ExcelMcp.McpServer/.mcp/server.json`: fork の `name`
  (`io.github.matsuzaki-hk/excel-mcp-japanese-support`) と `websiteUrl`。
- `Directory.Build.props`: `PackageProjectUrl` / `RepositoryUrl` は fork 向け。
- `src/ExcelMcp.McpServer/Tools/ExcelToolsBase.cs`: `JsonOptions` に
  `Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping` を維持
  (日本語を `\uXXXX` エスケープせず返す)。
- `gh-pages/**`: `SITE_URL`・JSON-LD・生成リンクは
  `https://matsuzaki-hk.github.io/excel-mcp-japanese-support/`。
  `excelmcpserver.dev` は upstream 専用ドメインなので使わない。
- `docs/agents/rules/readme-management.instructions.md` はフォーク専用。
  upstream では削除されているが、こちらでは維持する。
- バージョン番号は upstream に合わせる (例: `2.2.1`)。`-ja.N` サフィックスは
  `.github/workflows/release-fork.yml` が付与するのでソースには書かない。
- `scripts/Build-AgentSkills.ps1`: upstream 版をベースに `-Language ja` を
  維持。ja では `skills/excel-mcp-ja` / `skills/excel-cli-ja` をソースから
  `excel-skills-ja-v*.zip` にパッケージする。
- `scripts/Build-ReleasePackages.ps1`: VSIX に `excel-mcp-ja` skill を追加同梱し
  `excelmcp-ja-{version}.vsix` (x64 のみ) を生成するフォーク修正を維持。
- `mcpb/Build-McpBundle.ps1`: exe 同梱版を維持 (upstream の npx 型にしない)。

### upstream の設計変更で不要になったもの (v2.1+)

- upstream `e94159f9` ("Allow localized Excel table names") で厳格なテーブル名
  正規表現検証が廃止され、Excel 自身の検証に委ねる設計になった。フォーク独自の
  Unicode 正規表現 `^[\p{L}_][\p{L}\p{N}_]*$` は不要になり削除済み。
  upstream 側の `ValidateRequiredTableName` (null/空チェック) を採用する。

### カウントと監査

- 公開サーフェスは upstream の `doc-counts.json` が正準 (v2.2.1 時点で
  31 tools / 388 operations)。`scripts/check-doc-counts.ps1` がコード生成
  マニフェストから導出し、全ドキュメントの表記を検証する。カウント変更時は
  README / FEATURES / docs / gh-pages / mcpb / vscode-extension / skills
  すべてを同期する。
- `((dynamic))` キャストには直前に正当化コメント (PIA gap 等) が必須。
  `scripts/check-dynamic-casts.ps1` で強制される。

### CI / ワークフロー

- CI Gate は `main` への push/PR と `workflow_dispatch` でのみ起動する。
  `ja-localization` では `gh workflow run ci.yml --ref ja-localization` で
  手動起動する。
- Link Check は `_site` を `excel-mcp-japanese-support/` プレフィックス下に
  ミラーリングしてから lychee の `--root-dir` で検査する。GitHub リンクは
  `--remap` で API/raw に書き換え (remap 対象は fork と upstream の両方)、
  確認済みの外部 403/404 (npmjs、MCP Registry、削除済みリポジトリ等) は
  workflow の除外リストで管理。
- GitHub Pages へのデプロイは `deploy-gh-pages.yml`。`gh-pages/audit_site.py`
  が canonical / オフサイトリンク / 生成物 (llms.txt, tools.json 等) を監査。

### リリース

- `release-fork.yml` は GitHub リリースのみ作成し、NuGet / VS Code
  Marketplace / MCP Registry / npm には公開しない。成果物名:
  `excelmcp-ja-*.vsix`、`excel-mcp-ja-*.mcpb`、
  `ExcelMcp-MCP-Server-*-windows.zip`、`ExcelMcp-CLI-*-windows.zip`、
  `excel-skills-ja-*.zip`。
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
