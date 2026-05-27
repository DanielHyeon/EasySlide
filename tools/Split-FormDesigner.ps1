param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

$ErrorActionPreference = 'Stop'
$Utf8NoBom = New-Object System.Text.UTF8Encoding($false)

function Get-BlockRange {
    param(
        [string]$Text,
        [string]$Pattern
    )

    $match = [regex]::Match($Text, $Pattern)
    if (-not $match.Success) {
        return $null
    }

    $start = $match.Index
    $braceStart = $Text.IndexOf('{', $match.Index)
    if ($braceStart -lt 0) {
        throw "Opening brace not found for pattern: $Pattern"
    }

    $depth = 0
    $inString = $false
    $inChar = $false
    $inLineComment = $false
    $inBlockComment = $false
    $inVerbatimString = $false
    $escape = $false

    for ($i = $braceStart; $i -lt $Text.Length; $i++) {
        $ch = $Text[$i]
        $next = if ($i + 1 -lt $Text.Length) { $Text[$i + 1] } else { [char]0 }

        if ($inLineComment) {
            if ($ch -eq "`n") { $inLineComment = $false }
            continue
        }

        if ($inBlockComment) {
            if ($ch -eq '*' -and $next -eq '/') {
                $inBlockComment = $false
                $i++
            }
            continue
        }

        if ($inString) {
            if ($inVerbatimString) {
                if ($ch -eq '"' -and $next -eq '"') {
                    $i++
                }
                elseif ($ch -eq '"') {
                    $inString = $false
                    $inVerbatimString = $false
                }
            }
            else {
                if ($escape) {
                    $escape = $false
                }
                elseif ($ch -eq '\') {
                    $escape = $true
                }
                elseif ($ch -eq '"') {
                    $inString = $false
                }
            }
            continue
        }

        if ($inChar) {
            if ($escape) {
                $escape = $false
            }
            elseif ($ch -eq '\') {
                $escape = $true
            }
            elseif ($ch -eq "'") {
                $inChar = $false
            }
            continue
        }

        if ($ch -eq '/' -and $next -eq '/') {
            $inLineComment = $true
            $i++
            continue
        }

        if ($ch -eq '/' -and $next -eq '*') {
            $inBlockComment = $true
            $i++
            continue
        }

        if ($ch -eq '@' -and $next -eq '"') {
            $inString = $true
            $inVerbatimString = $true
            $i++
            continue
        }

        if ($ch -eq '"') {
            $inString = $true
            continue
        }

        if ($ch -eq "'") {
            $inChar = $true
            continue
        }

        if ($ch -eq '{') {
            $depth++
        }
        elseif ($ch -eq '}') {
            $depth--
            if ($depth -eq 0) {
                $end = $i + 1
                while ($end -lt $Text.Length -and ($Text[$end] -eq "`r" -or $Text[$end] -eq "`n")) {
                    $end++
                }
                return [pscustomobject]@{
                    Start = $start
                    End = $end
                    Text = $Text.Substring($start, $end - $start)
                }
            }
        }
    }

    throw "Closing brace not found for pattern: $Pattern"
}

function Remove-RangeDescending {
    param(
        [string]$Text,
        [object[]]$Ranges
    )

    $result = $Text
    foreach ($range in ($Ranges | Sort-Object Start -Descending)) {
        if ($null -ne $range) {
            $result = $result.Remove($range.Start, $range.End - $range.Start)
        }
    }
    return $result
}

function Get-AssignedDesignerNames {
    param([string]$InitializeText)

    $names = New-Object System.Collections.Generic.HashSet[string]
    [void]$names.Add('components')

    foreach ($match in [regex]::Matches($InitializeText, '(?m)^\s*(?:this\.)?(?<name>[A-Za-z_]\w*)\s*=\s*new\s+')) {
        [void]$names.Add($match.Groups['name'].Value)
    }

    return $names
}

function Split-LinesWithEndings {
    param([string]$Text)
    return [regex]::Matches($Text, '.*(?:\r\n|\n|\r|$)') |
        Where-Object { $_.Value.Length -gt 0 } |
        ForEach-Object { $_.Value }
}

$formsDir = Join-Path $ProjectRoot 'Easislides\Easislides'
$formFiles = Get-ChildItem -Path $formsDir -Filter 'Frm*.cs' |
    Where-Object { $_.Name -notmatch '\.(Designer|Events|Fields|Layout|Logic)\.cs$' } |
    Sort-Object Name

$processed = New-Object System.Collections.Generic.List[string]
$skipped = New-Object System.Collections.Generic.List[string]

