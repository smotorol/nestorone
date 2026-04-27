# 로컬 배포 가이드

이 문서는 포트폴리오/학습용 실무형 샘플 프로젝트를 로컬 PC에서 배포 형태로 실행하는 절차를 정리한 문서다.

## 1. 지원하는 로컬 배포 방식

1. Docker Compose 전체 실행
2. Oracle + DbMigrator 는 Docker, API 는 publish exe 로 실행
3. GHCR 이미지 pull 후 Docker Compose 실행

## 2. 사전 준비

- Docker Desktop 실행
- Docker Desktop 이 Linux container 모드인지 확인
- .NET 8 SDK 설치
- PowerShell 실행 가능 환경 준비

## 3. 로컬 Docker 배포 절차

### 권장 명령

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\deploy-local-docker.ps1
```

### 수동 명령

```powershell
cd docker
docker compose down -v
docker compose up -d --build
docker compose ps
docker logs orderinventory-oracle
docker logs orderinventory-db-migrator
docker logs orderinventory-api
```

### 확인 URL

- `http://localhost:8080/health`
- `http://localhost:8080/swagger`

### 종료 / 초기화

```powershell
cd docker
docker compose down
docker compose down -v
```

## 4. publish exe 배포 절차

이 방식은 Oracle + DbMigrator 는 Docker 로 실행하고, API 는 로컬 publish 산출물을 실행하는 방식이다.

### publish

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\publish-api-local.ps1
```

실행 시 확인하는 항목:

- `.publish/api` 경로 생성
- `OrderInventory.Api.exe` 생성
- publish 핵심 DLL 포함 여부 확인

### 실행

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\run-api-published.ps1
```

기본 실행 주소:

- `http://localhost:5138/health`
- `http://localhost:5138/swagger`

필요 시 환경변수 override 예시:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\run-api-published.ps1 `
  -Urls 'http://localhost:5139' `
  -EnvironmentName 'Development' `
  -OracleConnectionString 'User Id=app_user;Password=AppUser1234!;Data Source=localhost:1521/FREEPDB1'
```

## 5. GHCR 이미지 로컬 실행 절차

GHCR 발행 후에는 로컬 build 없이 이미지를 pull 해서 실행할 수 있다.

### 로그인

```powershell
docker login ghcr.io
```

### 환경변수 예시

```powershell
$env:GHCR_OWNER = 'your-github-id'
$env:GHCR_REPO = 'your-repo-name'
$env:GHCR_TAG = 'v0.1.0'
```

### 실행

```powershell
cd docker
docker compose -f docker-compose.yml -f docker-compose.ghcr.yml up -d
```

설명:

- 기본 `docker-compose.yml` 은 Oracle 설정을 유지한다.
- `docker-compose.ghcr.yml` 은 `api`, `db-migrator` 이미지를 GHCR 기준으로 덮어쓴다.

## 6. API 기능 확인

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-api-flow.ps1 -BaseUrl 'http://localhost:8080'
```

이 스크립트는 아래를 확인한다.

- `/health`
- `/swagger`
- 상품 조회
- 주문 생성
- 주문 취소
- 재고 부족 실패 응답 code/message

## 7. 현재 검증 상태

실제로 검증한 항목:

- Docker Compose 전체 실행
- Oracle healthy
- DbMigrator 실행 및 migration 적용
- API healthy
- `/health`, `/swagger`
- 상품 조회 / 주문 생성 / 주문 취소 / 재고 부족 실패
- publish 산출물 생성


아직 검증하지 않은 항목:

- publish exe 실행 후 `/health` 직접 확인
- GHCR 실제 이미지 pull 실행

## 8. 트러블슈팅

### Docker daemon 연결 실패

- Docker Desktop 실행 여부 확인
- Linux container 모드 확인
- `docker version`, `docker info`, `docker context ls` 확인

### publish 결과에 필수 애플리케이션 DLL 누락

- `scripts/publish-api-local.ps1` 로 publish 수행
- `OrderInventory.Api.csproj` publish 결과에 필수 애플리케이션 DLL이 포함되는지 확인

### 재고 부족 응답 메시지가 길게 보일 때

- 현재는 `GlobalExceptionMiddleware` 에서 Oracle business error 를 짧은 사용자 메시지로 변환한다.
- 내부 상세 원문은 서버 로그를 확인한다.



