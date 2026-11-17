# Apply-ForkIdentity.ps1 — take the working-tree version of shared files and
# re-apply the Japanese fork's identity fields. Idempotent: safe to run after
# any merge (clean or resolved), or standalone as an invariant check.
#
# Canonical rules: AGENTS.md 「upstream マージ時に必ず維持するもの」.
# Run from anywhere inside the repository. Requires: pwsh 7+, git.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# git may emit UTF-8 (e.g. non-ASCII repo paths); decode it correctly
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

Set-Location (git rev-parse --show-toplevel).Trim()

$script:Changed = @()
$Utf8NoBom = [System.Text.UTF8Encoding]::new($false)

function Read-Text([string]$Path) { [System.IO.File]::ReadAllText($Path, $Utf8NoBom) }
function Write-Text([string]$Path, [string]$Text) {
    if ($Text -notmatch "`r?`n$") {
        $Text += if ($Text -match "`r`n") { "`r`n" } else { "`n" }
    }
    [System.IO.File]::WriteAllText($Path, $Text, $Utf8NoBom)
}
function ConvertTo-Eol([string]$Text, [string]$Nl) {
    return ($Text -replace "`r`n", "`n") -replace "`n", $Nl
}
function Apply-Edit([string]$Path, [scriptblock]$Edit) {
    if (-not (Test-Path $Path)) { return }
    $before = Read-Text $Path
    $after = & $Edit $before
    if ($after -cne $before) {
        Write-Text $Path $after
        $script:Changed += $Path
        Write-Host "  fork-identity applied: $Path"
    }
}

# Replace the JSON object value of a top-level $Key with $NewRegion verbatim.
# Finds `"key":` then brace-matches to the end of its { ... } value.
function Set-JsonObjectRegion([string]$Text, [string]$Key, [string]$NewRegion) {
    $m = [regex]::Match($Text, '"' + [regex]::Escape($Key) + '"\s*:\s*\{')
    if (-not $m.Success) { return $Text }
    $depth = 0; $i = $Text.IndexOf('{', $m.Index)
    $start = $i
    while ($i -lt $Text.Length) {
        if ($Text[$i] -eq '{') { $depth++ }
        elseif ($Text[$i] -eq '}') { $depth--; if ($depth -eq 0) { $i++; break } }
        $i++
    }
    return $Text.Substring(0, $start) + $NewRegion + $Text.Substring($i)
}

# Remove a `"key": { ... }` object (and the comma before or after it).
function Remove-JsonObjectRegion([string]$Text, [string]$Key) {
    $m = [regex]::Match($Text, '(,\s*)?"' + [regex]::Escape($Key) + '"\s*:\s*\{')
    if (-not $m.Success) { return $Text }
    $depth = 0; $i = $Text.IndexOf('{', $m.Index)
    $end = $i
    while ($end -lt $Text.Length) {
        if ($Text[$end] -eq '{') { $depth++ }
        elseif ($Text[$end] -eq '}') { $depth--; if ($depth -eq 0) { $end++; break } }
        $end++
    }
    $removeStart = $m.Index
    # drop a trailing comma instead when key isn't the first member
    if (-not $m.Groups[1].Success -and $end -lt $Text.Length -and $Text[$end] -eq ',') { $end++ }
    return $Text.Substring(0, $removeStart) + $Text.Substring($end)
}

