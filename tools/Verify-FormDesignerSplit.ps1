param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

$ErrorActionPreference = 'Stop'

$formsDir = Join-Path $ProjectRoot 'Easislides\Easislides'
if (-not (Test-Path $formsDir)) {
    Write-Error "Forms directory not found: $formsDir"
}

$fragmentPattern = '\.(Designer|Events|Fields|Layout|Logic)\.cs$'
$failures = New-Object System.Collections.Generic.List[string]

$formRoots = Get-ChildItem -Path $formsDir -Filter 'Frm*.cs' |
    Where-Object { $_.Name -notmatch $fragmentPattern } |
    Where-Object {
        $text = Get-Content -LiteralPath $_.FullName -Raw
        $text -match 'class\s+(Frm\w+)\s*:\s*Form'
    } |
    Sort-Object Name

foreach ($form in $formRoots) {
    $text = Get-Content -LiteralPath $form.FullName -Raw
    $classMatch = [regex]::Match($text, 'class\s+(Frm\w+)\s*:\s*Form')
    if (-not $classMatch.Success) {
        continue
    }

    $className = $classMatch.Groups[1].Value
    $designerPath = Join-Path $formsDir "$className.Designer.cs"

    if ($text -notmatch '\bpartial\s+class\s+' + [regex]::Escape($className) + '\s*:\s*Form') {
        $failures.Add("$($form.Name): root form class is not partial")
    }

    if ($text -match 'private\s+void\s+InitializeComponent\s*\(') {
        $failures.Add("$($form.Name): InitializeComponent remains in root form file")
    }

    if ($text -match 'protected\s+override\s+void\s+Dispose\s*\(\s*bool\s+disposing\s*\)') {
        $failures.Add("$($form.Name): Dispose remains in root form file")
    }

    if (-not (Test-Path $designerPath)) {
        $failures.Add("$($form.Name): missing $className.Designer.cs")
        continue
    }

    $designerText = Get-Content -LiteralPath $designerPath -Raw
    if ($designerText -notmatch '\bpartial\s+class\s+' + [regex]::Escape($className) + '\b') {
        $failures.Add("$className.Designer.cs: missing matching partial class declaration")
    }

    if ($designerText -notmatch 'private\s+void\s+InitializeComponent\s*\(') {
        $failures.Add("$className.Designer.cs: missing InitializeComponent")
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Form designer split verification failed:" -ForegroundColor Red
    foreach ($failure in $failures) {
        Write-Host " - $failure" -ForegroundColor Red
    }
    exit 1
}

Write-Host "Form designer split verification passed for $($formRoots.Count) form roots." -ForegroundColor Green
