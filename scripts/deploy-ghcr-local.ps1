param(
    [string]$GhcrOwner = 'smotorol',
    [string]$GhcrRepo = 'nestorone',
    [string]$GhcrTag = 'v0.1.3',
    [string]$ComposeDir = 'G:\Programing\Work\nestorone\nestorone\docker',
    [switch]$SkipLogin
)

$ErrorActionPreference = 'Stop'

$env:GHCR_OWNER = $GhcrOwner
$env:GHCR_REPO = $GhcrRepo
$env:GHCR_TAG = $GhcrTag

Write-Host '==== GHCR 이미지 기반 로컬 실행 시작 ====' -ForegroundColor Cyan
Write-Host "GHCR_OWNER=$GhcrOwner"
Write-Host "GHCR_REPO=$GhcrRepo"
Write-Host "GHCR_TAG=$GhcrTag"
Write-Host ''

if (-not $SkipLogin) {
    Write-Host 'ghcr.io 로그인 시도' -ForegroundColor Green
    docker login ghcr.io
}

Push-Location $ComposeDir

try {
    docker compose -f docker-compose.yml -f docker-compose.ghcr.yml down -v
    docker compose -f docker-compose.yml -f docker-compose.ghcr.yml up -d
    docker compose -f docker-compose.yml -f docker-compose.ghcr.yml ps

    Write-Host ''
    Write-Host '실행 후 확인 URL' -ForegroundColor Green
    Write-Host ' - Health : http://localhost:8080/health'
    Write-Host ' - Swagger: http://localhost:8080/swagger'
    Write-Host ''
    Write-Host '로그 확인 명령' -ForegroundColor Green
    Write-Host ' - docker logs orderinventory-oracle'
    Write-Host ' - docker logs orderinventory-db-migrator'
    Write-Host ' - docker logs orderinventory-api'
}
finally {
    Pop-Location
}

Write-Host ''
Read-Host 'Enter 키를 누르면 종료합니다'