# --- mcpb/manifest.json ---
Apply-Edit 'mcpb/manifest.json' {
    param($t)
    $nl = if ($t -match "`r`n") { "`r`n" } else { "`n" }
    $t = $t -replace '"name": "excel-mcp"', '"name": "excel-mcp-ja"'
    $t = $t -replace '"display_name": "[^"]*"', '"display_name": "Excel (Windows) - 日本語対応フォーク"'
    $t = $t -replace '"description": "[^"]*"', '"description": "ExcelMcp の日本語対応フォーク。日本語テーブル名・シート名・ファイルパスを含む識別子をサポート。Excel デスクトップ版が必要です。"'
    # long_description: fork sentence goes after "...are preserved." (strip first → idempotent)
    $forkSentence = ' This fork adds support for Japanese table names, sheet names, file paths and other Unicode identifiers.'
    $t = $t -replace [regex]::Escape($forkSentence), ''
    if ($t -match '"long_description": "[^"]*?preserved\.') {
        $t = $t -replace '("long_description": "[^"]*?preserved\.)', ('$1' + $forkSentence)
    } else {
        $t = $t -replace '("long_description": "[^"]*?)(")', ('$1' + $forkSentence + '$2')
    }
    $t = Set-JsonObjectRegion $t 'author' ("{" + $nl + '    "name": "matsuzaki-hk",' + $nl + '    "url": "https://github.com/matsuzaki-hk"' + $nl + '  }')
    $t = $t -replace '"url": "https://github\.com/sbroenne/mcp-server-excel"', '"url": "https://github.com/matsuzaki-hk/excel-mcp-japanese-support"'
    $t = $t -replace '"documentation": "[^"]*"', '"documentation": "https://github.com/matsuzaki-hk/excel-mcp-japanese-support"'
    $t = $t -replace '"support": "[^"]*"', '"support": "https://github.com/matsuzaki-hk/excel-mcp-japanese-support/issues"'
    $t = $t -replace '"privacy_policies": \[\s*"[^"]*"\s*\]', '"privacy_policies": ["https://github.com/matsuzaki-hk/excel-mcp-japanese-support/blob/main/PRIVACY.md"]'
    $serverRegion = ConvertTo-Eol ('{' + "`n" +
        '    "type": "binary",' + "`n" +
        '    "entry_point": "server/excel-mcp-server",' + "`n" +
        '    "mcp_config": {' + "`n" +
        '      "command": "${__dirname}/server/excel-mcp-server",' + "`n" +
        '      "args": [],' + "`n" +
        '      "env": {}' + "`n" +
        '    }' + "`n" +
        '  }') $nl
    $t = Set-JsonObjectRegion $t 'server' $serverRegion
    $t = Remove-JsonObjectRegion $t 'runtimes'
    $t
}

# --- src/ExcelMcp.McpServer/.mcp/server.json ---
Apply-Edit 'src/ExcelMcp.McpServer/.mcp/server.json' {
    param($t)
    $t = $t -replace '"name": "io\.github\.sbroenne/mcp-server-excel"', '"name": "io.github.matsuzaki-hk/excel-mcp-japanese-support"'
    $t = $t -replace '"url": "https://github\.com/sbroenne/mcp-server-excel"', '"url": "https://github.com/matsuzaki-hk/excel-mcp-japanese-support"'
    $t
}

# --- Directory.Build.props ---
Apply-Edit 'Directory.Build.props' {
    param($t)
    $t -replace '<RepositoryUrl>https://github\.com/[^<]+</RepositoryUrl>', '<RepositoryUrl>https://github.com/matsuzaki-hk/excel-mcp-japanese-support</RepositoryUrl>'
}

# --- vscode-extension/package.json ---
Apply-Edit 'vscode-extension/package.json' {
    param($t)
    $nl = if ($t -match "`r`n") { "`r`n" } else { "`n" }
    $t = $t -replace '"name": "excel-mcp"', '"name": "excel-mcp-ja"'
    $t = $t -replace '"displayName": "[^"]*"', '"displayName": "ExcelMcp 日本語対応フォーク"'
    $t = $t -replace '"description": "[^"]*"', '"description": "ExcelMcp の日本語対応フォーク。日本語テーブル名・シート名・ファイルパスをサポートし、日本語 skill を同梱します。Excel デスクトップ版が必要です。"'
    $t = $t -replace '"publisher": "[^"]*"', '"publisher": "matsuzaki-hk"'
    $t = $t -replace '"url": "https://github\.com/sbroenne/mcp-server-excel"', '"url": "https://github.com/matsuzaki-hk/excel-mcp-japanese-support"'
    $t = [regex]::Replace($t, '("bugs": \{\s*"url": ")[^"]*(")', { param($m) $m.Groups[1].Value + 'https://github.com/matsuzaki-hk/excel-mcp-japanese-support/issues' + $m.Groups[2].Value })

    $providers = 'mcpServerDefinitionProviders'
    $providersRegion = '[' + $nl + '      {' + $nl + '        "id": "excel-mcp-ja",' + $nl + '        "label": "ExcelMcp 日本語対応フォーク"' + $nl + '      }' + $nl + '    ]'
    # locate `"mcpServerDefinitionProviders": [` and bracket-match to `]`
    $m = [regex]::Match($t, '"' + $providers + '"\s*:\s*\[')
    if ($m.Success) {
        $depth = 0; $i = $t.IndexOf('[', $m.Index); $start = $i
        while ($i -lt $t.Length) {
            if ($t[$i] -eq '[') { $depth++ }
            elseif ($t[$i] -eq ']') { $depth--; if ($depth -eq 0) { $i++; break } }
            $i++
        }
        $t = $t.Substring(0, $start) + $providersRegion + $t.Substring($i)
    }

    $chatSkills = 'chatSkills'
    $skillsRegion = '[' + $nl + '      {' + $nl + '        "name": "excel-mcp-ja",' + $nl + '        "description": "ExcelMcp 日本語対応フォークの skill。Windows 版 Excel 操作を支援します。",' + $nl + '        "path": "./skills/excel-mcp-ja/SKILL.md"' + $nl + '      },' + $nl + '      {' + $nl + '        "name": "excel-mcp-report-formatting",' + $nl + '        "description": "Optional presentation conventions for requested Excel reports.",' + $nl + '        "path": "./skills/excel-mcp-report-formatting/SKILL.md"' + $nl + '      }' + $nl + '    ]'
    $m = [regex]::Match($t, '"' + $chatSkills + '"\s*:\s*\[')
    if ($m.Success) {
        $depth = 0; $i = $t.IndexOf('[', $m.Index); $start = $i
        while ($i -lt $t.Length) {
            if ($t[$i] -eq '[') { $depth++ }
            elseif ($t[$i] -eq ']') { $depth--; if ($depth -eq 0) { $i++; break } }
            $i++
        }
        $t = $t.Substring(0, $start) + $skillsRegion + $t.Substring($i)
    } else {
        # insert chatSkills right after the mcpServerDefinitionProviders array
        $m = [regex]::Match($t, '"' + $providers + '"\s*:\s*\[')
        if ($m.Success) {
            $depth = 0; $i = $t.IndexOf('[', $m.Index)
            while ($i -lt $t.Length) {
                if ($t[$i] -eq '[') { $depth++ }
                elseif ($t[$i] -eq ']') { $depth--; if ($depth -eq 0) { $i++; break } }
                $i++
            }
            $insert = ',' + $nl + '    "chatSkills": ' + $skillsRegion
            $t = $t.Substring(0, $i) + $insert + $t.Substring($i)
        }
    }
    $t
}

