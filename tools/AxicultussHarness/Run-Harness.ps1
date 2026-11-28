#Requires -Version 7.4
# Runs the Axicultuss driver against a fresh mirror of the isolated copy. The guards follow the test-safety rules for
# the machine that holds the church's real data; never relax or bypass them. Each refuses with its own exit code and
# launches nothing. From the launch on, the after phase (comparison, leftover processes) always runs.
#
# Exit codes: the driver's own (0 pass, 1 unhandled exceptions or harness errors logged, 2 driver isolation check refused, 3 hang,
# 4 main form did not start, 5 unknown phase, 6 a modal dialog appeared), or: 10 fewer than two screens, 11 an app is
# running, 15 PowerPoint is running, 12 no isolated copy, 13 driver build failed or the app's own host is in the
# driver's folder, 14 mirror failed, 16 a real folder or system folder that must exist is missing, 20 a path check
# refused, 30 REAL DATA CHANGED, 31 a real folder could not be read, 32 NOT VERIFIED (the comparison itself failed),
# 40 the driver timed out and was killed, 41 script error.
param(
    [string]$Phases = 'smoke',
    [string]$Name = 'smoke',
    [int]$TimeoutSeconds = 300)
trap { "SCRIPT ERROR: $_"; exit 41 }
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\HarnessSafety.ps1"
$root = Get-HarnessRoot
$appProcesses = '^(Axicultuss|Axicultus|Easislides|Axicultuss\.Tests|Axicultus\.Tests)$'

