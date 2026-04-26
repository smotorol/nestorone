# 마이그레이션 전략

## 1. 핵심 원칙

EF Core Migration은 일반 테이블 구조와 단순 엔티티 변경 이력을 관리하고, Oracle 고유 기능인 PL/SQL package, partitioning, advanced index, grant, seed script는 별도 SQL migration runner로 관리한다.

## 2. 역할 분리

### EF Core Migration 담당

- 기본 테이블/컬럼
- 단순 관계
- 일반 엔티티 구조 변경
- `__EFMigrationsHistory` 기반 버전 추적

### SQL Script Migration 담당

- PL/SQL package
- Oracle 전용 seed data
- advanced index
- grant
- partitioning
- 성능 튜닝용 Oracle 전용 스크립트

## 3. 왜 EF Core Migration만으로 Oracle 전체를 관리하지 않는가

- Oracle package, partitioning, advanced index는 EF Core 모델로 표현력이 부족하거나 억지 매핑이 된다.
- 핵심 업무 로직은 여전히 Oracle 내부 트랜잭션과 compile 검증이 중요하다.
- Oracle 고유 기능을 EF Migration에 전부 넣으면 운영 반영과 리뷰가 오히려 어려워진다.

## 4. 병행 전략

- 베이스라인 스키마: Docker init SQL
- 일반 엔티티 변경: EF Core Migration
- Oracle 전용 변경: `db/migrations_sql` + `OrderInventory.DbMigrator`

## 5. SQL Migration 파일 규칙

폴더:

- `db/migrations_sql`

파일명 규칙:

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

1. Oracle container init 으로 baseline schema 생성
2. `OrderInventory.DbMigrator` 실행
   - 내부에서 EF Core migration 적용
   - 이어서 SQL script migration 적용
3. API 기동

### 로컬 환경

1. Oracle baseline 준비
2. 필요 시 `OrderInventory.DbMigrator` 실행
3. API 기동

## 8. SQL 파일 BOM 제거 방법

```powershell
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
Get-ChildItem docker/oracle/init, db/schema, db/plsql, db/migrations_sql -Recurse -File |
  ForEach-Object {
    $content = [System.IO.File]::ReadAllText($_.FullName)
    [System.IO.File]::WriteAllText($_.FullName, $content, $utf8NoBom)
  }
```

## 9. DbMigrator 재실행 정책

- 성공 이력이 있고 checksum 이 같은 script 는 SKIP 한다.
- checksum 이 달라지면 실패 또는 운영 검토 대상으로 본다.
- package script 는 적용 후 `user_errors` 로 compile 상태를 검증한다.

## 10. 운영 적용 전략

- 개발: Docker init + DbMigrator 자동 적용 허용
- 스테이징: EF migration + SQL script runner 검증
- 운영: reviewed SQL script / 배포 파이프라인에서 단계 적용 권장
