# ExcelMcp agent instructions

ExcelMcp automates installed desktop Excel through COM on Windows. Use
PowerShell 7.4 or later (`pwsh`) and the SDK selected by `global.json`. Desktop Excel is required
for COM tests; GitHub-hosted runners do not have Excel.

MCP Server and `excelcli` are equal entry points: behavior, defaults, validation,
results, and documentation must agree.

ExcelMcp always operates in the current user's Windows session and security
context. Discover and reconnect only to Excel instances accessible in that
context; do not seek access to other users' sessions or elevate permissions.
Workbook location does not change this scope. Running in the user's context
does not authorize changes to unrelated workbooks.

## Code Review Rules

Report high-confidence defects introduced by the change, not style or unrelated
cleanup. Establish a reachable failure, its affected caller/workbook state, and
the missing or misleading test evidence. Review does not require builds,
dependency installation, or edits unless requested.

- Trace changed catches and fallbacks through their caller. Logging an error
  then returning empty/default data, skipping a failed item, or reporting success
  still hides failure. Optional COM reads may suppress only exact documented
  unavailable-member HRESULTs; `0x800A03EC` is not a missing-property diagnosis.
  Unexpected COM errors must fail with their context.
- Check failure ownership: Core leaves execution errors to batch/Service;
  transport boundaries may convert failures, not erase them. Preserve original
  cause, cancellation, timeout, session recovery, and exact partial state.
  Cancellation is not rollback; reject unrequested workbook cleanup.
- Review COM ownership across every exit and failure path. Acquired references
  need `finally` release, not session-owned objects. Process cleanup needs
  PID/start-time identity and retained handles. EXL011 and EXL005 reject dynamic
  declarations and late-bound calls in production and test builds.
- Follow changed source contracts through generated Service, CLI options/batch
  JSON, MCP schemas, and manual exceptions. Names, defaults, validation, results,
  and timeouts must agree. Adapters must not acquire Core/session ownership.
- Check MCP protocol output, including bootstrap paths, not just direct console
  calls. Telemetry must never wait on setup, export, flush, or disposal on product
  paths, delay incremental results/cancellation/exit, or alter results/errors.
  Bound queues; verify local delivery separately from responsiveness and privacy.
- Check guidance against implemented inputs, defaults, and capabilities.
  Distinguish MCP snake_case inputs, CLI flags, nested JSON keys, and outputs.
  Flag nonexistent actions, forced unrequested work, emojis, required unattended
  screenshots, and consent prose presented as implemented server elicitation.
- Tests must establish returned fields and actual Excel state, including the
  partial state after failure; success alone or mocked COM is not evidence.
  Compiler checks catch supported patterns, not correct HRESULT semantics,
  complete COM ownership, runtime state, or matching entry-point behavior.
