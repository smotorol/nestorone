param(
    [string]$OracleConnectionString = $(if ($env:ORACLE_CONNECTION_STRING) { $env:ORACLE_CONNECTION_STRING } else { 'User Id=app_user;Password=AppUser1234!;Data Source=localhost:1521/FREEPDB1' }),
    [string]$SqlMigrationsPath = $(if ($env:SQL_MIGRATIONS_PATH) { $env:SQL_MIGRATIONS_PATH } else { (Join-Path (Split-Path -Parent $PSScriptRoot) 'db/migrations_sql') }),
    [string]$ShowSummary = $(if ($env:DBMIGRATOR_SHOW_SUMMARY) { $env:DBMIGRATOR_SHOW_SUMMARY } else { 'true' }),
    [switch]$NoWait
)

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src/OrderInventory.DbMigrator/OrderInventory.DbMigrator.csproj'

if (-not (Test-Path $project)) {
    Write-Error "DbMigrator project not found: $project"
    if (-not $NoWait) {
        Write-Host ''
        Read-Host 'Press Enter to close'
    }
    exit 1
}

$env:ORACLE_CONNECTION_STRING = $OracleConnectionString
$env:SQL_MIGRATIONS_PATH = $SqlMigrationsPath
$env:DBMIGRATOR_SHOW_SUMMARY = $ShowSummary

Write-Host "[INFO] ORACLE_CONNECTION_STRING=$OracleConnectionString"
Write-Host "[INFO] SQL_MIGRATIONS_PATH=$SqlMigrationsPath"
Write-Host "[INFO] DBMIGRATOR_SHOW_SUMMARY=$ShowSummary"
Write-Host "[INFO] Starting DbMigrator project: $project"

dotnet run --project $project
$exitCode = $LASTEXITCODE

if ($exitCode -ne 0) {
    Write-Host "[ERROR] DbMigrator process exited with code $exitCode" -ForegroundColor Red
}
else {
    Write-Host "[INFO] DbMigrator completed successfully." -ForegroundColor Green
}

if (-not $NoWait) {
    Write-Host ''
    Read-Host 'Press Enter to close'
}

exit $exitCode
