# Excel MCP 日本語対応プロジェクト管理ドキュメント

## プロジェクトの目的と方針

### 目的
本プロジェクトは、sbroenne/mcp-server-excel（本家）のフォークとして、日本語ユーザー向けに以下の機能を提供することを目的としています：

1. **日本語テーブル名サポート**: Unicode文字（日本語）を使用したテーブル名、シート名、識別子の作成を可能にする
2. **日本語SKILLファイル**: AIアシスタントが日本語でガイダンスを受け取れるようにSKILLファイルを日本語化
3. **本家の更新追従**: 本家の機能更新を取り込みつつ、日本語対応を維持する

### 方針
- **本家との整合性を維持**: 可能な限り本家のコードを変更せず、日本語対応に必要な最小限りの変更にとどめる
- **日本語化の範囲を明確に定義**: SKILLファイル、ドキュメント、ユーザー向けメッセージを日本語化
- **リベース戦略による更新追従**: 本家の更新を安全に取り込むためにリベース戦略を採用
- **コミュニケーションを日本語で行う**: ドキュメント、コミットメッセージ、ユーザーとの対話を日本語で行う

## 日本語化の範囲とアプローチ

### 日本語化の対象
1. **SKILLファイル** (skills/shared/配下の21ファイル)
   - behavioral-rules.md
   - gotchas.md
   - anti-patterns.md
   - chart.md
   - table.md
   - conditionalformat.md
   - dashboard.md
   - datamodel.md
   - dmv-reference.md
   - excel_agent_mode.md
   - m-code-syntax.md
   - pivottable.md
   - powerquery.md
   - range.md
   - screenshot.md
   - slicer.md
   - window.md
   - workflows.md
   - worksheet.md

2. **SKILLテンプレート** (skills/templates/配下の2ファイル)
   - SKILL.mcp.sbn
   - SKILL.cli.sbn

3. **生成されたSKILLファイル**
   - skills/excel-mcp/SKILL.md

4. **プロジェクトドキュメント**
   - README.md
   - CHANGELOG.md
   - PROJECT_MANAGEMENT.md (本ドキュメント)

### 日本語化のアプローチ
- **技術的正確性を維持**: コードブロック、JSONスニペット、CLIコマンド例は英語のまま保持
- **用語の統一**: 技術用語は英語のまま使用し、説明文のみを日本語化
- **Scriban構文の保持**: テンプレート内のScriban変数（{{ operationcount }}など）は変更しない
- **フォーマットの維持**: マークダウンの構造、見出し、リスト形式を維持

### 日本語化しないもの
- ソースコード（C#ファイル）
- テストコード
- ビルドスクリプト
- GitHub Actionsワークフロー
- パッケージマニフェスト

## Gitブランチ戦略

### ブランチ構造
```
main                ← 本家追従用（英語のみ）
  ↑
  │ (リベース)
  │
ja-localization    ← 日本語化ブランチ
  ↑
  │ (リベース)
  │
feature/*          ← 機能開発ブランチ
```