# --- vscode-extension/package-lock.json ---
Apply-Edit 'vscode-extension/package-lock.json' {
    param($t)
    $t -replace '"name": "excel-mcp"', '"name": "excel-mcp-ja"'
}

# --- src/ExcelMcp.McpServer/Tools/ExcelToolsBase.cs ---
Apply-Edit 'src/ExcelMcp.McpServer/Tools/ExcelToolsBase.cs' {
    param($t)
    $nl = if ($t -match "`r`n") { "`r`n" } else { "`n" }
    if ($t -notmatch 'System\.Text\.Encodings\.Web') {
        $t = $t -replace '(?m)^(using System\.Diagnostics;.*)$', ('$1' + $nl + 'using System.Text.Encodings.Web;')
    }
    if ($t -notmatch 'UnsafeRelaxedJsonEscaping') {
        $t = $t -replace [regex]::Escape('Converters = { new JsonStringEnumConverter() }'), (
            'Converters = { new JsonStringEnumConverter() },' + $nl +
            '        // Fork: emit Japanese text unescaped instead of \uXXXX sequences.' + $nl +
            '        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping')
    }
    $t
}

# --- vscode-extension/src/extension.ts ---
Apply-Edit 'vscode-extension/src/extension.ts' {
    param($t)
    $t = $t -replace "const userGuideUrl = 'https://excelmcpserver\.dev/guides/';", "const userGuideUrl = 'https://matsuzaki-hk.github.io/excel-mcp-japanese-support/guides/';"
    # replace bare excel-mcp identifiers that are not already excel-mcp-*
    $t = [regex]::Replace($t, "excel-mcp(?!-)", 'excel-mcp-ja')
    $t
}

# --- .github/workflows/dependency-review.yml ---
# Fork runs the check on ja-localization PRs too; upstream only lists main/develop.
Apply-Edit '.github/workflows/dependency-review.yml' {
    param($t)
    if ($t -match 'ja-localization') { return $t }
    $t2 = $t -replace 'branches:\s*\[\s*"main",\s*"develop"\s*\]', 'branches: [ "main", "develop", "ja-localization" ]'
    if ($t2 -eq $t) {
        # upstream changed the branches list format — flag for manual/agent review
        Write-Host "  WARN: dependency-review.yml branches pattern not matched; add ja-localization manually"
    }
    $t2
}

if ($script:Changed.Count -eq 0) {
    Write-Host "fork-identity: no changes needed (already consistent)"
} else {
    Write-Host ("fork-identity applied to {0} file(s)" -f $script:Changed.Count)
}
