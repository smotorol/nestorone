# 프로젝트 요약본

## 프로젝트 한 줄 설명

Oracle DB, PL/SQL package, versioned SQL Script Migration, ASP.NET Core API, WinForms Client를 조합한 주문/재고 관리 실무형 샘플 프로젝트다.

## 기술 스택

- Oracle Free / PL/SQL
- ASP.NET Core Web API (.NET 8)
- Dapper / Oracle.ManagedDataAccess
- WinForms (.NET 8 Windows)
- Docker Compose / Serilog / Swagger

## 프로젝트 구성

- `OrderInventory.Api`
- `OrderInventory.Application`
- `OrderInventory.Domain`
- `OrderInventory.Infrastructure`
- `OrderInventory.DbMigrator`
- `OrderInventory.Client.WinForms`

## 각 프로젝트 역할 한 줄 설명

- `OrderInventory.Api`: HTTP API 진입점이다.
- `OrderInventory.Application`: 서비스 흐름과 DTO를 담당한다.
- `OrderInventory.Domain`: 공통 도메인 확장 지점이다.
- `OrderInventory.Infrastructure`: Oracle SQL 조회와 stored procedure 호출을 담당한다.
- `OrderInventory.DbMigrator`: versioned SQL script migration을 실행한다.
- `OrderInventory.Client.WinForms`: Windows 데스크톱 테스트 클라이언트다.

## 실행 방식 요약

- Docker Compose: Oracle + DbMigrator + API 실행
- DbMigrator: versioned SQL script migration 실행
- API: 로컬 `dotnet run` 또는 publish exe 실행
- Client: 로컬 WinForms 실행
