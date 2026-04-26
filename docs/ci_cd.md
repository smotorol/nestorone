# CI/CD 준비 메모

## 1. 현재 상태

이 저장소는 GitHub Actions 기반 CI/CD 기본 구성이 준비되어 있다.

- CI: `.github/workflows/ci.yml`
- CD: `.github/workflows/cd-docker.yml`
- Docker 대상 이미지
  - API
  - DbMigrator

현재 기준으로 완료된 것은 다음과 같다.

- workflow 파일 정적 검토 완료
- 로컬에서 `docker/docker-compose.yml` 구성 검증 완료
- 로컬에서 API/DbMigrator Dockerfile 빌드 검증 완료

아직 완료되지 않은 것은 다음과 같다.

- GitHub Actions 실제 원격 실행 결과 확인
- GHCR 실제 이미지 발행 확인

## 2. CI 구성

`ci.yml` 은 두 개의 job 으로 구성된다.

### Windows job

- `dotnet restore OrderInventory.sln`
- `dotnet build OrderInventory.sln --configuration Release`
- `dotnet publish src/OrderInventory.Api/OrderInventory.Api.csproj`
- `dotnet publish src/OrderInventory.DbMigrator/OrderInventory.DbMigrator.csproj`
- publish 산출물 업로드

### Ubuntu job

- `src/OrderInventory.Api/Dockerfile` 빌드 검증
- `src/OrderInventory.DbMigrator/Dockerfile` 빌드 검증

의도:

- .NET 빌드 문제와 Docker 이미지 빌드 문제를 분리해서 빠르게 실패 지점을 찾는다.
- secret 없이 실행 가능하도록 구성해 PR 단계에서도 검증 가능하게 둔다.

## 3. CD 구성

`cd-docker.yml` 은 아래 두 경우에 동작하도록 구성했다.

- `workflow_dispatch`
- `v*` 형식 태그 push

권한:

- `contents: read`
- `packages: write`

GHCR 로그인:

- `docker/login-action@v3`
- `GITHUB_TOKEN` 사용

## 4. GHCR 이미지명/태그 전략

현재 권장 이미지명:

- `ghcr.io/<owner>/<repo>-orderinventory-api`
- `ghcr.io/<owner>/<repo>-orderinventory-db-migrator`

현재 태그 전략:

- `latest`
- Git tag (`v1.0.0` 같은 버전 태그)
- commit SHA

예시:

- `ghcr.io/acme/orderinventory-orderinventory-api:latest`
- `ghcr.io/acme/orderinventory-orderinventory-api:v1.0.0`
- `ghcr.io/acme/orderinventory-orderinventory-api:sha-<commit>`
- `ghcr.io/acme/orderinventory-orderinventory-db-migrator:latest`
- `ghcr.io/acme/orderinventory-orderinventory-db-migrator:v1.0.0`
- `ghcr.io/acme/orderinventory-orderinventory-db-migrator:sha-<commit>`

주의:

- 실제 owner/repo 이름에 따라 이미지 경로가 달라진다.
- 현재 workflow 는 `github.repository_owner` 와 `github.event.repository.name` 를 기준으로 태그를 생성한다.

## 5. Docker 실행 흐름

Docker 환경에서는 API startup migration 을 끄고 `db-migrator` 가 DB 변경 책임을 가진다.

실행 순서:

1. `oracle` 기동
2. `db-migrator` 실행
3. `api` 실행

의존 관계:

- `db-migrator` 는 `oracle healthy` 이후 실행
- `api` 는 `db-migrator service_completed_successfully` 이후 실행

## 6. 로컬 검증 명령

### Docker Compose 전체 실행

```powershell
cd docker
docker compose down -v
docker compose up -d --build
docker compose ps
docker logs orderinventory-oracle
docker logs orderinventory-db-migrator
docker logs orderinventory-api
```

### API 확인

```powershell
Invoke-WebRequest http://localhost:8080/health -UseBasicParsing
Invoke-WebRequest http://localhost:8080/swagger/index.html -UseBasicParsing
Invoke-RestMethod -Method Get -Uri "http://localhost:8080/api/products"
```

### DbMigrator 재실행

```powershell
cd docker
docker compose run --rm db-migrator
```

기대 결과:

- 이미 성공 적용된 script 는 `SKIP`
- 중복 row 없음
- checksum 불일치 시 실패 또는 경고

## 7. GitHub Actions 첫 실행 전 체크리스트

- 저장소 루트에 `OrderInventory.sln` 이 존재하는지 확인
- `src/OrderInventory.Api/Dockerfile` 경로가 맞는지 확인
- `src/OrderInventory.DbMigrator/Dockerfile` 경로가 맞는지 확인
- 로컬에서 `dotnet restore OrderInventory.sln` 가 되는지 확인
- 로컬에서 `dotnet build OrderInventory.sln` 가 되는지 확인
- Docker Desktop 또는 Docker Engine 환경에서 Dockerfile build 가 되는지 확인
- GitHub Packages / GHCR 사용 권한이 저장소 또는 조직 정책에서 허용되는지 확인
- package visibility 기본값을 확인하고 필요하면 조직 정책에 맞게 조정
- `v1.0.0` 형식으로 태그를 push 할 계획인지 확인

태그 push 예시:

```powershell
git tag v1.0.0
git push origin v1.0.0
```

## 8. 실패 시 확인 위치

- CI/CD 실행 화면: GitHub repository > Actions
- Docker build 실패: 해당 job 의 build step 로그
- dotnet publish 실패: Windows job 의 publish step 로그
- GHCR push 실패: `docker/login-action`, `docker/build-push-action` 단계 로그

## 9. 현재 판단

- CI workflow 현재 상태: 정적 검토 완료
- CD/GHCR 발행 준비 상태: workflow 와 태그 전략 준비 완료
- 아직 실제 원격 실행은 검증하지 않았으므로, 성공으로 단정하지 않는다.
