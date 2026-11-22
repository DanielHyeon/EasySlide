# Full regression: every harness phase, one run after the other (about 25 minutes). Guards are those of Run-Harness.ps1.
param([string]$Root = "$env:LOCALAPPDATA\AxicultusHarness")
New-Item -ItemType Directory -Force $Root | Out-Null
reg export 'HKCU\Software\EasiSlides' "$Root\reg-before.reg" /y | Out-Null
$runs = [ordered]@{
    'f1-windows' = 'tabs,windows,queue,trash,gap,wlist,chords,bible2,rows'
    'f2-commands' = 'menu,commands,shortcuts,empty'
    'f3-live' = 'live,liveppt,live2'
    'f4-ppt' = 'ppt2,ppt3'
    'f5-ppthang' = 'ppt2hang'
    'f6-editor' = 'editorscan,bodyoffset,splitter'
}
foreach ($name in $runs.Keys) {
    "===== $name ($($runs[$name]))"
    & "$PSScriptRoot\Run-Harness.ps1" -Phases $runs[$name] -Name $name -Root $Root
    $log = "$Root\logs\$name.log"
    if (-not (Test-Path $log)) { 'NO LOG'; continue }
    Select-String -Path $log -Pattern '\[(UNHANDLED\S*|DISPATCHER|ABORT|CRASH|SCENARIO-ABORT|HANG)\]' | Select-Object -First 8 | ForEach-Object { $_.Line.Substring(0, [Math]::Min(300, $_.Line.Length)) }
    Select-String -Path $log -Pattern '\[SUMMARY\]' | ForEach-Object { $_.Line.Substring(0, [Math]::Min(190, $_.Line.Length)) }
    Start-Sleep -Seconds 6
    'POWERPNT after settle: ' + @(Get-Process POWERPNT -ErrorAction SilentlyContinue).Count
}
"===== f7-pptown"
& "$PSScriptRoot\Run-OwnPowerPoint.ps1" -Name 'f7-pptown' -Root $Root
Select-String -Path "$Root\logs\f7-pptown.log" -Pattern '\[OWN\]' | ForEach-Object { $_.Line.Substring(0, [Math]::Min(300, $_.Line.Length)) }
reg export 'HKCU\Software\EasiSlides' "$Root\reg-after.reg" /y | Out-Null
'registry identical: ' + ((Get-FileHash "$Root\reg-before.reg").Hash -eq (Get-FileHash "$Root\reg-after.reg").Hash)
