param(
    [string]$EnvironmentName = $(if ($env:ASPNETCORE_ENVIRONMENT) { $env:ASPNETCORE_ENVIRONMENT } else { 'Development' }),
    [string]$Urls = $(if ($env:ASPNETCORE_URLS) { $env:ASPNETCORE_URLS } else { 'http://localhost:5138' }),
    [string]$OracleConnectionString = $(if ($env:ORACLE_CONNECTION_STRING) { $env:ORACLE_CONNECTION_STRING } else { 'User Id=app_user;Password=AppUser1234!;Data Source=localhost:1521/FREEPDB1' })
)

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src/OrderInventory.Api/OrderInventory.Api.csproj'

if (-not (Test-Path $project)) {
    Write-Error "API project not found: $project"
    exit 1
}

$env:ASPNETCORE_ENVIRONMENT = $EnvironmentName
$env:ASPNETCORE_URLS = $Urls
$env:ORACLE_CONNECTION_STRING = $OracleConnectionString

Write-Host "[INFO] ASPNETCORE_ENVIRONMENT=$EnvironmentName"
Write-Host "[INFO] ASPNETCORE_URLS=$Urls"
Write-Host "[INFO] Starting API project: $project"

dotnet run --project $project --no-launch-profile --no-build
exit $LASTEXITCODE
