@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%..") do set "ROOT_DIR=%%~fI"
set "PROJECT_PATH=%ROOT_DIR%\src\OrderInventory.Api\OrderInventory.Api.csproj"

if not exist "%PROJECT_PATH%" (
  echo [ERROR] API project not found: %PROJECT_PATH%
  exit /b 1
)

if "%ASPNETCORE_ENVIRONMENT%"=="" set "ASPNETCORE_ENVIRONMENT=Development"
if "%ASPNETCORE_URLS%"=="" set "ASPNETCORE_URLS=http://localhost:5138"
if "%ORACLE_CONNECTION_STRING%"=="" set "ORACLE_CONNECTION_STRING=User Id=app_user;Password=AppUser1234!;Data Source=localhost:1521/FREEPDB1"

echo [INFO] ASPNETCORE_ENVIRONMENT=%ASPNETCORE_ENVIRONMENT%
echo [INFO] ASPNETCORE_URLS=%ASPNETCORE_URLS%
echo [INFO] Starting API project: %PROJECT_PATH%

dotnet run --project "%PROJECT_PATH%" --no-launch-profile --no-build
if errorlevel 1 (
  echo [ERROR] API process exited with code %errorlevel%
  exit /b %errorlevel%
)
