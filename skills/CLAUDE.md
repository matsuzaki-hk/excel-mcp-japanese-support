# ExcelMcp 日本語対応フォーク

本リポジトリは [sbroenne/mcp-server-excel](https://github.com/sbroenne/mcp-server-excel) をフォークし、Excel のテーブル名・シート名・ファイルパスなどの識別子に日本語文字をサポートしたものです。

## 日本語サポートの範囲

- 日本語テーブル名（`[\p{L}_][\p{L}\p{N}_]*` 正規表現）
- 日本語シート名・ワークブック名・ファイルパス
- 日本語シナリオ名・XmlMap 名・QueryTable 名・VBA モジュール名・画像名

## Project Rules (English)

> The following rules are written in English so that agents can interpret them precisely.

### Branch & Commit Rules

- NEVER commit directly to `main`. Use feature branches and pull requests.
- All PRs are squash-merged. Keep feature branches focused and delete after merge.
- Follow the existing commit message style: `type(scope): description`.

### Code Quality

- `TreatWarningsAsErrors=true` is active. Fix all warnings.
- Target .NET 10, C# 14, nullable enabled, implicit usings enabled.
- Prefer `AsSpan()` / `[..n]` over `.AsSpan().ToString()` and `.Substring()`.
- Use specific exception types; avoid generic `catch` clauses.
- Avoid nested `if` statements that can be combined.
- Validate nullable types before access.

### Security

- Do not introduce SQL injection (CA2100), file path injection (CA3003), process injection (CA3006), archive traversal (CA5389), hardcoded encryption (CA5390), or insecure randomness (CA5394).
- Never log or expose secrets, API keys, or connection strings.

### Testing

- Run the relevant test categories for your changes.
- Excel-dependent integration tests require a local Windows Excel instance.
- GitHub-hosted runners have no Excel; Excel-free smoke tests run in CI.

### Documentation

- Update README, skill files, and upstream snapshot docs when user-facing behavior changes.
- Keep tool/operation counts consistent (31 tools / 325 operations as of v2.0.4).

### Japanese Fork Specifics

- Preserve Unicode support for table/sheet/file names. Do not reintroduce ASCII-only regexes.
- Keep `ComUtilities` Unicode normalization (`NormalizationForm.FormC`) intact.
- Do not remove the Japanese manual test assets under `tests/manual/japanese-*/`.
- Use the fork repository `matsuzaki-hk/excel-mcp-japanese-support` in release assets and skill package URLs.
