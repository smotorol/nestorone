param(
    [string]$ComposeDir = 'G:\Programing\Work\nestorone\nestorone\docker'
)

$ErrorActionPreference = 'Stop'

Write-Host '==== Docker Compose 로컬 배포 시작 ====' -ForegroundColor Cyan
Push-Location $ComposeDir

try {
    docker compose down -v
    docker compose up -d --build
    docker compose ps

    Write-Host ''
    Write-Host '배포 후 확인 URL' -ForegroundColor Green
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
