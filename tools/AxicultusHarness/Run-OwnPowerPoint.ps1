# Runs the "pptown" phase with a PowerPoint that was already open before the app started (as an operator's own
# PowerPoint would be), then checks that it survived. All guards of Run-Harness.ps1 still apply; this adds one more.
param([string]$Name = 'pptown', [string]$Root = "$env:LOCALAPPDATA\AxicultusHarness")
if (Get-Process POWERPNT -ErrorAction SilentlyContinue) { 'GUARD: a PowerPoint is already running (may be the user''s); not launching'; exit 12 }
$exe = (Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\powerpnt.exe').'(default)'
New-Item -ItemType Directory -Force "$Root\own" | Out-Null
$deck = Get-ChildItem "$Root\iso.pristine\work" -Recurse -Filter *.pptx -File | Sort-Object Length | Select-Object -First 1
Copy-Item $deck.FullName "$Root\own\operator.pptx" -Force
$pp = Start-Process -FilePath $exe -ArgumentList "`"$Root\own\operator.pptx`"" -PassThru
$deadline = (Get-Date).AddSeconds(40)
do { Start-Sleep -Milliseconds 500; $pp.Refresh() } while (-not $pp.HasExited -and $pp.MainWindowTitle -notmatch 'operator' -and (Get-Date) -lt $deadline)
"operator PowerPoint #$($pp.Id) title=`"$($pp.MainWindowTitle)`""
Start-Sleep -Seconds 3
try { & "$PSScriptRoot\Run-Harness.ps1" -Phases 'pptown' -Name $Name -Root $Root }
finally {
    $pp.Refresh()
    if ($pp.HasExited) { "OPERATOR POWERPOINT WAS CLOSED BY THE APP (exit $($pp.ExitCode))" }
    else {
        "operator PowerPoint still running after the app closed: #$($pp.Id) title=`"$($pp.MainWindowTitle)`""
        [void]$pp.CloseMainWindow()
        if (-not $pp.WaitForExit(6000)) { $pp.Kill() }
    }
    'POWERPNT left: ' + @(Get-Process POWERPNT -ErrorAction SilentlyContinue).Count
}
