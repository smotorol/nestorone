@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%..") do set "ROOT_DIR=%%~fI"
set "COMPOSE_DIR=%ROOT_DIR%\docker"

if "%GHCR_OWNER%"=="" set "GHCR_OWNER=smotorol"
if "%GHCR_REPO%"=="" set "GHCR_REPO=nestorone"
if "%GHCR_TAG%"=="" set "GHCR_TAG=v0.1.1"

echo ==== GHCR 이미지 기반 로컬 실행 시작 ====
echo GHCR_OWNER=%GHCR_OWNER%
echo GHCR_REPO=%GHCR_REPO%
echo GHCR_TAG=%GHCR_TAG%
echo.

echo ghcr.io 로그인 시도
docker login ghcr.io
if errorlevel 1 (
  echo [ERROR] docker login ghcr.io failed
  pause
  exit /b 1
)

pushd "%COMPOSE_DIR%"
docker compose -f docker-compose.yml -f docker-compose.ghcr.yml down -v
if errorlevel 1 (
  echo [ERROR] docker compose down failed
  popd
  pause
  exit /b 1
)

docker compose -f docker-compose.yml -f docker-compose.ghcr.yml up -d
if errorlevel 1 (
  echo [ERROR] docker compose up failed
  popd
  pause
  exit /b 1
)

docker compose -f docker-compose.yml -f docker-compose.ghcr.yml ps
popd

echo.
echo 실행 후 확인 URL
echo  - Health : http://localhost:8080/health
echo  - Swagger: http://localhost:8080/swagger
echo.
echo 로그 확인 명령
echo  - docker logs orderinventory-oracle
echo  - docker logs orderinventory-db-migrator
echo  - docker logs orderinventory-api
echo.
pause
