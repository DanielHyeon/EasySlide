<#
.SYNOPSIS
  승인된 episode 마크다운을 Hindsight 에 retain 하는 hook 스크립트.

.DESCRIPTION
  SDD 정책: 에이전트는 평소 recall/reflect 만 사용하고, **retain 은 이 hook(또는
  승인된 스크립트)으로만** 한다(`docs/sdd/hindsight-policy.md` allowlist). 작업 종료
  단계(Step 6)에서 `docs/memory/episodes/<date>-<change-id>.md` 를 작성한 뒤
  이 스크립트로 Hindsight episodic bank 에 적재한다.

  retain 대상은 "검증되어 episode 로 정리된" 내용만. 대화 전문/코드 전문/비밀/미검증
  추측은 넣지 않는다(garbage in → garbage memory). 상세 기준은 hindsight-policy.md.

.PARAMETER Path
  적재할 episode 마크다운 파일 경로(필수).

.PARAMETER Bank
  대상 bank(기본 easislides-dev-episodes). SDD 회고는 sdd-methodology-reflections.

.PARAMETER Port
  Hindsight 서버 포트(기본 8888).

.EXAMPLE
  ./scripts/hindsight-retain-episode.ps1 -Path docs/memory/episodes/2026-06-29-a016-settings-clipping.md
#>
param(
    [Parameter(Mandatory = $true)][string]$Path,
    [string]$Bank = "easislides-dev-episodes",
    [int]$Port = 8888
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $Path)) { throw "episode 파일 없음: $Path" }

# 서버 확인
try {
    Invoke-WebRequest -Uri "http://localhost:$Port/mcp/" -Method GET -TimeoutSec 5 -UseBasicParsing | Out-Null
} catch {
    throw "Hindsight 서버(:$Port) 미응답. 먼저 ./scripts/hindsight-mcp-serve.ps1 로 기동하세요."
}

$uri = "http://localhost:$Port/v1/default/banks/$Bank/documents"
Write-Host "retain → bank=$Bank  file=$Path"

# 문서 업로드(멀티파트). 서버 응답을 그대로 출력해 성공/스키마 불일치를 즉시 확인한다.
# (필드명이 다르면 422 응답에 기대 필드가 나오므로 그에 맞춰 조정)
try {
    $resp = Invoke-RestMethod -Uri $uri -Method Post -Form @{ file = Get-Item $Path } -TimeoutSec 120
    $resp | ConvertTo-Json -Depth 6
    Write-Host "retain 요청 완료. (메모리 추출은 서버 비동기 처리 — operations 로 진행 확인)"
} catch {
    Write-Host "retain 실패 또는 스키마 조정 필요:" -ForegroundColor Yellow
    if ($_.ErrorDetails.Message) { Write-Host $_.ErrorDetails.Message }
    else { Write-Host $_.Exception.Message }
    exit 1
}