- Use `ponytail-review` when available after correctness/security review for
  concrete behavior-preserving simplifications. Required COM cleanup,
  validation, failure reporting, tests, and generated contracts are not bloat.

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
| `src/**/*.cs` | [Runtime boundaries](src/AGENTS.md) |
| Core commands/action models, Service, CLI, MCP, generators, or `.github/usage-analytics-weights.json` | [Generated contracts](src/ExcelMcp.Core/AGENTS.md) |
| Core or ComInterop C# | [COM safety](src/ExcelMcp.ComInterop/AGENTS.md) |
| Connection commands/sanitizer or connection tests/helpers/fixtures | [Connections](src/ExcelMcp.Core/Commands/Connection/AGENTS.md) |
| `tests/**/*.cs` | [Testing strategy](tests/AGENTS.md) |
| Workflows, project files, SDK/build configuration, PowerShell scripts, `.changeset/**` | [Build and release](scripts/AGENTS.md) |
| README/index files, FEATURES, CHANGELOG, SECURITY, PRIVACY, docs, specs, skills, or website | [Documentation](docs/AGENTS.md) |
| Skill sources, Core command metadata, generators, MCP, or Build-AgentSkills.ps1 | [Product agent guidance](skills/AGENTS.md) |
| MCP Server or MCP generator | [MCP boundaries](src/ExcelMcp.McpServer/AGENTS.md) |
| `llm-tests/**` | [LLM evaluations](llm-tests/AGENTS.md) |
| Repository/agent instructions, CONTEXT, or `docs/agents/**` | [Instruction maintenance](docs/AGENTS.md#instruction-maintenance) |
| `vscode-extension/**` | [Extension](vscode-extension/AGENTS.md) |
| `videos/excel-mcp-intro/**` | [Video](videos/excel-mcp-intro/AGENTS.md) |
| `videos/agentic-world-bank-briefing/**` | [Agentic workflow video](videos/agentic-world-bank-briefing/AGENTS.md) |

## Implementation

The build enforces supported COM, contract, result, dynamic-access, and test
patterns. Fix diagnostics at their source; a suppression needs a reason, not an
assumption that passing compilation proves runtime correctness.

- Use typed Excel interop in production and tests. EXL011 rejects dynamic
  declarations and late-bound operations; EXL005 rejects casts to `dynamic`.
  Comments do not exempt them. The compiler checks whether the referenced PIA
  exposes a member; runtime availability still needs real-Excel validation.
- The user-approved document-property exception is confined to
  `ExcelDocumentProperties` in ComInterop. Its private native COM calls cover
  only workbook property collections/items, resolve member identifiers from
  Excel, and validate item identities. Do not reuse it for other features or
  add a general member-name, reflection, or dynamic fallback. Compiler checks
  remain enabled everywhere.

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
  an Excel workbook file as a ZIP/OOXML package or parse/modify its internal XML
  outside tests. The isolated Power Query user-settings archive reader is not a
  workbook-content path. Pre-open binary container detection may read only
  IRM/AIP protection metadata; it must not parse workbook content.
- Behavioral changes require a focused failing regression test before the fix.
  Documentation/configuration-only changes do not need synthetic tests.
- MCP Server and `excelcli` are the only supported product entry points. Do not
  preserve or add public Core/ComInterop compatibility APIs for hypothetical
  external consumers when neither entry point uses them.
- Keep customer/workbook data, credentials, connection strings, and private
  paths out of public artifacts. Keep temporary notes outside the repository.
- ExcelMcp acts for the current user. Task-relevant workbook contents, names,
  formulas, VBA/Power Query code, and file paths may be read and returned to
  that user through supported operations; do not hide them merely because
  they are private. This is not public disclosure or permission to access
  unrelated workbooks. Credential protections still apply; do not put private
  information in public artifacts or usage reporting.
- Returning an owned Excel dialog's visible text to its user for troubleshooting
  is allowed; this is not public disclosure. Do not copy it into public artifacts
  or telemetry, read password/input values, or answer the dialog automatically.

## Errors

- Handle failures only where the layer owns recovery or failure transport.
  Preserve the original exception and operation context. Optional reads must not
  treat arbitrary COM failures as absent data. A deliberate best-effort boundary
  must explain what failed and why continuing cannot misrepresent the operation.
  Cleanup must preserve the primary failure as well as any cleanup failure.
- Do not automatically clean up or roll back workbook changes when an
  operation fails. Report the exact partial state and error so the agent can
  inspect the workbook and decide whether cleanup is appropriate.

## Build and validation

For telemetry development, use the local Aspire dashboard first; see
[local telemetry development](docs/DEVELOPMENT.md#local-development-with-telemetry).
Verify actual logs/traces from both `excelcli` and MCP, including short-lived CLI
commands, as well as non-blocking behavior when the receiver is unavailable or
stalled. Passing in-memory tests or fast shutdown alone does not prove delivery.
If Aspire is unavailable, report the blocker; do not fall back to production.
Sending test telemetry to production Azure or changing its ingestion rules
requires explicit user approval for that specific test or deployment; available
credentials are not approval.

```powershell
dotnet restore Sbroenne.ExcelMcp.sln
dotnet build Sbroenne.ExcelMcp.sln -c Release --no-restore
```

Build with zero warnings. Use targeted tests; see the
[testing strategy](tests/AGENTS.md). Run every Excel-dependent test command
sequentially; never overlap Excel test fixtures, test hosts, or E2E runs.
You may close or stop any Excel copy on the machine, for example one left by a
test run; test code still must not. Report each copy you closed by process
number and start time.
The commit hook builds selected inputs and runs selected `Fast` unit tests from
CLI, MCP Server, Core, and Service (`Commit` group), excluding
`AdapterTestKind=System` and `Speed=Slow`. ComInterop-specific, process, tooling,
package, Excel, and acceptance checks do not run on
every commit. CI owns hosted checks; run local process checks when changing
their boundary, not on every push. A passing hook is not complete PR validation.
Local validation must follow the changed behavior, not merely the directory
containing a changed file. Start with a focused regression and the existing
tests for affected callers and failure paths. Run relevant Excel tests locally
when workbook behavior or Excel ownership changes; build-only checks do not
cover COM. Shared service, routing, or lifecycle files do not by themselves
require all feature tests, complete acceptance, or all ComInterop diagnostics.
Expand coverage when the change affects a shared contract or lifetime,
focused tests reveal wider failures, or a specific risk is not covered.
Run the full suite only when that wider scope is justified or explicitly requested.

Use `scripts\Get-ValidationPlan.ps1` to identify candidate checks, then review
the selection against the actual change. If it selects unrelated features,
run narrower existing test filters or runner groups and record the commands,
results, and reason for the narrower scope in the PR. Do not describe an
unrun broad selection as passed. This local scope decision does not waive
Git hooks or required CI checks. Unknown inputs must fail selection, not select
everything. `scripts\Test-E2E.ps1` remains the explicit complete acceptance
command; run it on a local Windows desktop with Excel.

The Python evaluation suite under `llm-tests/` is on-demand only, not part of the
normal development lifecycle. This includes offline checks, Excel fixture checks,
SDK discovery, and live agent comparisons. Run it and install its dependencies
only when the user explicitly requests evaluation work. Unrun evaluations or
unavailable Python/SDK dependencies are not implementation, commit, PR, or merge blockers.
This does not waive required .NET/COM/E2E validation or normal Git hooks.

Generate a candidate plan, review its scope, and run applicable existing checks,
not replacement audits. Run the complete plan only when its selection is relevant:

```powershell
& .\build.ps1 plan --base-ref origin/main --output artifacts\validation-plan.json
& .\build.ps1 validate --plan artifacts\validation-plan.json
```

PRs record the root cause, affected contracts, and validation.

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
- Before opening a PR or pushing further changes to one, run an independent
  review of the complete branch diff in a fresh context (for example, a
  code-review subagent) against the Code Review Rules. Fix confirmed findings
  and repeat until no high-confidence findings remain. After that review, run
  `ponytail-review` when available on the complete branch diff, following its
  scope in the Code Review Rules. Then verify that the PR description and
  changeset claim only what the diff does, and search the whole repository,
  including samples, workflows, and docs, for stale
  references to anything renamed or removed.
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
- Unchanged plugin output must not create publication commits/tags. Awesome
  Copilot listings are edited manually using the actual published output commit;
  follow `.github/workflows/docs/publish-plugins-setup.md#awesome-copilot-listings-are-manual`.

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

- 公開サーフェスは upstream の `doc-counts.json` が正準 (v2.5.7 時点で
  33 tools / 429 operations)。カウント検証は upstream v2.5.x で
  `scripts/check-doc-counts.ps1` から `src/ExcelMcp.Analyzers` の
  EXL030 (DocumentationAnalyzer) へ移り、Release ビルド時にコンパイラ
  診断として強制される。カウント変更時は EXL030 の期待値どおりに
  doc-counts.json と各ドキュメント表記を同期する。
- `((dynamic))` キャストには直前に正当化コメント (PIA gap 等) が必須。
  upstream v2.5.x で `src/ExcelMcp.Analyzers` の SafetyAnalyzer による
  コンパイル時診断に移行した。

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
