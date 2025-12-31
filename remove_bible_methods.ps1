# Remove duplicate Bible methods from gfUtility.cs
$filePath = "Easislides\Global\gfUtility.cs"

Write-Host "Reading $filePath..."
$lines = Get-Content $filePath

Write-Host "Total lines before: $($lines.Count)"

# We need to remove:
# 1. BuildBibleSearchString (first overload) - lines 4144-4147 (4 lines)
# 2. BuildBibleSearchString (second overload) - lines 4149-4237 (89 lines)
# 3. PartialWordSearch - lines 4239-4261 (23 lines)
# 4. SearchBiblePassages - lines 4263-4310 (48 lines)

# Convert to 0-based indexing
$newLines = New-Object System.Collections.ArrayList

# Copy lines before first method (0 to 4143)
for ($i = 0; $i -lt 4143; $i++) {
    [void]$newLines.Add($lines[$i])
}

# Skip lines 4143-4309 (BuildBibleSearchString, PartialWordSearch, SearchBiblePassages)
# Continue from line 4310 (FormInUse method)
for ($i = 4310; $i -lt $lines.Count; $i++) {
    [void]$newLines.Add($lines[$i])
}

Write-Host "Total lines after: $($newLines.Count)"
Write-Host "Lines removed: $($lines.Count - $newLines.Count)"

# Backup original file
Copy-Item $filePath "$filePath.bak3"

# Write new content
$newLines | Set-Content $filePath

Write-Host "Done! Original file backed up to $filePath.bak3"
