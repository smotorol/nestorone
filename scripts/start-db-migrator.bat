@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%..") do set "ROOT_DIR=%%~fI"
set "PROJECT_PATH=%ROOT_DIR%\src\OrderInventory.DbMigrator\OrderInventory.DbMigrator.csproj"

if not exist "%PROJECT_PATH%" (
  echo [ERROR] DbMigrator project not found: %PROJECT_PATH%
  pause
  exit /b 1
)

if "%ORACLE_CONNECTION_STRING%"=="" set "ORACLE_CONNECTION_STRING=User Id=app_user;Password=AppUser1234!;Data Source=localhost:1521/FREEPDB1"
if "%SQL_MIGRATIONS_PATH%"=="" set "SQL_MIGRATIONS_PATH=%ROOT_DIR%\db\migrations_sql"
if "%DBMIGRATOR_SHOW_SUMMARY%"=="" set "DBMIGRATOR_SHOW_SUMMARY=true"

echo [INFO] ORACLE_CONNECTION_STRING=%ORACLE_CONNECTION_STRING%
echo [INFO] SQL_MIGRATIONS_PATH=%SQL_MIGRATIONS_PATH%
echo [INFO] DBMIGRATOR_SHOW_SUMMARY=%DBMIGRATOR_SHOW_SUMMARY%
echo [INFO] Starting DbMigrator project: %PROJECT_PATH%

dotnet run --project "%PROJECT_PATH%"
set "EXIT_CODE=%errorlevel%"
if not "%EXIT_CODE%"=="0" (
  echo [ERROR] DbMigrator process exited with code %EXIT_CODE%
) else (
  echo [INFO] DbMigrator completed successfully.
)

echo.
echo Press any key to close this window...
pause >nul
exit /b %EXIT_CODE%
