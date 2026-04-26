param(
    [string]$PublishDir = 'G:\Programing\Work\nestorone\nestorone\.publish\api',
    [string]$Urls = 'http://localhost:5138',
    [string]$EnvironmentName = 'Development',
    [string]$OracleConnectionString = 'User Id=app_user;Password=AppUser1234!;Data Source=localhost:1521/FREEPDB1'
)

$ErrorActionPreference = 'Stop'

$exePath = Join-Path $PublishDir 'OrderInventory.Api.exe'
$dllPath = Join-Path $PublishDir 'OrderInventory.Api.dll'

$env:ASPNETCORE_ENVIRONMENT = $EnvironmentName
$env:ASPNETCORE_URLS = $Urls
$env:ORACLE_CONNECTION_STRING = $OracleConnectionString

Write-Host '==== publish API 실행 ====' -ForegroundColor Cyan
Write-Host "ASPNETCORE_ENVIRONMENT=$EnvironmentName"
Write-Host "ASPNETCORE_URLS=$Urls"
Write-Host '확인 URL'
Write-Host " - $Urls/health"
Write-Host " - $Urls/swagger"
Write-Host ''

if (Test-Path $exePath) {
    & $exePath
}
elseif (Test-Path $dllPath) {
    dotnet $dllPath
}
else {
    throw "실행 파일을 찾지 못했습니다. exe=$exePath dll=$dllPath"
}