# 1. Two screens: the app places its output on the second one.
Add-Type -AssemblyName System.Windows.Forms
$screens = [System.Windows.Forms.Screen]::AllScreens
if ($screens.Count -lt 2) { Stop-Guard 10 "only $($screens.Count) screen(s) present; not launching" }
# 2. No app instance: the real apps must not be disturbed, and a driver left from an earlier run would share the copy.
$running = @(Get-Process | Where-Object { $_.ProcessName -match $appProcesses })
if ($running) { Stop-Guard 11 "app instance already running ($(($running | ForEach-Object { "$($_.ProcessName)#$($_.Id)" }) -join ', ')); not launching" }
# 3. No PowerPoint: the app attaches to PowerPoint when it starts, and must never attach to the operator's.
$ppt = @(Get-Process POWERPNT -ErrorAction SilentlyContinue)
if ($ppt) { Stop-Guard 15 "PowerPoint is running ($(($ppt | ForEach-Object { "#$($_.Id)" }) -join ', ')); close it first; not launching" }
# 4. The isolated copy, made by New-IsolatedCopy.ps1 (it writes the marker only after its own check passed).
$markerPath = "$root\iso.pristine\profile\.axicultuss-isolated"
if (-not (Test-Path -LiteralPath $markerPath -PathType Leaf)) { Stop-Guard 12 "no isolated copy at $root; run New-IsolatedCopy.ps1 first" }
# 5. The driver builds (its build also removes the app's own host from the driver's folder: Harness.csproj). The
#    machine and user Path are set for the build only; the caller's session keeps its own.
$callerPath = $env:Path
try {
    $env:Path = [Environment]::GetEnvironmentVariable('Path', 'Machine') + ';' + [Environment]::GetEnvironmentVariable('Path', 'User')
    dotnet build "$PSScriptRoot\Harness.csproj" -c Debug -nologo -v quiet | Select-String -Pattern ' error ' | Select-Object -Unique -First 10
    $built = $LASTEXITCODE -eq 0
}
finally { $env:Path = $callerPath }
if (-not $built) { Stop-Guard 13 'driver build failed; not launching' }
$outDir = "$PSScriptRoot\bin\Debug\net11.0-windows7.0"
$exe = "$outDir\Axicultuss.Tests.exe"
$host_ = @(Get-ChildItem -LiteralPath $outDir -Force | Where-Object { $_.Name -eq 'Axicultuss.exe' -or $_.Name -like 'Axicultuss.runtimeconfig*.json' })
if ($host_) { Stop-Guard 13 "the app's own host ($($host_.Name -join ', ')) is in $outDir; not launching" }

$marker = Get-Content -LiteralPath $markerPath -Raw | ConvertFrom-Json
$realProfile = Get-RealProfile
$realRoot = Get-NormalPath $marker.realRoot
foreach ($must in $realProfile, $realRoot) { if (-not (Test-Path -LiteralPath $must -PathType Container)) { Stop-Guard 16 "real folder $must is missing; not launching" } }
$protected = Get-ProtectedFolders @($marker.realProfile, $realRoot)
Assert-HarnessRoot $root $protected

# What is checked before and after: content hashes for the small critical folders, name/size/time/attributes for the
# rest. Every protected folder that exists, every data folder of the real root and the whole real root (the program
# files beside the data too: the app's Axicultus_Debug.log must not appear there), and every real folder outside the
# copy that the copied settings or worship lists name (decks are opened in place, read-only).
$wpfProfile = Join-Path (Get-KnownFolder 'ApplicationData') 'Axicultus'
$checks = [Collections.Generic.List[object]]::new()
function Add-Check([string]$Path, [bool]$Content, [string]$Why) {
    $p = Get-NormalPath $Path
    if ($checks | Where-Object { $_.Path -eq $p }) { return }
    $checks.Add([pscustomobject]@{ Path = $p; Content = $Content; Why = $Why })
}
Add-Check $realProfile $true 'real profile'
Add-Check $wpfProfile $true 'WPF app profile'
Add-Check (Join-Path $realRoot 'Admin') $true 'real data: settings, database, worship lists'
foreach ($n in Get-DataFolderNames) { Add-Check (Join-Path $realRoot $n) $false 'real data folder' }
Add-Check $realRoot $false 'real root (program and data)'
foreach ($p in $protected) { if (Test-Path -LiteralPath $p) { Add-Check $p $false 'protected folder' } }
# (the repo holds the driver's bin, where the app's Axicultus_Debug.log grows on every run: a named folder around it is
# narrowed to the named path's own folder, Get-NamedChecks)
$repo = Get-NormalPath "$PSScriptRoot\..\.."
foreach ($n in Get-NamedChecks "$root\iso.pristine" $root $repo) {
    if ($n.Skip) { "not checked ($($n.Skip), named by the copy): $($n.Path)" } else { Add-Check $n.Path $false $n.Why }
}
foreach ($c in $checks) { "check: $($c.Path) ($(if ($c.Content) { 'content' } else { 'metadata' }); $($c.Why))" }
# The app writes no registry key; it reads these only to import settings when the settings file is missing.
$regKeys = 'HKCU\Software\Axicultuss', 'HKCU\Software\Axicultus', 'HKCU\Software\EasiSlides'
$all = @($checks) + @($regKeys | ForEach-Object { [pscustomobject]@{ Path = $_ } })
New-Item -ItemType Directory -Force "$root\logs" | Out-Null
$regScratch = "$root\logs\$Name.reg.tmp"
Assert-HarnessTarget $regScratch $root $protected
function Get-AllHashes { @($checks | ForEach-Object { Get-TreeHash $_.Path -Content:$_.Content }) + @($regKeys | ForEach-Object { Get-RegistryHash $_ $regScratch }) }
$before = Get-AllHashes
# A run that could not be compared afterwards is not started.
$unreadable = @(for ($i = 0; $i -lt $all.Count; $i++) { if ($before[$i] -like 'UNREADABLE*') { "$($all[$i].Path): $($before[$i])" } })
if ($unreadable) { $unreadable | ForEach-Object { "UNREADABLE before the run: $_" }; Stop-Guard 31 'a real folder cannot be read; not launching' }

# A fresh copy for this run, and the driver's own temp folder (the app's session folder, preview cache and deck copies
# go there instead of the user's %TEMP%, which the real app shares).
Sync-HarnessTree "$root\iso.pristine" "$root\iso" $root $protected
$settings = "$root\iso\profile\settings.json"
if (-not (Test-Path -LiteralPath "$root\iso\profile\.axicultuss-isolated" -PathType Leaf)) { Stop-Guard 12 'the mirrored copy has no marker' }
$temp = "$root\temp"
Remove-HarnessTree $temp $root $protected
[void][IO.Directory]::CreateDirectory($temp)
$log = "$root\logs\$Name.log"
Assert-HarnessTarget $log $root $protected

$start = [Diagnostics.ProcessStartInfo]::new($exe)
$start.UseShellExecute = $false
foreach ($argument in $settings, $log, $Phases) { $start.ArgumentList.Add($argument) }
# For the driver process only (this session keeps its own values).
$start.Environment['AXICULTUSS_SETTINGS_FILE'] = $settings
$start.Environment['TEMP'] = $temp
$start.Environment['TMP'] = $temp

# The guards of steps 2 and 3 again, right before the launch: the build, the hashes and the mirror took time.
$running = @(Get-Process | Where-Object { $_.ProcessName -match $appProcesses })
if ($running) { Stop-Guard 11 "app instance started meanwhile ($(($running | ForEach-Object { "$($_.ProcessName)#$($_.Id)" }) -join ', ')); not launching" }
$pptAtLaunch = @(Get-Process POWERPNT -ErrorAction SilentlyContinue | ForEach-Object Id)
if ($pptAtLaunch) { Stop-Guard 15 "PowerPoint started meanwhile (#$($pptAtLaunch -join ', #')); close it first; not launching" }

# From here on the after phase always runs (an error or Ctrl+C included). Its lines go to the host: output to the
# pipeline is refused while a stopped pipeline runs its finally block.
$p = $null
$code = 41
$scriptError = $null
try {
    Write-Host "launching the driver: phases=$Phases log=$log (hands off keyboard and mouse until it ends)"
    $p = [Diagnostics.Process]::Start($start)
    if ($p.WaitForExit($TimeoutSeconds * 1000)) { $code = $p.ExitCode }
    else { Write-Host "TIMEOUT after $TimeoutSeconds s: killing the driver"; $p.Kill($true); [void]$p.WaitForExit(10000); $code = 40 }
    $meaning = @{ 0 = 'pass'; 1 = 'unhandled exceptions or harness errors logged'; 2 = 'driver isolation check refused'; 3 = 'hang'; 4 = 'main form did not start';
        5 = 'unknown phase'; 6 = 'a modal dialog appeared (not answered)'; 40 = 'timed out, killed' }[$code]
    Write-Host "driver exit=$code ($(if ($meaning) { $meaning } else { 'unexpected code' })) log=$log"
}
catch {
    $scriptError = $_
    Write-Host "SCRIPT ERROR during the run: $_"
}
finally {
    if ($p -and -not $p.HasExited) { Write-Host "DRIVER STILL RUNNING #$($p.Id) (the script was interrupted; it was not killed)" }
    $state = 'not verified'
    try {
        $compared = Compare-Hashes $all $before (Get-AllHashes)
        $compared.Lines | ForEach-Object { Write-Host $_ }
        $state = $compared.State
    }
    catch { Write-Host "NOT VERIFIED: the comparison failed: $_" }
    $left = @(Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -match $appProcesses })
    if ($left) { Write-Host ('LEFTOVER PROCESSES: ' + (($left | ForEach-Object { "$($_.ProcessName)#$($_.Id)" }) -join ', ')) }
    # No PowerPoint ran at the launch (checked right before it): any PowerPoint now is new since then. Reported, not killed.
    foreach ($pp in @(Get-Process POWERPNT -ErrorAction SilentlyContinue | Where-Object { $_.Id -notin $pptAtLaunch })) {
        $started = try { $pp.StartTime.ToString('HH:mm:ss') } catch { 'unknown' }
        Write-Host "LEFTOVER POWERPNT #$($pp.Id) started $started"
    }
    if (Test-Path -LiteralPath $log) {
        foreach ($kind in 'ABORT', 'MODAL', 'HARNESS-ERROR', 'UNHANDLED-UI', 'UNHANDLED-DOMAIN', 'UNOBSERVED-TASK', 'HANG', 'INPUT-DROPPED', 'FIRSTCHANCE') {
            $n = @(Select-String -LiteralPath $log -Pattern "\[$kind\]").Count
            if ($n) { Write-Host "log [$kind]: $n line(s)" }
        }
    }
    if ($state -eq 'changed') { Write-Host 'REAL DATA CHANGED: do not run again before the cause is found'; exit 30 }
    if ($state -eq 'unreadable') { Write-Host 'a real folder could not be read: this run is not checked'; exit 31 }
    if ($state -ne 'ok') { Write-Host 'NOT VERIFIED: this run is not checked'; exit 32 }
    if ($scriptError) { exit 41 }
    exit $code
}
