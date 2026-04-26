# 주문/재고 미니 시스템

Oracle DB, ASP.NET Core API, WinForms Client를 기반으로 한 주문/재고 관리 실무형 샘플 프로젝트다. 상품 조회는 EF Core 기반 read model로 처리하고, 주문 생성/취소 같은 핵심 트랜잭션은 Oracle PL/SQL package로 처리한다.

## 프로젝트 개요

이 프로젝트의 목적은 단순 CRUD 데모가 아니라 아래 조합을 설명 가능한 형태로 보여주는 것이다.

- Oracle DB 설계
- PL/SQL package 기반 핵심 업무 처리
- EF Core Migration과 Oracle SQL Script Migration 병행
- ASP.NET Core Web API 계층 구조
- WinForms 기반 Windows 테스트 클라이언트
- Docker Compose 기반 로컬 Oracle 개발 환경

## 기술 스택

- DB: Oracle Free / Oracle SQL / PL/SQL
- Server: ASP.NET Core Web API (.NET 7)
- Client: WinForms (.NET 7 Windows)
- ORM / DB Access: EF Core, Oracle.EntityFrameworkCore, Oracle.ManagedDataAccess, Dapper
- Migration: EF Core Migration + SQL Script Migration + DbMigrator
- Infra: Docker Compose, Serilog, Swagger, GitHub Actions, GHCR

## 솔루션 구조

- `OrderInventory.Api`
- `OrderInventory.Application`
- `OrderInventory.Domain`
- `OrderInventory.Infrastructure`
- `OrderInventory.Persistence`
- `OrderInventory.Migrations`
- `OrderInventory.DbMigrator`
- `OrderInventory.Client.WinForms`

## 각 프로젝트 역할

- `OrderInventory.Api`
  - HTTP API 진입점이다. Controller, 미들웨어, Swagger, Health Check, DI 구성을 담당한다.
- `OrderInventory.Application`
  - 서비스 인터페이스와 DTO를 정의하고, API와 Repository 사이 서비스 흐름을 단순하게 조율한다.
- `OrderInventory.Domain`
  - 현재 구조에서는 공통 도메인/기초 계층 자리로 두고 있으며, 확장 지점 역할을 한다.
- `OrderInventory.Infrastructure`
  - Oracle Stored Procedure 호출과 외부 DB 접근을 담당한다. 주문 생성/취소는 이 계층에서 `pkg_order` 를 호출한다.
- `OrderInventory.Persistence`
  - EF Core DbContext, Entity, Configuration을 관리한다. 상품 조회 같은 read/query 모델을 담당한다.
- `OrderInventory.Migrations`
  - EF Core Migration 이력을 관리한다.
- `OrderInventory.DbMigrator`
  - Oracle 전용 SQL Script Migration을 실행하고 이력을 남긴다.
- `OrderInventory.Client.WinForms`
  - 상품 조회와 주문 생성 흐름을 검증하는 Windows 데스크톱 테스트 클라이언트다.

## 전체 아키텍처 요약

- 상품 조회
  - WinForms/HTTP Client → API Controller → Application Service → EF Core DbContext → Oracle PRODUCTS 조회
- 주문 생성/취소
  - WinForms/HTTP Client → API Controller → Application Service → Infrastructure Repository → Oracle `pkg_order` 실행
- DB 변경 관리
  - baseline schema: Docker init SQL
  - 일반 schema 변경: EF Core Migration
  - Oracle 전용 변경: SQL Script Migration + DbMigrator

## Oracle / EF Core / DbMigrator 역할 분리

- EF Core Migration
  - 일반 테이블/컬럼/엔티티 구조 변경을 관리한다.
- SQL Script Migration
  - Oracle 전용 기능을 관리한다. package, seed, advanced index, partitioning 같은 항목이 여기에 속한다.
- PL/SQL Package
  - 주문 생성/취소, 재고 차감/복구, 오류 기록처럼 DB 중심 트랜잭션이 필요한 로직을 처리한다.
- DbMigrator
  - `db/migrations_sql` 스크립트를 실행하고 `DB_SCRIPT_MIGRATION_HISTORY` 에 적용 이력을 남긴다.
- Oracle DB
  - 실제 데이터 저장소이자 트랜잭션 처리 엔진이다.

## WinForms 클라이언트 설명

