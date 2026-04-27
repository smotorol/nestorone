# 마이그레이션 전략

## 1. 핵심 원칙

이 프로젝트는 Oracle 전용 기능이 많기 때문에 DB 변경 관리를 `DbMigrator + versioned SQL script` 중심으로 운영한다. 테이블, 시퀀스, 인덱스, package, seed 모두 `db/migrations_sql` 아래 버전 파일로 관리하고, `DB_SCRIPT_MIGRATION_HISTORY` 로 적용 이력을 추적한다.

## 2. 현재 구조

### Docker init 담당

- PDB 진입
- `APP_USER` 생성 및 권한 부여
- 애플리케이션 스키마 자체는 생성하지 않음

### SQL Script Migration 담당

- baseline schema 생성
- sequence 생성
- runtime index 생성
- PL/SQL package 생성/교체
- seed data 반영
- Oracle 전용 DDL 반영

## 3. 왜 EF migration을 제거했는가

- 현재 프로젝트의 핵심 쓰기 로직은 Oracle `pkg_order` package 에 있다.
- 주문/재고 처리에서 `sequence`, `PL/SQL`, Oracle DDL 의존성이 크다.
- partitioning 같은 Oracle 전용 기능은 EF migration보다 SQL script가 더 직접적이고 검토하기 쉽다.
- 실제 운영 흐름도 “Oracle 객체 자체”를 기준으로 버전 관리하는 편이 더 자연스럽다.

## 4. baseline + 증분 전략

- baseline: `V000__baseline_schema.sql`
- 증분 변경: `V001__...sql`, `V002__...sql`, `V003__...sql` 식으로 계속 추가
- 기존 성공 script 는 수정하지 않고 새 버전 script 를 추가
- 모든 적용 이력은 `DB_SCRIPT_MIGRATION_HISTORY` 에 기록

## 5. SQL Migration 파일 규칙

폴더:

- `db/migrations_sql`

파일명 규칙:

- `V000__baseline_schema.sql`
- `V001__create_package_pkg_order_spec.sql`
- `V002__create_package_pkg_order_body.sql`
- `V003__seed_products.sql`

## 6. DB_SCRIPT_MIGRATION_HISTORY

컬럼:

- `SCRIPT_ID`
- `SCRIPT_NAME`
- `CHECKSUM`
- `APPLIED_AT`
- `APPLIED_BY`
- `SUCCESS_YN`
- `ERROR_MESSAGE`

## 7. 권장 적용 흐름

### Docker 환경

1. Oracle container init 으로 사용자/권한 생성
2. `OrderInventory.DbMigrator` 실행
   - baseline schema 적용
   - package 적용
   - seed 적용
3. API 기동

### 로컬 환경

1. Oracle 준비
2. `OrderInventory.DbMigrator` 실행
3. API 기동

## 8. DbMigrator 재실행 정책

- 성공 이력이 있고 checksum 이 같은 script 는 SKIP 한다.
- checksum 이 달라지면 재적용이 아니라 검토 대상으로 본다.
- package script 는 적용 후 `user_errors` 로 compile 상태를 검증한다.

## 9. 운영 적용 전략

- 개발: Docker + DbMigrator 자동 적용 허용
- 스테이징: DbMigrator 실행 후 health/API 검증
- 운영: reviewed SQL script 를 배포 파이프라인에서 순차 적용
