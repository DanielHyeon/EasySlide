# Find and run MSBuild
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"

if (Test-Path $vswhere) {
    $msbuildPath = & $vswhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | Select-Object -First 1

    if ($msbuildPath) {
        Write-Host "Found MSBuild at: $msbuildPath"
        Write-Host "Building solution..."
        & $msbuildPath Easislides.sln /p:Configuration=Release /v:minimal /nologo

        if ($LASTEXITCODE -eq 0) {
            Write-Host "`nBuild succeeded!" -ForegroundColor Green
        } else {
            Write-Host "`nBuild failed with exit code $LASTEXITCODE" -ForegroundColor Red
        }

        exit $LASTEXITCODE
    } else {
        Write-Host "Could not find MSBuild" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "vswhere.exe not found. Trying alternative approach..." -ForegroundColor Yellow

    # Try common MSBuild locations
    $possiblePaths = @(
        "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
    )

    foreach ($path in $possiblePaths) {
        if (Test-Path $path) {
            Write-Host "Found MSBuild at: $path"
            Write-Host "Building solution..."
            & $path Easislides.sln /p:Configuration=Release /v:minimal /nologo

            if ($LASTEXITCODE -eq 0) {
                Write-Host "`nBuild succeeded!" -ForegroundColor Green
            } else {
                Write-Host "`nBuild failed with exit code $LASTEXITCODE" -ForegroundColor Red
            }

            exit $LASTEXITCODE
        }
    }

    Write-Host "Could not find MSBuild in common locations" -ForegroundColor Red
    exit 1
}
