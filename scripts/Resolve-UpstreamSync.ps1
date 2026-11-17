# Resolve-UpstreamSync.ps1 — merge upstream into the current branch and resolve
# conflicts deterministically by file class.
#
# Classes:
#   A (theirs + Apply-ForkIdentity): shared files whose fork deltas are a fixed
#     set of identity fields re-applied by scripts/Apply-ForkIdentity.ps1.
#   B (ours): fork-owned files (workflows, skills, JA docs, site config).
#   AGENTS.md: theirs + re-append the fork's JA rules section.
#   everything else: semantic fork code — NOT auto-resolvable → needs_agent.
#
# Outputs (also written to $env:GITHUB_OUTPUT when set):
#   result          merged | needs_agent | already_synced
#   conflict_files  files that could not be auto-resolved (comma separated)
#   review_files    fork-owned files kept despite upstream changes
#   readme_changed  upstream README.md changed since merge base (true/false)
#
# Usage: pwsh scripts/Resolve-UpstreamSync.ps1 [-Ref upstream/main] [-KeepMerge]
#   -KeepMerge: on unresolved conflicts leave the working tree mid-merge (Class A/B
#   already resolved, semantic conflicts left as markers) instead of aborting.
#   The Devin worker uses this to reproduce the merge state and finish by hand.
[CmdletBinding()]
param([string]$Ref = 'upstream/main', [switch]$KeepMerge)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

Set-Location (git rev-parse --show-toplevel).Trim()

$ClassAReapply = @(
    'mcpb/manifest.json',
    'src/ExcelMcp.McpServer/.mcp/server.json',
    'Directory.Build.props',
    'vscode-extension/package.json',
    'vscode-extension/package-lock.json',
    'src/ExcelMcp.McpServer/Tools/ExcelToolsBase.cs',
    'vscode-extension/src/extension.ts',
    '.github/workflows/dependency-review.yml'
)

# Fork-owned files: keep ours verbatim. Entries ending in / are directory prefixes.
# NOTE: shared workflows (ci/codeql/link-check/...) and gh-pages/ merge normally —
# they carry intentional fork deltas but also receive upstream fixes, so conflicts
# there are left for the agent instead of being force-resolved.
$ClassBOurs = @(
    'README.md',
    'skills/',
    'docs/UPSTREAM-README-EN.md',
    'docs/INSTALLATION.md',
    'docs/agents/rules/',
    'PROJECT_MANAGEMENT.md',
    'build-local.ps1',
    '.gitignore',
    '.gitattributes',
    'mcpb/BUILD.md',
    'mcpb/README.md',
    'scripts/Build-AgentSkills.ps1',
    'tests/manual/',
    'vscode-extension/.vscodeignore',
    'vscode-extension/AGENTS.md'
)

# Union-merge files: both sides append (changelog-style); union is safe.
$ClassUUnion = @('CHANGELOG.md')

function Write-Output-Var([string]$Name, [string]$Value) {
    Write-Host "  $Name=$Value"
    if ($env:GITHUB_OUTPUT) { "$Name=$Value" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8 }
}

function Test-ClassA([string]$f) { $ClassAReapply -contains $f }
function Test-ClassB([string]$f) {
    foreach ($x in $ClassBOurs) {
        if ($x.EndsWith('/')) { if ($f.StartsWith($x)) { return $true } }
        elseif ($f -eq $x) { return $true }
    }
    return $false
}
function Test-ClassU([string]$f) { $ClassUUnion -contains $f }

# stage numbers present for an unmerged path (1=base 2=ours 3=theirs)
function Get-Stages([string]$Path) {
    @(git ls-files -u -- $Path | ForEach-Object { ($_ -split '\s+')[2] })
}

# --- already merged? ---
git merge-base --is-ancestor $Ref HEAD
if ($LASTEXITCODE -eq 0) {
    Write-Host "Upstream ref $Ref is already an ancestor of HEAD"
    Write-Output-Var result already_synced
    Write-Output-Var conflict_files ''
    Write-Output-Var review_files ''
    Write-Output-Var readme_changed 'false'
    Write-Output-Var docs_to_review ''
    exit 0
}

$mergeBase = (git merge-base HEAD $Ref).Trim()
$readmeChanged = [bool](git diff --name-only $mergeBase $Ref -- README.md)
# upstream-changed workflow files cannot be pushed by GITHUB_TOKEN (workflows
# permission) → the merge must be finished and pushed by the Devin worker.
$workflowsChanged = @(git diff --name-only $mergeBase $Ref -- '.github/workflows/')
# fork-owned docs that upstream touched since merge-base → translation sync review
$upstreamChanged = @(git diff --name-only $mergeBase $Ref)
$docsToReview = @($upstreamChanged | Where-Object { Test-ClassB $_ -and $_ -match '\.(md|markdown)$' -and $_ -ne 'docs/UPSTREAM-README-EN.md' })

