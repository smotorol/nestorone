param(
    [string]$ProjectPath = 'G:\Programing\Work\nestorone\nestorone\src\OrderInventory.Api\OrderInventory.Api.csproj',
    [string]$OutputDir = 'G:\Programing\Work\nestorone\nestorone\.publish\api'
)

$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_HOME = 'G:\Programing\Work\nestorone\nestorone\.dotnet_home'

Write-Host '==== API publish 시작 ====' -ForegroundColor Cyan

if (Test-Path $OutputDir) {
    Remove-Item -LiteralPath $OutputDir -Recurse -Force
}

dotnet publish $ProjectPath -c Release -o $OutputDir /p:UseAppHost=true

$requiredFiles = @(
    'OrderInventory.Api.dll',
    'OrderInventory.Application.dll',
    'OrderInventory.Domain.dll',
    'OrderInventory.Infrastructure.dll',
    'OrderInventory.Persistence.dll',
    'OrderInventory.Migrations.dll'
)

Write-Host ''
Write-Host '==== publish 결과 확인 ====' -ForegroundColor Cyan
Get-ChildItem $OutputDir | Select-Object Name, Length

foreach ($file in $requiredFiles) {
    $fullPath = Join-Path $OutputDir $file
    if (-not (Test-Path $fullPath)) {
        throw "필수 파일이 없습니다: $file"
    }
}

Write-Host ''
Write-Host '필수 publish 산출물 확인 완료' -ForegroundColor Green
Write-Host "출력 경로: $OutputDir" -ForegroundColor Green
Write-Host ''
Read-Host 'Enter 키를 누르면 종료합니다'