WinForms는 이 프로젝트에서 운영용 UI가 아니라 Windows 데스크톱 클라이언트 경험과 API 연동 흐름을 보여주기 위한 검증용 클라이언트다.

현재 구현된 화면/기능:

- 상품 목록 조회
- 검색어 기반 상품 조회
- 선택 상품 주문 생성
- 결과 메시지 표시

## WinForms와 WPF 차이

- WinForms
  - 빠르게 데스크톱 테스트 UI를 만들기 좋다.
  - 단순한 업무 화면, 관리자 도구, 내부 툴 느낌을 내기 쉽다.
- WPF
  - MVVM, 바인딩, 복잡한 UI 구성에 더 유리하다.
  - 화면 규모와 표현력이 커질수록 장점이 커진다.

이 프로젝트는 포트폴리오 범위를 과도하게 키우지 않고 API/Oracle 설명에 집중하기 위해 WinForms를 선택했다.

## 실행 방법

### 1. Docker Compose 통합 실행

```powershell
cd docker
docker compose down -v
docker compose up -d --build
docker compose ps
docker logs orderinventory-oracle
docker logs orderinventory-db-migrator
docker logs orderinventory-api
```

기본 흐름:

1. `oracle` 기동
2. `db-migrator` 실행
3. `api` 실행

확인 주소:

- `http://localhost:8080/health`
- `http://localhost:8080/swagger`

### 2. 로컬 실행 대안

필요하면 Oracle은 Docker로 실행하고, `DbMigrator` 와 `API` 는 로컬에서 실행할 수 있다. 디버깅과 브레이크포인트 확인이 필요할 때 유용하다.

## 테스트 방법

### Docker 기준 핵심 검증 순서

1. `http://localhost:8080/health`
2. `http://localhost:8080/swagger`
3. `GET /api/products`
4. `POST /api/orders`
5. `POST /api/orders/{id}/cancel`
6. 재고 부족 실패 시나리오 확인

PowerShell 예시:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-api-flow.ps1
```

### Oracle 검증 SQL

```sql
SELECT object_name, object_type, status
FROM user_objects
WHERE object_name IN ('PKG_ORDER', 'DB_SCRIPT_MIGRATION_HISTORY')
ORDER BY object_type, object_name;

SELECT name, type, line, position, text
FROM user_errors
WHERE name = 'PKG_ORDER'
ORDER BY sequence;

SELECT script_name, success_yn, applied_at
FROM db_script_migration_history
ORDER BY applied_at;
```

## CI/CD 준비

- CI: `.github/workflows/ci.yml`
  - Windows에서 솔루션 build/publish 검증
  - Ubuntu에서 API/DbMigrator Docker 이미지 build 검증
- CD: `.github/workflows/cd-docker.yml`
  - 태그 push 또는 수동 실행 시 GHCR로 Docker 이미지 발행
- 추가 메모: `docs/ci_cd.md`

## 현재 검증 상태

실제로 확인한 항목:

- `docker compose config` 성공
- Docker 통합 구조에서 Oracle, DbMigrator, API 단계별 기동 확인
- `/health` HTTP 200 확인
- `/swagger` HTTP 200 확인
- 상품 조회 성공 확인
- 주문 생성 성공 확인
- 주문 취소 성공 확인
- 재고 부족 실패 응답 확인
- `PKG_ORDER` PACKAGE / PACKAGE BODY `VALID` 확인
- `DB_SCRIPT_MIGRATION_HISTORY` 적용 이력 확인
- DbMigrator 재실행 시 `SKIP` 확인

아직 확인하지 않은 항목:

- GitHub Actions CI 원격 실행 결과
- GHCR 실제 이미지 발행 결과
- Docker Compose clean run 반복 실행 안정성

## 면접 설명 포인트

- 조회는 EF Core, 핵심 쓰기 트랜잭션은 Oracle package로 분리했다.
- EF Core Migration과 SQL Script Migration을 병행해 Oracle 고유 기능까지 관리 가능한 구조로 만들었다.
- Docker 환경에서는 API startup migration을 끄고 DbMigrator가 DB 변경 책임을 맡게 해 운영 흐름을 분리했다.
- WinForms 클라이언트를 붙여 API 기능 검증과 Windows 클라이언트 연동 경험까지 보여주도록 구성했다.