### 各ブランチの役割
- **main**: 本家のupstream/mainと完全に同期。日本語化の変更を含まない
- **ja-localization**: 日本語化の変更を含むブランチ。mainからリベースして本家の更新を取り込む
- **feature/*****: 新機能開発用。ja-localizationからブランチを作成

### ユーザー向け注意事項
GitHubで自分の変更を確認する場合、**ja-localizationブランチ**を見る必要があります。mainブランチは本家との同期用であり、日本語化の変更は反映されません。

### リベース戦略の採用理由
- **履歴のクリーンさ**: マージコミットが増えない
- **競合解決の容易さ**: 本家の変更が最新の状態で反映される
- **履歴の直線性**: 日本語化の変更履歴が追跡しやすい

### 本家の更新追従手順
```bash
# 1. 本家の変更を取得
git fetch upstream

# 2. mainを本家の状態にリセット
git checkout main
git reset --hard upstream/main

# 3. ja-localizationをmainにリベース
git checkout ja-localization
git rebase main

# 4. 競合解決
# - 新しいファイルが追加された場合: 日本語化が必要
# - 既存ファイルが変更された場合: 内容を確認し、必要に応じて日本語訳を更新
# - 日本語化されたファイルが削除された場合: 本家の意図を確認

# 5. プッシュ
git push origin ja-localization --force-with-lease
```

### 新機能追加時のワークフロー
```bash
# 1. ja-localizationから機能ブランチを作成
git checkout ja-localization
git checkout -b feature/new-feature

# 2. 機能を開発
# - 新しい機能を実装
# - 必要に応じてSKILLファイルを更新
# - 該当する場合は日本語化

# 3. コミット
git add .
git commit -m "feat: 機能説明"

# 4. 本家と同期
git checkout main
git fetch upstream
git reset --hard upstream/main

git checkout ja-localization
git rebase main

git checkout feature/new-feature
git rebase ja-localization

# 5. ja-localizationにマージ
git checkout ja-localization
git merge feature/new-feature

# 6. プッシュ
git push origin ja-localization --force-with-lease
```

## 開発の作法と方向性

### コード変更の原則
- **最小限の変更**: 日本語対応に必要な最小限のコード変更にとどめる
- **本家のパターンを尊重**: 本家のコーディングスタイル、命名規則、アーキテクチャを尊重する
- **変更の分離**: 日本語対応の変更と機能追加の変更を分離する

### SKILLファイルの翻訳原則
- **意図の正確な伝達**: 元の英語の意図を正確に日本語に翻訳する
- **技術用語の保持**: 技術用語（PivotTable、Data Model、DAXなど）は英語のまま使用
- **簡潔な表現**: 冗長な表現を避け、簡潔で明確な日本語を使用する
- **一貫性**: 用語、表現スタイルを一貫して維持する

### ドキュメントの作法
- **日本語と英語の併記**: 重要なセクションは日本語と英語を併記する
- **コードブロックの維持**: コード例は英語のまま維持し、説明文を日本語化
- **構造の維持**: 見出し、リスト、セクション構造を維持する

### テストと検証
- **機能テスト**: 日本語テーブル名が正常に動作することを確認
- **SKILLテスト**: 日本語化されたSKILLファイルがAIアシスタントに正しく認識されることを確認
- **ビルドテスト**: ビルドプロセスが正常に動作することを確認

## 方向性の明確化

### 優先順位
1. **本家の更新追従**: 本家の機能更新を優先的に取り込む
2. **日本語化の維持**: 日本語化を維持しつつ本家の更新を反映する
3. **新機能の追加**: 本家にない日本語固有の機能を追加する（必要な場合のみ）

### 範囲外の作業
- **本家へのPR**: 本家へのプルリクエストは行わない（これはフォークプロジェクト）
- **自動同期の実装**: 手動同期を前提とする（自動同期ワークフローの実装は行わない）
- **本家のバグ修正**: 本家のバグ修正は本家のリポジトリで行うべき

### 成功の定義
- 日本語ユーザーが日本語テーブル名でExcel操作が可能であること
- AIアシスタントが日本語でガイダンスを受け取れること
- 本家の更新を安全に取り込めること
- ドキュメントが最新の状態で維持されていること

## コミュニケーションとコラボレーション

### GitHubリポジトリ情報
- **本家リポジトリ**: https://github.com/sbroenne/mcp-server-excel
- **フォークリポジトリ**: https://github.com/matsuzaki-hk/excel-mcp-japanese-support
- **リモート名**:
  - `upstream`: 本家リポジトリ（sbroenne/mcp-server-excel）
  - `origin`: フォークリポジトリ（matsuzaki-hk/excel-mcp-japanese-support）

### ユーザーとの対話
- **日本語で応答**: ユーザーとの対話は常に日本語で行う
- **簡潔な説明**: 技術的な説明は簡潔かつ明確に行う
- **状況の共有**: 進捗、問題、決定事項を明確に共有する

### 本家との関係
- **フォークとしての位置づけ**: 本家のフォークとしての位置づけを明確にする
- **クレジットの帰属**: 本家の作者と貢献者へのクレジットを明確にする
- **バグ報告**: 本家のバグは本家のリポジトリで報告する

## ドキュメントの管理

### ドキュメントの更新タイミング
- **機能追加時**: 新機能を追加した際、関連するドキュメントを更新する
- **本家の更新時**: 本家の更新を取り込んだ際、ドキュメントを更新する
- **方針の変更時**: 方針やアプローチを変更した際、本ドキュメントを更新する

### ドキュメントの構造
- **PROJECT_MANAGEMENT.md**: 本ドキュメント（プロジェクト管理用）
- **README.md**: プロジェクトの概要と使用方法
- **CHANGELOG.md**: 変更履歴
- **FEATURES.md**: 機能の詳細なリファレンス

## リスク管理

### 主なリスク
1. **本家との乖離**: 本家の更新に追従できなくなるリスク
2. **日本語化の不整合**: 日本語化が不完全または不正確になるリスク
3. **競合の複雑化**: 競合解決が複雑化するリスク

### リスク軽減策
- **定期的な同期**: 定期的に本家の更新を取り込む
- **小規模な変更**: 変更を小規模に保ち、競合を最小限にする
- **ドキュメントの更新**: ドキュメントを常に最新の状態に保つ
- **バックアップ**: 重要な変更前にバックアップを作成する

## MCPサーバー使用ルール

### 基本ルール
- **excel-mcp-forFILES-DEBUGの使用**: 本プロジェクトでの開発・テストでは、明示的に指定しない限り`excel-mcp-forFILES-DEBUG`のみを使用する
- **他のMCPサーバーの使用禁止**: `excel-mcp-forFILES`や`excel-mcp-forVBA`などの他のMCPサーバーは、ユーザーの明示的な許可なしに使用しない
- **使用許可の要求**: 他のMCPサーバーを使用が必要と判断した場合は、必ず事前にユーザーに使用許可を求める

### 理由
- **最新ビルドの確認**: `excel-mcp-forFILES-DEBUG`は最新ビルドのMCPサーバーを指しており、開発中の機能を正しくテストするために必要
- **意図しない動作の防止**: 古いバージョンや異なる設定のMCPサーバーを使用することで、意図しない動作や誤ったテスト結果が発生するのを防止
- **開発の一貫性**: 常に最新ビルドを使用することで、開発の一貫性を維持

### MCPサーバー設定
- **設定ファイル**: `c:\Users\avalo\.codeium\windsurf-next\mcp_config.json`
- **excel-mcp-forFILES-DEBUGパス**: `C:\work\ExcelMcp-MCP-Server\mcp-excel-260521-v4.exe`（最新ビルド）
- **バージョン管理**: ビルドごとにバージョン番号を更新し、最新ビルドを指すように設定を更新する

### ビルドとpublishルール
- **変更後は必ずpublishを実行**: コード変更後は必ず`dotnet publish`を実行してEXEを作成する
- **publishコマンド**: `dotnet publish src/ExcelMcp.McpServer/ExcelMcp.McpServer.csproj --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false -p:PublishReadyToRun=false -p:NuGetAudit=false --output C:\temp\ExcelMcpPublish`
- **成果物のコピー**: publish後、`C:\temp\ExcelMcpPublish\Sbroenne.ExcelMcp.McpServer.exe`を`C:\work\ExcelMcp-MCP-Server\`にコピーし、バージョン番号を更新する
- **バージョン番号の更新**: 現在のバージョン番号を確認し、次のバージョン番号（例：1.7.1 → 1.7.2）を付けてコピーする
- **mcp_config.jsonの更新**: 新しいEXEを指すように`mcp_config.json`を更新する
- **MCPサーバーの再起動**: mcp_config.json更新後、MCPサーバーを再起動する

### デプロイ手順（必須）
publish成功後、以下の手順を必ず実行する：
1. 成果物をコピー: `$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"; Copy-Item "C:\temp\ExcelMcpPublish\Sbroenne.ExcelMcp.McpServer.exe" "C:\work\ExcelMcp-MCP-Server\mcp-excel-260521-$timestamp.exe" -Force`
2. 最新のEXEを確認: `Get-ChildItem "C:\work\ExcelMcp-MCP-Server\mcp-excel-260521-*.exe" | Sort-Object LastWriteTime -Descending | Select-Object -First 1`
3. mcp_config.jsonを更新: `excel-mcp-forFILES-DEBUG`の`command`を最新のEXEに更新する
4. MCPサーバーを再起動: Windsurfの設定またはコマンドパレットからMCPサーバーを再起動する

**重要**: 成果物のコピー時に末尾にビルド日時を付与して既存ファイルと重複しないようにする

## まとめ

このプロジェクトは、日本語ユーザー向けにExcel MCPの日本語対応を提供することを目的としています。本家の更新を安全に取り込みつつ、日本語化を維持するためにリベース戦略を採用し、明確なブランチ構造と運用手順を定めています。すべての作業はこの方針と作法に従って行われます。
