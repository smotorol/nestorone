# 프로젝트 요약본

## 프로젝트 한 줄 설명

Oracle DB, PL/SQL package, EF Core Migration, SQL Script Migration, ASP.NET Core API, WinForms Client를 조합한 주문/재고 관리 실무형 샘플 프로젝트다.

## 기술 스택

- Oracle Free / PL/SQL
- ASP.NET Core Web API (.NET 7)
- EF Core / Oracle EntityFrameworkCore
- Oracle.ManagedDataAccess / Dapper
- WinForms (.NET 7 Windows)
- Docker Compose / Serilog / Swagger

## 프로젝트 구성

- `OrderInventory.Api`
- `OrderInventory.Application`
- `OrderInventory.Domain`
- `OrderInventory.Infrastructure`
- `OrderInventory.Persistence`
- `OrderInventory.Migrations`
- `OrderInventory.DbMigrator`
- `OrderInventory.Client.WinForms`

## 주요 기능

- 상품 목록 조회
- 상품 상세 조회
- 주문 생성
- 주문 취소
- 재고 차감/복구
- 재고/오류 이력 기록
- Oracle package compile 및 SQL script migration 관리

## 각 프로젝트 역할 한 줄 설명

- `OrderInventory.Api`: HTTP API 진입점이다.
- `OrderInventory.Application`: 서비스 흐름과 DTO를 담당한다.
- `OrderInventory.Domain`: 공통 도메인 확장 지점이다.
- `OrderInventory.Infrastructure`: Oracle stored procedure 호출과 DB 접근을 담당한다.
- `OrderInventory.Persistence`: EF Core DbContext와 read/query 모델을 담당한다.
- `OrderInventory.Migrations`: EF Core migration 이력을 관리한다.
- `OrderInventory.DbMigrator`: Oracle 전용 SQL script migration을 실행한다.
- `OrderInventory.Client.WinForms`: Windows 데스크톱 테스트 클라이언트다.

## 실행 방식 요약

- Docker Compose: Oracle 개발 DB 실행
- DbMigrator: 로컬에서 SQL script migration 실행
- API: 로컬 `dotnet run`
- Client: 로컬 WinForms 실행

## 면접용 30초 설명

이 프로젝트는 Oracle 실무 경험을 보여주기 위해 주문 생성과 취소 같은 핵심 트랜잭션을 PL/SQL package로 처리하고, 상품 조회는 EF Core read model로 분리한 구조입니다. DB 변경 관리도 EF Core Migration과 Oracle 전용 SQL Script Migration을 나눠서 운영성을 높였고, ASP.NET Core API와 WinForms 클라이언트로 전체 흐름을 직접 검증할 수 있게 구성했습니다.
