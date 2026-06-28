param(
    [ValidateSet("full", "changed", "staged")]
    [string]$Mode = "full",

    [switch]$Enforce,
    [switch]$GithubFormat
)

$ErrorActionPreference = "Stop"

function Get-RepoRoot {
    $root = (& git rev-parse --show-toplevel 2>$null)
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($root)) {
        throw "Unable to find git repository root."
    }

    return $root.Trim()
}

function Get-AstGrepCommand {
    if (Get-Command ast-grep -ErrorAction SilentlyContinue) {
        return @{ Exe = "ast-grep"; Prefix = @() }
    }

    if (Get-Command sg -ErrorAction SilentlyContinue) {
        return @{ Exe = "sg"; Prefix = @() }
    }

    if (Get-Command npm -ErrorAction SilentlyContinue) {
        return @{
            Exe = "npm"
            Prefix = @("exec", "--yes", "--package=@ast-grep/cli@0.44.0", "--", "ast-grep")
        }
    }

    throw "ast-grep CLI is not available. Install ast-grep/sg or Node.js+npm."
}

function Invoke-GitLines {
    param([string[]]$Arguments)

    $output = (& git @Arguments 2>$null)
    if ($LASTEXITCODE -ne 0) {
        return @()
    }

    return @($output | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

function Get-ChangedCSharpFiles {
    param([string]$Mode)

    if ($Mode -eq "full") {
        return @()
    }

    if ($Mode -eq "staged") {
        return Invoke-GitLines @("diff", "--cached", "--name-only", "--diff-filter=ACMR", "--", "*.cs")
    }

    if (-not [string]::IsNullOrWhiteSpace($env:GITHUB_BASE_REF)) {
        $baseRef = "origin/$($env:GITHUB_BASE_REF)"
        $mergeBase = (& git merge-base $baseRef HEAD 2>$null)
        if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($mergeBase)) {
            return Invoke-GitLines @("diff", "--name-only", "--diff-filter=ACMR", "$($mergeBase.Trim())...HEAD", "--", "*.cs")
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($env:GITHUB_EVENT_BEFORE) -and $env:GITHUB_EVENT_BEFORE -notmatch "^0+$") {
        $files = Invoke-GitLines @("diff", "--name-only", "--diff-filter=ACMR", "$($env:GITHUB_EVENT_BEFORE)..HEAD", "--", "*.cs")
        if ($files.Count -gt 0) {
            return $files
        }
    }

    $previous = (& git rev-parse --verify HEAD~1 2>$null)
    if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($previous)) {
        return Invoke-GitLines @("diff", "--name-only", "--diff-filter=ACMR", "$($previous.Trim())..HEAD", "--", "*.cs")
    }

    return @()
}

$repoRoot = Get-RepoRoot
Set-Location $repoRoot

$files = Get-ChangedCSharpFiles $Mode
if ($Mode -ne "full" -and $files.Count -eq 0) {
    Write-Host "ast-grep SDD scan: no C# files for mode '$Mode'."
    exit 0
}

$cli = Get-AstGrepCommand
$scanArgs = @("scan", "--config", "sgconfig.yml", "--globs", "*.cs")

if ($GithubFormat) {
    $scanArgs += @("--format", "github")
} else {
    $scanArgs += @("--report-style", "medium")
}

if ($Enforce) {
    $scanArgs += @("--filter", "wpf-shell-no-raw-db-connection", "--error=wpf-shell-no-raw-db-connection")
} else {
    $scanArgs += @("--warning")
}

if ($files.Count -gt 0) {
    $scanArgs += $files
}

$commandArgs = @()
$commandArgs += $cli.Prefix
$commandArgs += $scanArgs

& $cli.Exe @commandArgs
exit $LASTEXITCODE
