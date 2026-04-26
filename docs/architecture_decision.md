# 아키텍처 결정 기록

## ADR-001: Oracle + EF Core + PL/SQL 혼합 구조

### 결정

- 핵심 주문 로직은 Oracle PL/SQL package 유지
- EF Core 는 schema migration 과 단순 조회 모델에 사용
- Oracle 고유 기능은 SQL Script Migration으로 별도 관리

### 이유

- 주문/재고 로직은 Oracle 내부 잠금/예외/트랜잭션이 중요하다.
- EF Core 는 일반 엔티티 구조 변경 추적에 적합하다.
- package/partition/index 튜닝은 SQL script 로 관리하는 편이 더 실무적이다.

## ADR-002: DB 변경 관리 이중 구조

### 결정

- EF Core Migration: 일반 schema versioning
- SQL Script Migration: Oracle 전용 변경 이력

### 이유

- Oracle 전체를 EF Migration만으로 밀어 넣지 않기 위해서다.
- Oracle 특화 기능은 SQL script와 compile 검증이 더 자연스럽다.

## ADR-003: DbMigrator 별도 콘솔 프로젝트

### 결정

- `src/OrderInventory.DbMigrator` 를 별도 콘솔로 둔다.
- Docker Compose 에서 API 전에 1회 실행한다.

### 이유

- API startup 에 모든 마이그레이션 책임을 몰아주지 않기 위해서다.
- 실패 지점을 Oracle init / DbMigrator / API startup 으로 분리할 수 있다.

## ADR-004: 실행 방식 단순화

### 결정

- Docker Compose 실행
- 로컬 `dotnet run`
- publish exe 실행

### 이유

- 현재 프로젝트는 ASP.NET Core Web API 중심이므로 위 3가지가 가장 설명 가능하고 실용적이다.