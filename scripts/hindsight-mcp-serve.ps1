<#
.SYNOPSIS
  Hindsight 로컬 MCP 서버(:8888) 기동/확인 launcher.

.DESCRIPTION
  SDD 2층 기억의 episodic 계층(Hindsight)을 띄운다. 이미 :8888에서 응답하면
  재기동하지 않는다. retain/recall/reflect 는 이 서버가 떠 있을 때만 동작한다.

  검증된 환경 특이사항(이 PC):
    - PYTHONUTF8=1            : Windows 콘솔 cp949 가 배너 유니코드(U+2584)를 못 찍어
                               죽는 문제 회피(없으면 부팅 시 UnicodeEncodeError).
    - uvx --system-certs     : 사내 프록시 루트 CA 미신뢰(UnknownIssuer) 회피
                               (gbrain 의 NODE_EXTRA_CA_CERTS 와 동일 취지).
    - LLM provider=ollama    : 외부 키 없이 로컬. 단 retain/reflect 는 *채팅* 모델이
                               필요하다(임베딩 nomic-embed-text 만으로는 부족) →
                               `ollama pull llama3.2` 등으로 채팅 모델을 먼저 받아야
                               retain/reflect 가 실제 동작한다.

.PARAMETER Model
  Ollama 채팅 모델 이름(기본 llama3.2).

.PARAMETER Port
  바인드 포트(기본 8888).

.PARAMETER Foreground
  포그라운드로 실행(로그 확인용). 기본은 백그라운드 daemon.
#>
param(
    [string]$Model = "llama3.2",
    [int]$Port = 8888,
    [switch]$Foreground
)

$ErrorActionPreference = "Stop"

# 이미 떠 있으면 그대로 둔다.
try {
    $resp = Invoke-WebRequest -Uri "http://localhost:$Port/mcp/" -Method GET -TimeoutSec 5 -UseBasicParsing
    if ($resp.StatusCode -ge 200 -and $resp.StatusCode -lt 500) {
        Write-Host "Hindsight already serving on :$Port (HTTP $($resp.StatusCode)). Skipping start."
        exit 0
    }
} catch {
    # 연결 실패 = 미기동 → 아래에서 기동
}

$env:PYTHONUTF8 = "1"
$env:PYTHONIOENCODING = "utf-8"
$env:HINDSIGHT_API_LLM_PROVIDER = "ollama"
$env:HINDSIGHT_API_LLM_MODEL = $Model
$env:HINDSIGHT_API_PORT = "$Port"

$uvxArgs = @("--system-certs", "--from", "hindsight-api", "hindsight-local-mcp", "--port", "$Port")

if ($Foreground) {
    Write-Host "Starting Hindsight (foreground, provider=ollama model=$Model port=$Port)..."
    & uvx @uvxArgs
} else {
    # daemon: 백그라운드 기동 후 즉시 반환. 유휴 시 auto-exit(서버 기본 동작).
    $uvxArgs += @("--daemon")
    Write-Host "Starting Hindsight daemon (provider=ollama model=$Model port=$Port)..."
    & uvx @uvxArgs
    Start-Sleep -Seconds 2
    Write-Host "MCP endpoint: http://localhost:$Port/mcp/  (등록: claude mcp add --scope user --transport http hindsight http://localhost:$Port/mcp/)"
}
