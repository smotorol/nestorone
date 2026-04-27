# 프로젝트 상세 정리

## 전체 아키텍처 설명

이 프로젝트는 Oracle DB 중심 구조를 기준으로 설계했다. 핵심 쓰기 트랜잭션은 Oracle PL/SQL package에서 처리하고, ASP.NET Core API는 HTTP 요청 처리, 예외 표준화, 응답 포맷, 계층 분리 역할에 집중한다. 상품 조회는 `Infrastructure` 에서 Dapper 기반 SQL 조회로 처리하고, DB 변경 관리는 `DbMigrator + versioned SQL script` 중심으로 운영한다.

## 프로젝트별 상세 역할

### OrderInventory.Api
- 외부 클라이언트 요청을 받아 서비스 계층으로 연결하고, 공통 예외 응답, Swagger, Health Check를 제공한다.

### OrderInventory.Application
- 서비스 인터페이스와 DTO를 정의하고, API와 저장소 구현 사이 계약을 분리한다.

### OrderInventory.Domain
- 현재 구현 범위에서는 얇지만, 공통 도메인 규칙이나 값 객체를 둘 수 있는 확장 지점이다.

### OrderInventory.Infrastructure
- 상품 조회 SQL과 주문 처리 DB 접근을 한곳에 모은다.
- `ProductRepository` 는 Dapper 기반 SQL 조회를 수행한다.
- `OrderRepository` 는 `pkg_order.create_order`, `pkg_order.cancel_order` 를 호출한다.

### OrderInventory.DbMigrator
- baseline schema, sequence, index, package, seed 같은 Oracle 변경을 versioned SQL script로 실행한다.
- `DB_SCRIPT_MIGRATION_HISTORY` 를 기록하고 package compile error를 검증한다.

### OrderInventory.Client.WinForms
- API를 Windows 클라이언트 관점에서도 검증하기 위한 테스트 클라이언트다.

## 전체 데이터 흐름

### 상품 조회 흐름
1. WinForms 또는 Swagger에서 상품 조회 요청
2. `ProductsController` 가 요청 수신
3. `IProductService` 호출
4. `ProductRepository` 가 Dapper SQL 조회 실행
5. Oracle `PRODUCTS`, `CATEGORIES` 조회
6. DTO를 API 응답으로 반환

### 주문 생성 흐름
1. WinForms/API 요청
2. `OrdersController` 수신
3. `IOrderService` 호출
4. `OrderRepository` 실행
5. Oracle `pkg_order.create_order` 호출
6. `ORDERS`, `ORDER_ITEMS`, `STOCK_HISTORY` 반영
7. 결과 코드/메시지 반환

### 주문 취소 흐름
1. API 요청
2. `OrdersController.Cancel()` 실행
3. `OrderRepository.CancelOrderAsync()` 호출
4. Oracle `pkg_order.cancel_order` 실행
5. 주문 상태 변경, 재고 복구, 이력 적재
6. 결과 반환

## Oracle 변경 관리 전략

- Docker init SQL
  - Oracle 컨테이너 최초 기동 시 사용자/권한 생성만 담당한다.
- SQL Script Migration
  - baseline schema, sequence, package, seed, Oracle 전용 변경을 담당한다.
- DbMigrator
  - SQL script 실행과 이력/검증을 담당한다.

## 왜 SQL Script Migration 중심으로 정리했는가

- Oracle package, sequence, advanced index, partitioning 같은 변경은 EF migration보다 SQL script가 더 직접적이다.
- 핵심 로직이 Oracle 객체에 강하게 의존하기 때문에 DB 변경 이력도 Oracle 객체 중심으로 관리하는 편이 자연스럽다.
- baseline 과 증분 이력을 같은 버전 체계로 관리하면 Docker, 로컬, CI/CD 흐름이 단순해진다.
