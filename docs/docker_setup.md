# Docker 구성 안내

## `gvenzl/oracle-free` 를 선택한 이유

- Oracle Free 이미지를 손쉽게 로컬에서 실행할 수 있고 Docker 기반 개발환경 재현성이 좋다.
- `container-entrypoint-initdb.d` 초기화 스크립트 패턴을 지원해 `docker compose up` 직후 schema/bootstrap 자동화가 쉽다.
- 포트 1521, 5500, healthcheck, volume 유지 전략을 단일 compose 파일에서 설명하기 좋다.

## Compose 파일

- 파일: `/docker/docker-compose.yml`
- 실행 위치: `/docker`

```bash
docker compose up -d
```

## Oracle 접속 정보

- Host: `localhost`
- Port: `1521`
- Service Name: `FREEPDB1`
- SYS Password: `OracleSys1234!`
- App User: `app_user`
- App Password: `AppUser1234!`

## sqlplus 예시

SYS:

```bash
docker exec -it orderinventory-oracle sqlplus sys/OracleSys1234!@localhost:1521/FREEPDB1 as sysdba
```

APP_USER:

```bash
docker exec -it orderinventory-oracle sqlplus app_user/AppUser1234!@localhost:1521/FREEPDB1
```

## SQL Developer 접속 예시

- Connection Name: `OrderInventoryLocal`
- Username: `app_user`
- Password: `AppUser1234!`
- Hostname: `localhost`
- Port: `1521`
- Service name: `FREEPDB1`

## 자동 초기화 순서

1. `00_create_user.sql`
2. `01_tables.sql`
3. `02_sequences.sql`
4. `03_indexes.sql`
5. `04_partitioning.sql`
6. `05_sample_data.sql`
7. `pkg_order_spec.sql`
8. `pkg_order_body.sql`

초기화는 `docker/oracle/init/01_bootstrap.sql` 이 순서를 강제한다.

## 볼륨 운영 포인트

- 데이터는 `oracle_data` named volume 에 저장된다.
- 컨테이너 재시작만으로는 데이터가 지워지지 않는다.
- 완전 초기화가 필요하면 아래 순서를 사용한다.

```bash
docker compose down -v
docker volume prune
```

## 볼륨 백업

```bash
docker run --rm -v orderinventory_oracle_data:/source -v ${PWD}:/backup alpine tar czf /backup/oracle_data_backup.tar.gz -C /source .
```

