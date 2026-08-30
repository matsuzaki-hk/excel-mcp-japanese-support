<#
.SYNOPSIS
    Aggregates download counts for release assets in a GitHub repository.

.DESCRIPTION
    Fetches releases via the GitHub API and sums the download_count for each
    release's assets, then prints a sorted table. If -Repository is omitted,
    the current git remote "origin" is used.
#>
param(
    [Parameter()]
    [ValidatePattern("^[^/]+/[^/]+$")]
    [string]$Repository,

    [Parameter()]
    [string]$Token
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
if ($repoRoot) {
    Push-Location $repoRoot
}

try {
    function Get-RepositoryFromOrigin {
        $remoteUrl = git remote get-url origin 2>$null
        if (-not $remoteUrl) {
            throw "Remote origin not found. Specify -Repository owner/repo."
        }

        if ($remoteUrl -match "github\.com[:/](?<owner>[^/]+)/(?<repo>[^/]+?)(?:\.git)?$") {
            return "$($Matches.owner)/$($Matches.repo)"
        }

        throw "Could not extract owner/repo from remote URL: $remoteUrl"
    }

    function Get-GitHubToken {
        if ($Token) { return $Token }
        if ($env:GITHUB_TOKEN) { return $env:GITHUB_TOKEN }

        $ghToken = gh auth token 2>$null
        if ($ghToken) { return $ghToken.Trim() }

        return $null
    }

    if (-not $Repository) {
        $Repository = Get-RepositoryFromOrigin
    }

    $authHeader = @{}
    $resolvedToken = Get-GitHubToken
    if ($resolvedToken) {
        $authHeader["Authorization"] = "Bearer $resolvedToken"
    }

    $headers = @{
        Accept = "application/vnd.github+json"
        "X-GitHub-Api-Version" = "2022-11-28"
        "User-Agent" = "mcp-server-excel-release-downloads"
    } + $authHeader

    $releases = [System.Collections.Generic.List[object]]::new()
    $page = 1

    do {
        $uri = "https://api.github.com/repos/$Repository/releases?per_page=100&page=$page"
        $pageItems = Invoke-RestMethod -Uri $uri -Headers $headers
        $pageItems = @($pageItems)

        foreach ($release in $pageItems) {
            $downloads = if ($release.assets) {
                ($release.assets | Measure-Object -Property download_count -Sum).Sum
            } else {
                0
            }

            $releases.Add([pscustomobject]@{
                Tag = $release.tag_name
                Name = $release.name
                PublishedAt = [DateTimeOffset]$release.published_at
                Downloads = $downloads
            })
        }

        $page++
    } while ($pageItems.Count -eq 100)

    if ($releases.Count -eq 0) {
        Write-Output "No releases found for $Repository"
        return
    }

    $total = ($releases | Measure-Object -Property Downloads -Sum).Sum

    $table = $releases |
        Sort-Object PublishedAt -Descending |
        Format-Table -AutoSize |
        Out-String

    Write-Output $table.Trim()
    Write-Output "Total downloads: $total"
} catch {
    Write-Output "ERROR: $_"
    exit 1
} finally {
    Pop-Location
}
