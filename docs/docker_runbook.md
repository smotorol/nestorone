# Docker 실행 점검표

## 1. 사전 확인

```powershell
docker version
docker info
docker context ls
docker compose -f docker/docker-compose.yml config
```

확인 포인트:

- Docker daemon 이 running 이어야 한다.
- 현재 context 가 Docker Desktop Linux engine 으로 정상 연결되어야 한다.
- `permission denied while trying to connect to the docker API` 가 나오면 Docker Desktop Service 상태를 확인한다.

## 2. 초기화 실행

```powershell
docker compose -f docker/docker-compose.yml down -v
docker compose -f docker/docker-compose.yml up -d --build
docker compose -f docker/docker-compose.yml ps
```

기본 실행 순서:

1. `oracle` 기동
2. `db-migrator` 실행
3. `api` 실행

## 3. 로그 확인

```powershell
docker logs orderinventory-oracle
docker logs orderinventory-db-migrator
docker logs orderinventory-api
```

확인 포인트:

- Oracle 로그에 `SP2-0734` 가 없어야 한다.
- Oracle 로그에 `ORA-06550`, `PLS-00103` 가 없어야 한다.
- `Bootstrap completed successfully.` 는 실제 성공일 때만 보여야 한다.
- DbMigrator 가 SQL script migration을 적용하고 `Exited (0)` 상태로 끝나야 한다.
- API 컨테이너가 `OrderInventory.Migrations` assembly 오류 없이 기동하고 healthcheck 통과 상태여야 한다.

## 4. 실제 확인할 URL

- `http://localhost:8080/health`
- `http://localhost:8080/swagger`

## 5. Oracle 확인 SQL

```sql
SELECT object_name, object_type, status
FROM user_objects
WHERE object_name IN ('PKG_ORDER', 'DB_SCRIPT_MIGRATION_HISTORY')
ORDER BY object_type, object_name;

SELECT name, type, line, position, text
FROM user_errors
WHERE name = 'PKG_ORDER'
ORDER BY sequence;

SELECT script_name, checksum, applied_at, success_yn
FROM db_script_migration_history
ORDER BY script_id;
```

## 6. DbMigrator 재실행 점검

```powershell
docker compose -f docker/docker-compose.yml run --rm db-migrator
```

확인 포인트:

- 이미 성공 적용된 script 는 `SKIP`
- migration history 중복 row 없음
- checksum 불일치 시 명확한 경고 또는 실패

## 7. 장애 대응

### SP2-0734 발생

- SQL 파일에 UTF-8 BOM 이 남아 있는지 확인
- sqlplus 첫 줄이 `WHENEVER` 로 시작하는 파일 인코딩 확인

### PKG_ORDER INVALID

- `user_errors` 조회
- spec/body 순서 확인
- trailing `/` 처리 확인

### API가 Migrations assembly 오류로 종료

- API build output 또는 publish output 에 `OrderInventory.Migrations.dll` 포함 여부 확인
- `Program.cs` 의 `MigrationsAssembly("OrderInventory.Migrations")` 설정 확인

### API healthcheck 실패

- API 이미지에 `curl` 이 포함되어 있는지 확인
- `docker inspect orderinventory-api --format "{{json .State.Health}}"` 로 health 로그 확인
- `/health` 엔드포인트가 200을 반환하는지 확인

### Docker daemon 연결 실패

- Docker Desktop 실행 여부 확인
- `com.docker.service` 상태 확인
- Linux containers 모드 확인
- `docker context ls` 확인

## 8. 현재 문서 기준 주의사항

- Docker 통합 실행은 실제로 검증했다.
- 다만 clean run 반복 시 로컬 Docker Compose 상태가 간헐적으로 불안정했던 적이 있어, 문제 발생 시 `docker compose ps`, `docker ps -a`, `docker logs` 를 함께 확인하는 것이 좋다.

## 9. 권장 절차

1. Docker daemon 확인
2. Oracle baseline init
3. DbMigrator 적용
4. Oracle object validation
5. API health 확인
6. Swagger 확인
7. 상품 조회/주문 생성/주문 취소 테스트
8. DbMigrator 재실행 SKIP 확인
