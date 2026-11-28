#Requires -Version 7.4
# Makes the isolated copy of the Axicultuss settings and data the harness runs against
# (%LOCALAPPDATA%\AxicultussHarness\iso.pristine). The real settings file and data folders are only read.
#
# How a real folder is kept from being written: every write goes through HarnessSafety.ps1, whose Assert-HarnessTarget
# refuses any target that is not below the harness root, has a reparse point on its path, or overlaps a protected folder
# (the real profiles, C:\Axicultuss, C:\Axicultus, C:\EasiSlides, the worship data and church OneDrive folders on D: and the real data root
# read from the settings). Real paths are only ever robocopy SOURCES; the copy goes into a destination that does not
# exist yet, so there is no /MIR or /PURGE here, and the only delete (the previous iso.pristine) is a harness folder
# checked the same way. The sources are hashed before and after the copy.
#
# Exit codes: 0 ready, 11 an app is running, 16 a system folder is unknown, 20 a path check refused, 21 no real settings
# file, 22 no usable root_directory, 23 a copy failed, 24 the rewritten settings still name the real root,
# 30 REAL DATA CHANGED during the copy, 31 a real folder could not be read, 41 script error.
trap { "SCRIPT ERROR: $_"; exit 41 }
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\HarnessSafety.ps1"

# A running app may be writing the database and the settings file while they are read.
$running = @(Get-Process | Where-Object { $_.ProcessName -match '^(Axicultuss|Axicultus|Easislides|Axicultuss\.Tests|Axicultus\.Tests)$' })
if ($running) { Stop-Guard 11 "app instance running ($(($running | ForEach-Object { "$($_.ProcessName)#$($_.Id)" }) -join ', ')); not copying" }

$root = Get-HarnessRoot
$realProfile = Get-RealProfile
$realSettings = Join-Path $realProfile 'settings.json'
if (-not (Test-Path -LiteralPath $realSettings -PathType Leaf)) { Stop-Guard 21 "no settings file at $realSettings" }

$json = [IO.File]::ReadAllText($realSettings)
$rootText = Get-SettingsRootDirectory $json
if (-not $rootText) { Stop-Guard 22 "config\root_directory is missing from $realSettings" }
$realRoot = Resolve-PathLike $rootText
if (-not $realRoot -or -not (Test-Path -LiteralPath $realRoot -PathType Container)) { Stop-Guard 22 "root_directory '$rootText' is not an existing local folder" }

$protected = Get-ProtectedFolders @($realProfile, $realRoot)
Assert-HarnessRoot $root $protected
$pristine = Join-Path $root 'iso.pristine'
# The copy is used from iso (Run-Harness.ps1 mirrors iso.pristine there), so its paths name iso\work.
$runWork = Join-Path $root 'iso\work'

$sources = @(Get-DataFolderNames | ForEach-Object { Join-Path $realRoot $_ } | Where-Object { Test-Path -LiteralPath $_ -PathType Container })
$checks = @([pscustomobject]@{ Path = $realProfile; Content = $true }) +
    @($sources | ForEach-Object { [pscustomobject]@{ Path = $_; Content = ((Split-Path $_ -Leaf) -eq 'Admin') } })
$before = @($checks | ForEach-Object { Get-TreeHash $_.Path -Content:$_.Content })

# The old marker goes first: a copy whose making fails halfway never carries one.
$oldMarker = "$pristine\profile\.axicultuss-isolated"
Assert-HarnessTarget $oldMarker $root $protected
if (Test-Path -LiteralPath $oldMarker) { [IO.File]::Delete((Get-NormalPath $oldMarker)) }
Remove-HarnessTree $pristine $root $protected
$copied = @()
foreach ($source in $sources) {
    $name = Split-Path $source -Leaf
    Copy-HarnessTree $source "$pristine\work\$name" $root $protected
    $copied += $name
}
$skipped = @(Get-ChildItem -LiteralPath $realRoot -Directory -Force | Where-Object Name -notin $copied | ForEach-Object Name)

# The copied settings point at the copy: every path under the real root, in any spelling (root_directory, export_dir,
# media_dir, ...). Only the rewritten text is written: no copy naming the real root ever exists in the harness folder.
$rewritten = Convert-SettingsRoot $json $realRoot $runWork
$left = @(Find-RealPathStrings @(Get-SettingsStrings $rewritten) @($realRoot, $realProfile))
if ($left) { Stop-Guard 24 "the rewritten settings still name a real folder: $($left[0])" }
if (-not (Test-Within (Resolve-PathLike (Get-SettingsRootDirectory $rewritten)) $runWork)) { Stop-Guard 24 'the rewritten root_directory is not in the copy' }
Write-HarnessFile "$pristine\profile\settings.json" $rewritten $root $protected


$after = @($checks | ForEach-Object { Get-TreeHash $_.Path -Content:$_.Content })
$compared = Compare-Hashes $checks $before $after
$compared.Lines

foreach ($name in $copied) {
    $files = @(Get-ChildItem -LiteralPath "$pristine\work\$name" -Recurse -File -Force)
    "copied {0,-12} {1,8:N1} MB {2,5} files" -f $name, (($files | Measure-Object Length -Sum).Sum / 1MB), $files.Count
}
if ($skipped) { "not copied (program folders, or not a data folder the app reads): $($skipped -join ', ')" }
"real folders the copy names (Run-Harness.ps1 checks them before and after, or says why not):"
Get-NamedChecks $pristine $root (Get-NormalPath "$PSScriptRoot\..\..") | ForEach-Object { "  $($_.Path)$(if ($_.Skip) { " (not checked: $($_.Skip))" } elseif ($_.Why -ne 'named by the copy') { " ($($_.Why))" })" }
# A copy that failed its check gets no marker, so Run-Harness.ps1 refuses it (12).
if ($compared.State -eq 'changed') { 'REAL DATA CHANGED while copying: find the cause before running the harness (no marker written)'; exit 30 }
if ($compared.State -ne 'ok') { 'a real folder could not be read: the copy was not checked (no marker written)'; exit 31 }

# Written last, only after the check: the driver and Run-Harness.ps1 refuse a profile without this marker. It also tells
# them which real folders the copy came from.
$marker = [ordered]@{
    note = 'Isolated copy for tools\AxicultussHarness. Safe to delete.'
    made = (Get-Date -Format s)
    realProfile = $realProfile
    realRoot = $realRoot
    dataFolders = $copied
} | ConvertTo-Json
Write-HarnessFile "$pristine\profile\.axicultuss-isolated" $marker $root $protected
"isolated copy ready: $pristine (real root $realRoot)"
exit 0
