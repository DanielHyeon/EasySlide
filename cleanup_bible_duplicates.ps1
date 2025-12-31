# gfUtility.cs에서 중복된 성경 관련 메서드 제거 스크립트

$filePath = "Easislides\Global\gfUtility.cs"

Write-Host "Reading $filePath..."
$content = Get-Content $filePath -Raw

# 제거할 메서드 목록과 대략적인 라인 범위
$methodsToRemove = @(
    @{Name="LoadSelectedBibleVerses"; StartLine=1631; EndApprox=1689},
    @{Name="LoadSelectedBibleVerses_Old"; StartLine=1691; EndApprox=1733},
    @{Name="BuildBibleSearchString"; StartLine=4248; EndApprox=4365},
    @{Name="PartialWordSearch"; StartLine=4343; EndApprox=4365},
    @{Name="SearchBiblePassages"; StartLine=4367; EndApprox=4414}
)

Write-Host "`n제거해야 할 메서드들:"
foreach ($method in $methodsToRemove) {
    Write-Host "  - $($method.Name) (대략 라인 $($method.StartLine)-$($method.EndApprox))"
}

Write-Host "`n수동으로 다음 메서드들을 gfUtility.cs에서 삭제하세요:"
Write-Host "1. LoadSelectedBibleVerses (line ~1631-1689)"
Write-Host "2. LoadSelectedBibleVerses_Old (line ~1691-1733)"
Write-Host "3. BuildBibleSearchString - 2개 오버로드 (line ~4248-4341)"
Write-Host "4. PartialWordSearch (line ~4343-4365)"
Write-Host "5. SearchBiblePassages (line ~4367-4414)"

Write-Host "`n이 메서드들은 이미 gfBible.cs로 이동되었습니다."
Write-Host "`n컴파일 오류가 나는 경우 gfUtility.cs에서 위 메서드 정의를 찾아서 제거하세요."
