# Makes the isolated copy the harness runs against. The real profile and data are only read.
param(
    [string]$Root = "$env:LOCALAPPDATA\AxicultusHarness",
    [string]$RealProfile = "$env:APPDATA\Axicultus",
    [string]$RealData = 'C:\Axicultus')
$ErrorActionPreference = 'Stop'
$Root = [IO.Path]::GetFullPath($Root)
foreach ($real in $RealProfile, $RealData) {
    if ($Root.StartsWith([IO.Path]::GetFullPath($real), [StringComparison]::OrdinalIgnoreCase)) { "GUARD: $Root lies inside $real"; exit 14 }
}
$pristine = Join-Path $Root 'iso.pristine'
robocopy $RealProfile "$pristine\profile" /MIR /NFL /NDL /NJH /NJS /NP | Out-Null
# Data folders only: the program files next to them are not data.
foreach ($folder in Get-ChildItem $RealData -Directory | Where-Object Name -ne 'runtimes') {
    robocopy $folder.FullName "$pristine\work\$($folder.Name)" /MIR /NFL /NDL /NJH /NJS /NP | Out-Null
}
# The copied settings must point at the copy (iso\work is where Run-Harness.ps1 mirrors the pristine copy to).
$settings = "$pristine\profile\settings.json"
$from = [regex]::Escape($RealData.TrimEnd('\').Replace('\', '\\')) + '(?=\\\\|")'
$to = (Join-Path $Root 'iso\work').Replace('\', '\\')
$json = [IO.File]::ReadAllText($settings)
[IO.File]::WriteAllText($settings, [regex]::Replace($json, $from, { $to }, 'IgnoreCase'), [Text.UTF8Encoding]::new($false))
# The harness refuses a profile without this marker.
Set-Content "$pristine\profile\.axicultus-isolated" "Isolated copy of $RealProfile made $(Get-Date -Format s). Safe to delete."
"isolated copy ready: $pristine"
$global:LASTEXITCODE = 0
