# Remove duplicate Bible methods from gfUtility.cs - Preserving encoding
$filePath = "Easislides\Global\gfUtility.cs"

Write-Host "Reading $filePath with proper encoding..."

# Read file with UTF8 encoding
$content = [System.IO.File]::ReadAllText($filePath, [System.Text.Encoding]::UTF8)
$lines = $content -split "`r`n|`n"

Write-Host "Total lines before: $($lines.Count)"

# We need to remove lines 4144-4310 (0-indexed: 4143-4309)
# These contain BuildBibleSearchString, PartialWordSearch, and SearchBiblePassages

$newLines = @()

# Copy lines before first method (0 to 4142, which is 0-indexed 0-4142)
for ($i = 0; $i -le 4142; $i++) {
    $newLines += $lines[$i]
}

# Skip lines 4143-4309 (the methods to remove)
# Continue from line 4310 (0-indexed 4310)
for ($i = 4310; $i -lt $lines.Count; $i++) {
    $newLines += $lines[$i]
}

Write-Host "Total lines after: $($newLines.Count)"
Write-Host "Lines removed: $($lines.Count - $newLines.Count)"

# Backup original file
Copy-Item $filePath "$filePath.bak4"

# Write new content with UTF8 encoding and original line endings
$newContent = $newLines -join "`r`n"
[System.IO.File]::WriteAllText($filePath, $newContent, [System.Text.Encoding]::UTF8)

Write-Host "Done! Original file backed up to $filePath.bak4"
