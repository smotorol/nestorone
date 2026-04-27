# 주문/재고 미니 시스템

Oracle DB, ASP.NET Core API, WinForms Client를 기반으로 한 주문/재고 관리 실무형 샘플 프로젝트다. 상품 조회는 SQL 기반 조회로 처리하고, 주문 생성/취소 같은 핵심 트랜잭션은 Oracle PL/SQL package로 처리한다. DB 변경 관리는 `DbMigrator + versioned SQL script` 중심으로 구성했다.

## 프로젝트 개요

- Oracle DB 설계
- PL/SQL package 기반 핵심 업무 처리
- versioned SQL Script Migration + DbMigrator 기반 변경 관리
- ASP.NET Core Web API 계층 구조
- WinForms 기반 Windows 테스트 클라이언트
- Docker Compose 기반 로컬 Oracle 개발 환경
- GitHub Actions / GHCR 기반 CI/CD 준비

## 기술 스택

- DB: Oracle Free / Oracle SQL / PL/SQL
- Server: ASP.NET Core Web API (.NET 8)
- Client: WinForms (.NET 8 Windows)
- DB Access: Dapper, Oracle.ManagedDataAccess
- Infra: Docker Compose, Serilog, Swagger, GitHub Actions, GHCR

## 솔루션 구조

- `OrderInventory.Api`
- `OrderInventory.Application`
- `OrderInventory.Domain`
- `OrderInventory.Infrastructure`
- `OrderInventory.DbMigrator`
- `OrderInventory.Client.WinForms`

## 각 프로젝트 역할

- `OrderInventory.Api`
  - HTTP API 진입점이다. Controller, 미들웨어, Swagger, Health Check, DI 구성을 담당한다.
- `OrderInventory.Application`
  - 서비스 인터페이스와 DTO를 정의하고, API와 Repository 사이 서비스 흐름을 조율한다.
- `OrderInventory.Domain`
  - 공통 도메인 확장 지점 역할을 한다.
- `OrderInventory.Infrastructure`
  - Oracle SQL 조회와 Stored Procedure 호출을 담당한다. 주문 생성/취소는 이 계층에서 `pkg_order` 를 호출한다.
- `OrderInventory.DbMigrator`
  - `db/migrations_sql` 아래 versioned SQL script를 실행하고 이력을 남긴다.
- `OrderInventory.Client.WinForms`
  - 상품 조회와 주문 생성 흐름을 검증하는 Windows 데스크톱 테스트 클라이언트다.

## 전체 아키텍처 요약

- 상품 조회
  - WinForms/HTTP Client → API Controller → Application Service → Infrastructure Repository → Oracle SQL 조회
- 주문 생성/취소
  - WinForms/HTTP Client → API Controller → Application Service → Infrastructure Repository → Oracle `pkg_order` 실행
- DB 변경 관리
  - Docker init: Oracle 사용자/권한 생성
  - baseline + 증분 변경: `db/migrations_sql` + `OrderInventory.DbMigrator`

## Oracle / Dapper / DbMigrator 역할 분리

- SQL 조회
  - 상품 목록/상세 조회를 담당한다.
- PL/SQL Package
  - 주문 생성/취소, 재고 차감/복구, 오류 기록처럼 DB 중심 트랜잭션이 필요한 로직을 처리한다.
- DbMigrator
  - `db/migrations_sql` 스크립트를 실행하고 `DB_SCRIPT_MIGRATION_HISTORY` 에 적용 이력을 남긴다.
- Oracle DB
  - 실제 데이터 저장소이자 트랜잭션 처리 엔진이다.

## 실행 방법

### 로컬 Docker 배포

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\deploy-local-docker.ps1
```

확인 주소:

- `http://localhost:8080/health`
- `http://localhost:8080/swagger`

### 로컬 publish exe 배포

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\publish-api-local.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\run-api-published.ps1
```

## 테스트 방법

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-api-flow.ps1 -BaseUrl 'http://localhost:8080'
```

확인 항목:

- `/health`
- `/swagger`
- 상품 조회
- 주문 생성
- 주문 취소
- 재고 부족 실패 응답 code/message

## 현재 검증 상태

실제로 확인한 항목:

- Docker 통합 구조에서 Oracle, DbMigrator, API 기동 확인
- `/health` HTTP 200 확인
- `/swagger` HTTP 200 확인
- 상품 조회 성공 확인
- 주문 생성 성공 확인
- 주문 취소 성공 확인
- 재고 부족 실패 응답 확인
- `PKG_ORDER` PACKAGE / PACKAGE BODY `VALID` 확인
- `DB_SCRIPT_MIGRATION_HISTORY` 적용 이력 확인
- DbMigrator 재실행 시 `SKIP` 확인
- API publish 성공

## 면접 설명 포인트

- 조회는 SQL 기반 조회, 핵심 쓰기 트랜잭션은 Oracle package로 분리했다.
- Oracle 전용 기능이 많아서 DB 변경 관리는 versioned SQL Script Migration + DbMigrator 중심으로 정리했다.
- Docker 환경에서는 Oracle 사용자/권한만 init에서 만들고, 실제 스키마/패키지/seed 변경은 DbMigrator가 맡는다.
- WinForms 클라이언트를 붙여 API 기능 검증과 Windows 클라이언트 연동 경험까지 보여주도록 구성했다.