Write-Host "Merging $Ref (merge-base $mergeBase, readme_changed=$readmeChanged)..."
git merge $Ref --no-commit --no-ff 2>&1 | ForEach-Object { Write-Host $_ }

# collect unmerged paths (includes modify/delete states)
$conflicted = @(git ls-files -u | ForEach-Object { ($_ -split "`t")[1] } | Sort-Object -Unique)

$unresolved = @()
$reviewFiles = @()
foreach ($f in $conflicted) {
    $stages = Get-Stages $f
    if ($f -eq 'AGENTS.md') {
        # upstream's file + re-append the fork JA rules section from pre-merge HEAD
        $ours = git show "HEAD:$f" | Out-String
        $section = [regex]::Match($ours, '(?ms)^## 日本語対応フォーク固有ルール.*$').Value
        if (-not $section) { $unresolved += "$f (fork JA section not found)"; continue }
        git checkout --theirs -- $f 2>$null
        if ($LASTEXITCODE -ne 0) { git show "${Ref}:$f" | Out-File -FilePath $f -Encoding utf8 }
        $theirs = [System.IO.File]::ReadAllText($f)
        $nl = if ($theirs -match "`r`n") { "`r`n" } else { "`n" }
        $theirs = $theirs.TrimEnd() + $nl + $nl + $section.TrimEnd() + $nl
        [System.IO.File]::WriteAllText($f, $theirs, [System.Text.UTF8Encoding]::new($false))
        git add -- $f
        $reviewFiles += "$f (theirs + fork JA section re-appended)"
    }
    elseif (Test-ClassA $f) {
        if ($stages -contains '3') {
            git checkout --theirs -- $f
            git add -- $f
            $reviewFiles += "$f (theirs + fork-identity)"
        } else {
            # upstream deleted this identity file — keep ours to preserve fork metadata
            git checkout --ours -- $f
            git add -- $f
            $reviewFiles += "$f (upstream deleted; kept ours - REVIEW)"
        }
    }
    elseif (Test-ClassU $f) {
        git checkout --union -- $f
        git add -- $f
        $reviewFiles += "$f (union)"
    }
    elseif (Test-ClassB $f) {
        if ($stages -contains '2') {
            git checkout --ours -- $f
            git add -- $f
        } else {
            git rm -q -- $f
        }
        $reviewFiles += "$f (ours)"
    }
    else {
        $unresolved += $f
    }
}

# workflow updates need agent-side push credentials even when merge is clean
foreach ($f in $workflowsChanged) { $unresolved += "$f (upstream workflow change - agent push required)" }

if ($unresolved.Count -gt 0) {
    $cf = $unresolved -join ','
    Write-Host "Unresolved (needs agent): $cf"
    if ($KeepMerge) {
        Write-Host "Keeping merge state for manual/agent resolution (-KeepMerge)."
        # enforce identity on the resolved Class A working-tree files, re-stage them
        & "$PSScriptRoot/Apply-ForkIdentity.ps1"
        foreach ($f in $ClassAReapply) { if (Test-Path $f) { git add -- $f } }
    } else {
        git merge --abort 2>$null
        if ($LASTEXITCODE -ne 0) { git reset --hard HEAD | Out-Null }
    }
    Write-Output-Var result needs_agent
    Write-Output-Var conflict_files $cf
    Write-Output-Var review_files ($reviewFiles -join ',')
    Write-Output-Var readme_changed ($readmeChanged.ToString().ToLower())
    Write-Output-Var docs_to_review ($docsToReview -join ',')
    exit 0
}

# all conflicts resolved (or clean merge) — enforce fork invariants
& "$PSScriptRoot/Apply-ForkIdentity.ps1"

# keep the upstream README snapshot current for diff-based JA translation
if ($readmeChanged) {
    git show "${Ref}:README.md" | Out-File -FilePath 'docs/UPSTREAM-README-EN.md' -Encoding utf8
    git add -- 'docs/UPSTREAM-README-EN.md'
}

git add -A
git commit --no-edit | Out-Null
Write-Host ("Merge committed: " + (git rev-parse --short HEAD))

Write-Output-Var result merged
Write-Output-Var conflict_files ''
Write-Output-Var review_files ($reviewFiles -join ',')
Write-Output-Var readme_changed ($readmeChanged.ToString().ToLower())
Write-Output-Var docs_to_review ($docsToReview -join ',')
