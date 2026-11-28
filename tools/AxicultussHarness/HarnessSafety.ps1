# Path checks, copies, hashes and path discovery shared by New-IsolatedCopy.ps1 and Run-Harness.ps1. Dot-source only.
# Every write these functions make goes to a path that Assert-HarnessTarget accepted: below the harness root, no reparse
# point anywhere on its path (a junction could lead into a real folder), not inside or around a protected folder. Real
# folders only ever appear as copy SOURCES. No robocopy /MIR or /PURGE and no delete is ever given a real path.

Add-Type -Namespace HarnessNative -Name Kernel32 -MemberDefinition @'
[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
public static extern uint GetLongPathNameW(string shortPath, System.Text.StringBuilder longPath, uint size);
'@

# To the host, not the pipeline: inside a captured call ($root = Get-HarnessRoot) a pipeline message would be swallowed.
function Stop-Guard([int]$Code, [string]$Message) { Write-Host "GUARD: $Message"; exit $Code }

# The app finds its folders with Environment.GetFolderPath, not the environment variables; so do the scripts.
function Get-KnownFolder([string]$Name) {
    $path = [Environment]::GetFolderPath($Name)
    if (-not $path) { Stop-Guard 16 "the system reports no $Name folder" }
    $path
}

function Get-HarnessRoot { Get-NormalPath (Join-Path (Get-KnownFolder 'LocalApplicationData') 'AxicultussHarness') }
function Get-RealProfile { Get-NormalPath (Join-Path (Get-KnownFolder 'ApplicationData') 'Axicultuss') }

# The folders that hold the church's real data or the real apps on this PC. Never a write target.
function Get-ProtectedFolders([string[]]$Extra = @()) {
    $roaming = Get-KnownFolder 'ApplicationData'
    @("$roaming\Axicultuss", "$roaming\Axicultus", 'C:\Axicultuss', 'C:\Axicultus', 'C:\EasiSlides', 'D:\예배자료', 'D:\OneDrive - 산소망교회') + $Extra |
        Where-Object { $_ } | ForEach-Object { Get-NormalPath $_ } | Select-Object -Unique
}

# The data folders the app reads under its root (Gf.LoadSavedData, gfConstants Default*Dir, RestoreBackgroundImages).
# Everything else there (Axicultuss.exe, the dlls, ko, runtimes, win-x86) is the program, not data.
function Get-DataFolderNames { 'Admin', 'Backgrounds', 'Documents', 'HolyBibles', 'Html', 'Images', 'InfoScreens', 'Media', 'Music', 'Powerpoint', 'User Images' }

# Full path with 8.3 components of its existing part in long form, no trailing backslash (except a drive root).
function Get-NormalPath([string]$Path) {
    $full = [IO.Path]::GetFullPath($Path)
    # (no file system call for a network path: an unreachable server would stall the run)
    if ($full.Contains('~') -and -not $full.StartsWith('\\')) {
        # GetLongPathName needs an existing path: expand the existing part, keep the rest as written.
        $existing = $full
        while ($existing -and -not (Test-Path -LiteralPath $existing)) { $existing = Split-Path $existing -Parent }
        $buffer = [Text.StringBuilder]::new(32768)
        if ($existing -and [HarnessNative.Kernel32]::GetLongPathNameW($existing, $buffer, 32768) -gt 0) {
            $full = $buffer.ToString().TrimEnd('\') + '\' + $full.Substring($existing.Length).TrimStart('\')
        }
    }
    $trimmed = $full.TrimEnd('\')
    if ($trimmed -match '^[A-Za-z]:$') { $trimmed + '\' } else { $trimmed }
}

# A path-like text as the app would resolve it (variables expanded, / as \, \\?\ dropped, full, long form); $null when
# it is not a rooted path. A network path (\\server\share\...) is kept as such, with no file system call.
function Resolve-PathLike([string]$Text) {
    if (-not $Text) { return $null }
    $t = [Environment]::ExpandEnvironmentVariables($Text.Trim()).Replace('/', '\')
    if ($t.StartsWith('\\?\UNC\')) { $t = '\\' + $t.Substring(8) }
    elseif ($t.StartsWith('\\?\') -or $t.StartsWith('\??\')) { $t = $t.Substring(4) }
    # (\\.\DISPLAY3, a device name in the settings, is no path)
    if ($t -notmatch '^([A-Za-z]:\\|\\\\(?![.?]\\)[^\\]+\\[^\\]+)') { return $null }
    try { Get-NormalPath $t } catch { $null }
}

function Test-NetworkPath([string]$Path) { $Path.StartsWith('\\') }

# Every place in a text where a path starts, overlapping, each running to the next character that cannot be in a path
# (or ;): a drive letter before a slash (not inside a word, so http:// is no drive; a one-letter item type in front,
# "PD:\...", is allowed at the start or after a non-word character), a %variable% before a slash, or \\server\share. Returns objects with
# Index, Length, Raw and the resolved Path. The driver uses the same pattern (Guards.cs PathText).
$script:PathStartPattern = '(?=((?:(?:(?<![A-Za-z0-9])|(?<=(?:^|[^A-Za-z0-9])[A-Za-z]))[A-Za-z]:[\\/]|%[A-Za-z0-9_()]+%[\\/]|(?<!\\)\\\\[^\\/"<>|\r\n\t*?;:]+\\)[^"<>|\r\n\t*?;]*))'
# A text with a %variable% is also read as the app would expand it: the whole text as one path (Index 0, so a rewrite
# replaces it whole: "%AXI_MEDIA%" naming C:\Axicultuss\Media), and every path in its expanded form (Index -1: found and
# refused, never rewritten in place).
function Find-PathsInText([string]$Text) {
    if (-not $Text) { return }
    foreach ($m in [regex]::Matches($Text, $script:PathStartPattern)) {
        $g = $m.Groups[1]
        $path = Resolve-PathLike $g.Value
        if ($path) { [pscustomobject]@{ Index = $g.Index; Length = $g.Length; Raw = $g.Value; Path = $path } }
    }
    if (-not $Text.Contains('%')) { return }
    $whole = Resolve-PathLike $Text
    if ($whole) { [pscustomobject]@{ Index = 0; Length = $Text.Length; Raw = $Text; Path = $whole } }
    $expanded = [Environment]::ExpandEnvironmentVariables($Text)
    if ($expanded -ne $Text) {
        foreach ($m in [regex]::Matches($expanded, $script:PathStartPattern)) {
            $path = Resolve-PathLike $m.Groups[1].Value
            if ($path) { [pscustomobject]@{ Index = -1; Length = 0; Raw = $m.Groups[1].Value; Path = $path } }
        }
    }
}

# True when Path is Root itself or lies below it (path-boundary aware: C:\Ax is not inside C:\A).
function Test-Within([string]$Path, [string]$Root) {
    $p = (Get-NormalPath $Path).TrimEnd('\'); $r = (Get-NormalPath $Root).TrimEnd('\')
    $p.Equals($r, [StringComparison]::OrdinalIgnoreCase) -or $p.StartsWith($r + '\', [StringComparison]::OrdinalIgnoreCase)
}

function Test-Overlap([string]$A, [string]$B) { (Test-Within $A $B) -or (Test-Within $B $A) }

# A reparse point on Path or on any of its existing parents, or $null.
function Find-ReparseOnPath([string]$Path) {
    for ($p = Get-NormalPath $Path; $p; $p = Split-Path $p -Parent) {
        $item = Get-Item -LiteralPath $p -Force -ErrorAction SilentlyContinue
        if ($item -and ($item.Attributes -band [IO.FileAttributes]::ReparsePoint)) { return $p }
    }
    $null
}

# The harness root itself must be a plain folder, reached through plain folders, outside every protected folder.
function Assert-HarnessRoot([string]$HarnessRoot, [string[]]$Protected) {
    foreach ($real in $Protected) {
        if (Test-Overlap $HarnessRoot $real) { Stop-Guard 20 "harness root $HarnessRoot overlaps protected folder $real" }
    }
    $link = Find-ReparseOnPath $HarnessRoot
    if ($link) { Stop-Guard 20 "harness root $HarnessRoot passes reparse point $link" }
}

# A write target: strictly below the harness root, no reparse point on its whole path, no overlap with a protected folder.
function Assert-HarnessTarget([string]$Target, [string]$HarnessRoot, [string[]]$Protected) {
    $t = Get-NormalPath $Target; $r = Get-NormalPath $HarnessRoot
    if (-not $t.StartsWith($r + '\', [StringComparison]::OrdinalIgnoreCase)) { Stop-Guard 20 "write target $t is not below the harness root $r" }
    foreach ($real in $Protected) {
        if (Test-Overlap $t $real) { Stop-Guard 20 "write target $t overlaps protected folder $real" }
    }
    $link = Find-ReparseOnPath $t
    if ($link) { Stop-Guard 20 "write target $t passes reparse point $link" }
}

# Inside a checked harness tree: refuses any reparse point in it, then clears read-only attributes (a read-only file
# would stop a delete or a mirror halfway).
function Clear-HarnessTree([string]$Target, [string]$What) {
    $all = @(Get-ChildItem -LiteralPath $Target -Recurse -Force -ErrorAction Stop)
    $links = @($all | Where-Object { $_.Attributes -band [IO.FileAttributes]::ReparsePoint })
    if ($links) { Stop-Guard 20 "refusing to $What $Target`: it holds reparse point $($links[0].FullName)" }
    foreach ($item in $all) {
        if ($item.Attributes -band [IO.FileAttributes]::ReadOnly) { $item.Attributes = $item.Attributes -band -bnot [IO.FileAttributes]::ReadOnly }
    }
}

# Removes a harness folder (checked target, no reparse point inside).
function Remove-HarnessTree([string]$Target, [string]$HarnessRoot, [string[]]$Protected) {
    Assert-HarnessTarget $Target $HarnessRoot $Protected
    if (-not (Test-Path -LiteralPath $Target)) { return }
    Clear-HarnessTree $Target 'delete'
    [IO.Directory]::Delete((Get-NormalPath $Target), $true)
}

# Copies a real folder (source, read only) into a new harness folder. The destination must not exist yet, so the copy
# never needs /MIR or /PURGE. /XJ: junctions in the source are skipped, not followed.
function Copy-HarnessTree([string]$Source, [string]$Destination, [string]$HarnessRoot, [string[]]$Protected) {
    if (Test-Overlap $Source $Destination) { Stop-Guard 20 "source $Source and destination $Destination overlap" }
    if (Test-Within $Source $HarnessRoot) { Stop-Guard 20 "source $Source lies inside the harness root" }
    Assert-HarnessTarget $Destination $HarnessRoot $Protected
    if (Test-Path -LiteralPath $Destination) { Stop-Guard 20 "destination $Destination exists already" }
    robocopy $Source $Destination /E /XJ /COPY:DAT /DCOPY:T /R:1 /W:1 /NFL /NDL /NJH /NJS /NP | Out-Null
    if ($LASTEXITCODE -ge 8) { Stop-Guard 23 "copy $Source -> $Destination failed (robocopy $LASTEXITCODE)" }
}

# Writes a new harness file (it must not exist yet).
function Write-HarnessFile([string]$Destination, [string]$Text, [string]$HarnessRoot, [string[]]$Protected) {
    Assert-HarnessTarget $Destination $HarnessRoot $Protected
    $full = Get-NormalPath $Destination
    [void][IO.Directory]::CreateDirectory((Split-Path $full -Parent))
    $stream = [IO.FileStream]::new($full, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write)
    try { $bytes = [Text.UTF8Encoding]::new($false).GetBytes($Text); $stream.Write($bytes, 0, $bytes.Length) } finally { $stream.Dispose() }
}

# Mirrors one harness folder onto another (iso.pristine -> iso). Both ends are harness folders; /XJ keeps robocopy out
# of any junction, and the target tree is checked (no reparse point, read-only cleared) first.
function Sync-HarnessTree([string]$Source, [string]$Destination, [string]$HarnessRoot, [string[]]$Protected) {
    Assert-HarnessTarget $Source $HarnessRoot $Protected
    Assert-HarnessTarget $Destination $HarnessRoot $Protected
    if (Test-Overlap $Source $Destination) { Stop-Guard 20 "source $Source and destination $Destination overlap" }
    if (Test-Path -LiteralPath $Destination) { Clear-HarnessTree $Destination 'mirror onto' }
    robocopy $Source $Destination /MIR /XJ /R:1 /W:1 /NFL /NDL /NJH /NJS /NP | Out-Null
    if ($LASTEXITCODE -ge 8) { Stop-Guard 14 "mirror $Source -> $Destination failed (robocopy $LASTEXITCODE)" }
}

# Rewrites, in one text, every path that resolves under OldRoot (any spelling: / or \, variables, 8.3 names) to the same
# path under NewRoot. Whatever stands before the path (an item type letter, "PC:\...") is kept.
function Convert-PathsInText([string]$Text, [string]$OldRoot, [string]$NewRoot) {
    $old = (Get-NormalPath $OldRoot).TrimEnd('\'); $new = (Get-NormalPath $NewRoot).TrimEnd('\')
    for ($pass = 0; $pass -lt 100; $pass++) {
        $hit = Find-PathsInText $Text | Where-Object { $_.Index -ge 0 -and (Test-Within $_.Path $old) } | Select-Object -First 1
        if (-not $hit) { return $Text }
        $trailing = if ($hit.Raw -match '[\\/]$') { '\' } else { '' }
        $Text = $Text.Substring(0, $hit.Index) + $new + $hit.Path.TrimEnd('\').Substring($old.Length) + $trailing + $Text.Substring($hit.Index + $hit.Length)
    }
    throw "path rewrite did not settle: $Text"
}

# Rewrites every string of the settings document with Convert-PathsInText. Kinds and every other value stay as they are.
function Convert-SettingsRoot([string]$Json, [string]$OldRoot, [string]$NewRoot) {
    $doc = [System.Text.Json.Nodes.JsonNode]::Parse($Json)
    # Returns a new node for a string that changed, $null otherwise (an unchanged node keeps its place and parent).
    $walk = {
        param($node)
        if ($node -is [System.Text.Json.Nodes.JsonObject]) {
            foreach ($name in @($node | ForEach-Object Key)) { $new = & $walk $node[$name]; if ($null -ne $new) { $node[$name] = $new } }
            return $null
        }
        if ($node -is [System.Text.Json.Nodes.JsonArray]) {
            for ($i = 0; $i -lt $node.Count; $i++) { $new = & $walk $node[$i]; if ($null -ne $new) { $node[$i] = $new } }
            return $null
        }
        if (Test-JsonString $node) {
            $text = $node.ToString()
            $changed = Convert-PathsInText $text $OldRoot $NewRoot
            if ($changed -cne $text) { return [System.Text.Json.Nodes.JsonValue]::Create($changed) }
        }
        return $null
    }
    [void](& $walk $doc)
    # Relaxed escaping keeps Korean paths readable; the app parses either form to the same strings.
    $doc.ToJsonString([System.Text.Json.JsonSerializerOptions]@{ WriteIndented = $true; Encoder = [System.Text.Encodings.Web.JavaScriptEncoder]::UnsafeRelaxedJsonEscaping })
}

# The strings that still name a path inside one of the Real folders (any spelling).
function Find-RealPathStrings([string[]]$Strings, [string[]]$Real) {
    foreach ($s in $Strings) {
        foreach ($hit in Find-PathsInText $s) {
            if ($Real | Where-Object { Test-Within $hit.Path $_ }) { $s; break }
        }
    }
}

function ConvertFrom-SettingsJson([string]$Json) {
    # The app reads section and value names case-insensitively (JsonProfileStore.Names); so does this.
    # (the comma: PowerShell would otherwise unroll the object into its name-value pairs)
    , [System.Text.Json.Nodes.JsonNode]::Parse($Json, [System.Text.Json.Nodes.JsonNodeOptions]@{ PropertyNameCaseInsensitive = $true }, [System.Text.Json.JsonDocumentOptions]::new())
}

# config\root_directory as the app reads it (RegUtil.GetRegValue -> JsonProfileStore.ReadValue); $null when missing.
function Get-SettingsRootDirectory([string]$Json) {
    $config = (ConvertFrom-SettingsJson $Json)['config']
    if ($config -isnot [System.Text.Json.Nodes.JsonObject]) { return $null }
    $entry = $config['root_directory']
    if ($entry -isnot [System.Text.Json.Nodes.JsonObject] -or -not (Test-JsonString $entry['value'])) { return $null }
    $text = $entry['value'].ToString()
    $kind = $entry['kind']
    if ((Test-JsonString $kind) -and $kind.ToString() -eq 'ExpandString') { $text = [Environment]::ExpandEnvironmentVariables($text) }
    $text
}

function Test-JsonString($Node) { $Node -is [System.Text.Json.Nodes.JsonValue] -and $Node.GetValueKind() -eq [System.Text.Json.JsonValueKind]::String }

# Every string in a JSON document.
function Get-SettingsStrings([string]$Json) {
    $out = [Collections.Generic.List[string]]::new()
    $walk = {
        param($node)
        if ($node -is [System.Text.Json.Nodes.JsonObject]) { foreach ($pair in $node) { & $walk $pair.Value }; return }
        if ($node -is [System.Text.Json.Nodes.JsonArray]) { foreach ($child in $node) { & $walk $child }; return }
        if (Test-JsonString $node) { $out.Add($node.ToString()) }
    }
    & $walk ([System.Text.Json.Nodes.JsonNode]::Parse($Json))
    $out.ToArray()
}

# The top-level real folder a path lies in: drive + first folder (the worship data folder on D:), or, under the user's profile folders,
# that folder + its first child (C:\Users\me\Documents), so a path in the profile does not mean all of C:\Users.
# $null for a drive root.
function Get-TopFolder([string]$Path) {
    if (Test-NetworkPath $Path) { $n = $Path.TrimStart('\').Split('\'); return '\\' + $n[0] + '\' + $n[1] }
    $userProfile = Get-NormalPath (Get-KnownFolder 'UserProfile')
    $bases = @((Get-KnownFolder 'LocalApplicationData'), (Get-KnownFolder 'ApplicationData'), $userProfile) | ForEach-Object { Get-NormalPath $_ }
    foreach ($base in $bases) {
        if ((Test-Within $Path $base) -and -not (Test-Within $base $Path)) { return Join-Path $base ((Get-NormalPath $Path).Substring($base.Length + 1).Split('\')[0]) }
    }
    # Another profile (C:\Users\Public\...): that profile + its first child, never all of C:\Users.
    $profiles = Split-Path $userProfile -Parent
    if ((Test-Within $Path $profiles) -and -not (Test-Within $profiles $Path)) {
        $rest = (Get-NormalPath $Path).Substring($profiles.Length + 1).Split('\')
        return (Join-Path $profiles ($rest[0..([Math]::Min(1, $rest.Count - 1))] -join '\'))
    }
    $parts = (Get-NormalPath $Path).TrimEnd('\').Split('\')
    if ($parts.Count -lt 2) { return $null }
    $parts[0] + '\' + $parts[1]
}

# Why a folder the copy names is not checked, or $null. System-managed folders (Windows, programs, the temp folders, the
# profile's Microsoft and Packages folders) change by themselves and hold no church data; a network path or drive could
# stall the run on an unreachable server.
function Get-SkipReason([string]$Path) {
    if (Test-NetworkPath $Path) { return 'network path' }
    try { if ([IO.DriveInfo]::new([IO.Path]::GetPathRoot($Path)).DriveType -eq [IO.DriveType]::Network) { return 'network drive' } } catch { }
    $local = Get-KnownFolder 'LocalApplicationData'; $roaming = Get-KnownFolder 'ApplicationData'
    $system = @('Windows', 'ProgramFiles', 'ProgramFilesX86', 'CommonApplicationData' | ForEach-Object { [Environment]::GetFolderPath($_) } | Where-Object { $_ }) +
        @([IO.Path]::GetTempPath(), "$local\Temp", "$local\Microsoft", "$local\Packages", "$roaming\Microsoft")
    if ($system | Where-Object { Test-Overlap $Path $_ }) { return 'system folder' }
    $null
}

# The real paths outside the harness root that the copy names: every path in its settings, in its JSON files (parsed,
# so escapes are read right) and in its other text files (worship lists *.esw, templates *.est, praise books *.esp,
# info screens *.esi, *.xml, *.txt, *.ini).
function Get-NamedRealPaths([string]$CopyRoot, [string]$HarnessRoot) {
    $texts = [Collections.Generic.List[string]]::new()
    foreach ($file in Get-ChildItem -LiteralPath $CopyRoot -Recurse -File -Force -ErrorAction Stop) {
        if ($file.Length -gt 5MB) { continue }
        if ($file.Extension -eq '.json') {
            try { foreach ($s in Get-SettingsStrings ([IO.File]::ReadAllText($file.FullName))) { $texts.Add($s) } }
            catch { $texts.Add([IO.File]::ReadAllText($file.FullName)) }
        }
        elseif ($file.Extension -in '.esw', '.est', '.esp', '.esi', '.xml', '.txt', '.ini') { $texts.Add([IO.File]::ReadAllText($file.FullName)) }
    }
    $texts | ForEach-Object { Find-PathsInText $_ } | Where-Object { -not (Test-Within $_.Path $HarnessRoot) } |
        ForEach-Object Path | Sort-Object -Unique
}

# What the before/after check does with each real path the copy names: its top-level folder (Get-TopFolder), checked in
# metadata mode, or not checked with a reason (Skip). A top folder that holds the harness itself (Exclude: the repo with
# the driver's bin, where the app's Axicultus_Debug.log grows on every run) is not checked whole: the named path's own
# folder is checked instead when that one is clear of the harness, otherwise it is listed as not checked.
function Get-NamedChecks([string]$CopyRoot, [string]$HarnessRoot, [string]$Exclude) {
    $seen = @{}
    foreach ($named in Get-NamedRealPaths $CopyRoot $HarnessRoot) {
        $top = Get-TopFolder $named
        if (-not $top) { continue }
        $entry = $null
        $skip = Get-SkipReason $top
        if ($skip) { $entry = [pscustomobject]@{ Path = $top; Skip = $skip; Why = '' } }
        elseif ($Exclude -and (Test-Overlap $top $Exclude)) {
            $own = if (Test-Path -LiteralPath $named -PathType Container) { Get-NormalPath $named } else { Split-Path (Get-NormalPath $named) -Parent }
            if (Test-Overlap $own $Exclude) { $entry = [pscustomobject]@{ Path = $own; Skip = "it holds the harness's own files ($Exclude)"; Why = '' } }
            else { $entry = [pscustomobject]@{ Path = $own; Skip = $null; Why = "named by the copy (its own folder: the top folder $top holds the harness)" } }
        }
        else { $entry = [pscustomobject]@{ Path = $top; Skip = $null; Why = 'named by the copy' } }
        if (-not $seen.ContainsKey($entry.Path)) { $seen[$entry.Path] = $true; $entry }
    }
}

# The top folders only (for printing in New-IsolatedCopy.ps1).
function Get-NamedRealFolders([string]$CopyRoot, [string]$HarnessRoot) {
    Get-NamedRealPaths $CopyRoot $HarnessRoot | ForEach-Object { Get-TopFolder $_ } | Where-Object { $_ } | Sort-Object -Unique
}
# A hash of a folder (or file) tree: every folder and file with its relative path, size, write time and attributes;
# with -Content also each file's SHA-256. MISSING when it does not exist (never read as an unchanged empty folder);
# UNREADABLE (a failure, never "unchanged") when any part cannot be listed or read.
function Get-TreeHash([string]$Path, [switch]$Content) {
    $mode = if ($Content) { 'content' } else { 'metadata' }
    if (-not (Test-Path -LiteralPath $Path)) { return 'MISSING' }
    $errors = $null
    $top = Get-Item -LiteralPath $Path -Force -ErrorAction Stop
    $items = @($top) + @(if ($top.PSIsContainer) { Get-ChildItem -LiteralPath $Path -Recurse -Force -ErrorAction SilentlyContinue -ErrorVariable errors })
    if ($errors) { return "UNREADABLE: $($errors[0].Exception.Message)" }
    $base = $top.FullName.TrimEnd('\')
    $sb = [Text.StringBuilder]::new()
    $files = 0; $folders = 0
    foreach ($i in $items | Sort-Object FullName) {
        $relative = $i.FullName.Substring([Math]::Min($base.Length, $i.FullName.Length))
        [void]$sb.Append($relative).Append('|').Append([int]$i.Attributes).Append('|').Append($i.LastWriteTimeUtc.Ticks)
        if ($i.PSIsContainer) { $folders++ }
        else {
            $files++
            [void]$sb.Append('|').Append($i.Length)
            if ($Content) {
                try {
                    $stream = [IO.FileStream]::new($i.FullName, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]'ReadWrite, Delete')
                    try { [void]$sb.Append('|').Append([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))) } finally { $stream.Dispose() }
                }
                catch { return "UNREADABLE: $($i.FullName): $($_.Exception.Message)" }
            }
        }
        [void]$sb.Append("`n")
    }
    ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($sb.ToString())))).Substring(0, 16) + " ($files files, $folders folders, $mode)"
}

# Compares before and after hashes. Returns the report lines (one per entry) and a State: 'changed', 'unreadable', 'ok'.
function Compare-Hashes($Checks, [string[]]$Before, [string[]]$After) {
    $state = 'ok'
    $lines = for ($i = 0; $i -lt $Checks.Count; $i++) {
        $name = $Checks[$i].Path
        if ($Before[$i] -like 'UNREADABLE*' -or $After[$i] -like 'UNREADABLE*') { "UNREADABLE (not checked, a failure): $name before=$($Before[$i]) after=$($After[$i])"; if ($state -eq 'ok') { $state = 'unreadable' } }
        elseif ($Before[$i] -ne $After[$i]) { "REAL DATA CHANGED: $name $($Before[$i]) -> $($After[$i])"; $state = 'changed' }
        else { "unchanged: $name $($After[$i])" }
    }
    [pscustomobject]@{ Lines = @($lines); State = $state }
}

# Value hash of a registry key and its subkeys (reg export text). A missing key reads as MISSING.
function Get-RegistryHash([string]$Key, [string]$ScratchFile) {
    reg query $Key 2>$null | Out-Null
    if ($LASTEXITCODE -ne 0) { return 'MISSING' }
    reg export $Key $ScratchFile /y 2>$null | Out-Null
    if ($LASTEXITCODE -ne 0) { return 'UNREADABLE: reg export failed' }
    $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([IO.File]::ReadAllBytes($ScratchFile))).Substring(0, 16)
    [IO.File]::Delete($ScratchFile)
    $hash
}
