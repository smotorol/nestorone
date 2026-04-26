# 로컬 실행 가이드

## 1. 지원 실행 방식

이 프로젝트는 아래 실행 방식을 지원한다.

1. Docker Compose 통합 실행
2. 로컬 `dotnet run` 실행
3. publish 후 exe 실행

현재 기준 기본 시연 방식은 `Docker Compose로 oracle + db-migrator + api` 를 함께 올리는 흐름이다. 로컬 디버깅이 필요할 때만 Oracle은 Docker, API/DbMigrator/WinForms는 로컬 실행을 사용한다.

## 2. Docker Compose 실행

```powershell
docker version
cd docker
docker compose down -v
docker compose up -d --build
docker compose ps
docker logs orderinventory-oracle
docker logs orderinventory-db-migrator
docker logs orderinventory-api
```

확인:

- Oracle Listener: `localhost:1521`
- Oracle PDB: `FREEPDB1`
- API: `http://localhost:8080/swagger`
- Health: `http://localhost:8080/health`

실행 순서:

1. `oracle`
2. `db-migrator`
3. `api`

## 3. DbMigrator 로컬 실행

```powershell
$env:ORACLE_CONNECTION_STRING='User Id=app_user;Password=AppUser1234!;Data Source=localhost:1521/FREEPDB1'
dotnet run --project .\src\OrderInventory.DbMigrator\OrderInventory.DbMigrator.csproj
```

간단 실행 스크립트:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\start-db-migrator.ps1
```

또는

```bat
scripts\start-db-migrator.bat
```

재실행 시 이미 성공 적용된 SQL script 는 `SKIP` 되어야 한다.

## 4. 로컬 dotnet run 실행

사전 빌드:

```powershell
dotnet restore OrderInventory.sln
dotnet build OrderInventory.sln
```

실행:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
$env:ASPNETCORE_URLS='http://localhost:5138'
$env:ORACLE_CONNECTION_STRING='User Id=app_user;Password=AppUser1234!;Data Source=localhost:1521/FREEPDB1'
dotnet run --project .\src\OrderInventory.Api\OrderInventory.Api.csproj --no-launch-profile --no-build
```

간단 실행 스크립트:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\start-api.ps1
```

또는

```bat
scripts\start-api.bat
```

## 5. publish exe 실행

```powershell
dotnet publish .\src\OrderInventory.Api\OrderInventory.Api.csproj -c Release -o .\.publish\api
.\.publish\api\OrderInventory.Api.exe
```

주의:

- 현재 로컬 publish 는 사용자 환경의 NuGet/권한 상태에 영향을 받을 수 있다.
- GitHub Actions CI 에서는 publish 검증 단계를 별도로 둔다.

## 6. 검증

권장 순서:

1. Oracle 기동 확인
2. DbMigrator 적용 확인
3. `/swagger`
4. `/health`
5. `GET /api/products`
6. `POST /api/orders`
7. `POST /api/orders/{id}/cancel`
8. 재고 부족 실패 시나리오 확인

자동 테스트 스크립트:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-api-flow.ps1
```

## 7. WinForms 실행

```powershell
$env:ORDERINVENTORY_API_BASE_URL='http://localhost:5138'
dotnet run --project .\src\OrderInventory.Client.WinForms\OrderInventory.Client.WinForms.csproj
```

## 8. publish 문제 해결 메모

- `C:\Users\<user>\AppData\Roaming\NuGet\NuGet.Config` 접근 오류가 나면 `APPDATA` 문제를 먼저 확인한다.
- 네트워크 제한 환경에서는 `NU1301` 로 nuget.org 접근이 실패할 수 있다.

## 9. 현재 검증 상태

실제로 확인한 항목:

- 로컬 `dotnet run` 기준 API 실행
- `/health` 와 `/swagger` 확인
- 상품 조회 / 주문 생성 / 주문 취소 / 재고 부족 실패 시나리오 확인

아직 별도 확인이 필요한 항목:

- 로컬 publish exe 반복 검증
- GitHub Actions 원격 publish 결과 확인
