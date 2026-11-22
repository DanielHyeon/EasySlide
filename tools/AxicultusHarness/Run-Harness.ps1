# Runs the harness against a fresh mirror of the isolated copy. The guards follow the test-safety rules for the
# machine that holds the church's real data; never relax them.
param(
    [string]$Phases = 'windows',
    [string]$Name = 'run',
    [string]$Policy = 'cancel',
    [switch]$KeepState,
    [string]$Root = "$env:LOCALAPPDATA\AxicultusHarness")
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
$screens = [System.Windows.Forms.Screen]::AllScreens
if ($screens.Count -lt 2) { "GUARD: only $($screens.Count) screen(s) present; not launching"; exit 10 }
$running = Get-Process | Where-Object { $_.ProcessName -match '^(Axicultus|Axicultuss|Easislides)$' }
if ($running) { "GUARD: app instance already running ($($running.ProcessName -join ',')); not launching"; exit 11 }
if (-not (Test-Path "$Root\iso.pristine\profile\.axicultus-isolated")) { "GUARD: no isolated copy at $Root; run New-IsolatedCopy.ps1 first"; exit 12 }

$env:Path = [Environment]::GetEnvironmentVariable('Path', 'Machine') + ';' + [Environment]::GetEnvironmentVariable('Path', 'User')
dotnet build "$PSScriptRoot\Harness.csproj" -nologo -v quiet | Select-String -Pattern ' error ' | Select-Object -Unique -First 10
if ($LASTEXITCODE -ne 0) { 'BUILD FAILED; not launching'; exit 13 }

function Get-TreeHash($root) {
    # A folder that is gone must not read as an unchanged empty one.
    if (-not (Test-Path $root)) { return 'MISSING' }
    $files = Get-ChildItem $root -Recurse -File -ErrorAction SilentlyContinue | Sort-Object FullName
    $sb = [Text.StringBuilder]::new()
    foreach ($f in $files) { [void]$sb.Append($f.FullName).Append('|').Append($f.Length).Append('|').Append($f.LastWriteTimeUtc.Ticks).Append("`n") }
    $bytes = [Text.Encoding]::UTF8.GetBytes($sb.ToString())
    ([BitConverter]::ToString([Security.Cryptography.SHA256]::HashData($bytes))).Replace('-', '').Substring(0, 16) + " ($($files.Count) files)"
}
$real = @("$env:APPDATA\Axicultus", 'C:\Axicultus\Admin', 'C:\EasiSlides\Admin')
$before = $real | ForEach-Object { Get-TreeHash $_ }
$pptBefore = @(Get-Process POWERPNT -ErrorAction SilentlyContinue).Count

if (-not $KeepState) { robocopy "$Root\iso.pristine" "$Root\iso" /MIR /NFL /NDL /NJH /NJS /NP | Out-Null }
$log = "$Root\logs\$Name.log"
New-Item -ItemType Directory -Force "$Root\logs" | Out-Null
$exe = "$PSScriptRoot\bin\Debug\net11.0-windows7.0\Axicultus.Tests.exe"
$p = Start-Process -FilePath $exe -ArgumentList "`"$Root\iso\profile`"", "`"$Root\iso\work`"", "`"$log`"", $Phases, $Policy -PassThru
if (-not $p.WaitForExit(900000)) { "TIMEOUT: killing harness"; $p.Kill() }
"exit=$($p.ExitCode) log=$log"

$after = $real | ForEach-Object { Get-TreeHash $_ }
for ($i = 0; $i -lt $real.Count; $i++) { if ($before[$i] -ne $after[$i]) { "REAL DATA CHANGED: $($real[$i]) $($before[$i]) -> $($after[$i])" } else { "unchanged: $($real[$i]) $($after[$i])" } }
$left = @(Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -match '^(Axicultus|Axicultus\.Tests)$' })
if ($left) { "LEFTOVER PROCESSES: " + (($left | ForEach-Object { "$($_.ProcessName)#$($_.Id)" }) -join ', ') }
$pptAfter = @(Get-Process POWERPNT -ErrorAction SilentlyContinue).Count
"POWERPNT before=$pptBefore after=$pptAfter"
$global:LASTEXITCODE = 0