foreach ($file in $formFiles) {
    $text = [System.IO.File]::ReadAllText($file.FullName)
    $classMatch = [regex]::Match($text, 'public\s+(?:partial\s+)?class\s+(?<name>Frm\w+)\s*:\s*Form')
    if (-not $classMatch.Success) {
        $skipped.Add("$($file.Name): form class not found")
        continue
    }

    $className = $classMatch.Groups['name'].Value
    $initializeRange = Get-BlockRange -Text $text -Pattern '(?m)^[ \t]*private\s+void\s+InitializeComponent\s*\('
    if ($null -eq $initializeRange) {
        $skipped.Add("$($file.Name): InitializeComponent not in root")
        continue
    }

    $designerPath = Join-Path $formsDir "$className.Designer.cs"
    if (Test-Path $designerPath) {
        throw "Refusing to overwrite existing designer file: $designerPath"
    }

    $disposeRange = Get-BlockRange -Text $text -Pattern '(?m)^[ \t]*protected\s+override\s+void\s+Dispose\s*\(\s*bool\s+disposing\s*\)'
    $assignedNames = Get-AssignedDesignerNames -InitializeText $initializeRange.Text

    $withoutMethods = Remove-RangeDescending -Text $text -Ranges @($initializeRange, $disposeRange)
    $movedFields = New-Object System.Collections.Generic.List[string]
    $remainingLines = New-Object System.Collections.Generic.List[string]

    foreach ($line in (Split-LinesWithEndings -Text $withoutMethods)) {
        $fieldMatch = [regex]::Match($line, '^\s*(?:private|protected|internal|public)\s+(?!const\b)(?:readonly\s+)?[\w\.\<\>\[\],\?\s]+\s+(?<name>[A-Za-z_]\w*)\s*(?:=\s*[^;]+)?;\s*(?://.*)?(?:\r\n|\n|\r)?$')
        if ($fieldMatch.Success -and $assignedNames.Contains($fieldMatch.Groups['name'].Value)) {
            $movedFields.Add($line.TrimEnd("`r", "`n"))
        }
        else {
            $remainingLines.Add($line)
        }
    }

    if ($movedFields.Count -eq 0) {
        throw "No designer fields found to move for $($file.Name)"
    }

    $rootText = [string]::Concat($remainingLines)
    $rootText = [regex]::Replace($rootText, 'public\s+class\s+' + [regex]::Escape($className) + '\s*:\s*Form', "public partial class $className : Form", 1)
    $rootText = [regex]::Replace($rootText, '(\r?\n){3,}', "`r`n`r`n")
    $rootText = [regex]::Replace($rootText, '[ \t]+(?=\r?\n)', '')
    $rootText = $rootText.TrimEnd() + "`r`n"

    $namespacePrefix = $text.Substring(0, $classMatch.Index)
    $namespacePrefix = [regex]::Replace($namespacePrefix, '\s*namespace\s+Easislides\s*\{\s*$', '')
    $namespacePrefix = $namespacePrefix.TrimEnd()

    $fieldText = ($movedFields | Sort-Object -Unique) -join "`r`n"
    $disposeText = if ($null -ne $disposeRange) { $disposeRange.Text.TrimEnd() } else { '' }
    $initializeText = $initializeRange.Text.TrimEnd()

    $designerParts = New-Object System.Collections.Generic.List[string]
    if ($namespacePrefix.Length -gt 0) {
        $designerParts.Add($namespacePrefix)
        $designerParts.Add('')
    }
    $designerParts.Add('namespace Easislides')
    $designerParts.Add('{')
    $designerParts.Add("    partial class $className")
    $designerParts.Add('    {')
    foreach ($fieldLine in ($fieldText -split "`r`n")) {
        $designerParts.Add($fieldLine)
    }
    if ($disposeText.Length -gt 0) {
        $designerParts.Add('')
        foreach ($disposeLine in ($disposeText -split "`r?`n")) {
            $designerParts.Add($disposeLine)
        }
    }
    $designerParts.Add('')
    $designerParts.Add('        #region Windows Form Designer generated code')
    $designerParts.Add('')
    foreach ($initLine in ($initializeText -split "`r?`n")) {
        $designerParts.Add($initLine)
    }
    $designerParts.Add('')
    $designerParts.Add('        #endregion')
    $designerParts.Add('    }')
    $designerParts.Add('}')

    $designerText = ($designerParts -join "`r`n") + "`r`n"
    $designerText = [regex]::Replace($designerText, '[ \t]+(?=\r?\n)', '')

    [System.IO.File]::WriteAllText($file.FullName, $rootText, $Utf8NoBom)
    [System.IO.File]::WriteAllText($designerPath, $designerText, $Utf8NoBom)
    $processed.Add("$($file.Name) -> $className.Designer.cs")
}

Write-Host "Processed $($processed.Count) form files." -ForegroundColor Green
foreach ($entry in $processed) {
    Write-Host " - $entry"
}

if ($skipped.Count -gt 0) {
    Write-Host "Skipped $($skipped.Count) form files." -ForegroundColor Yellow
    foreach ($entry in $skipped) {
        Write-Host " - $entry"
    }
}
