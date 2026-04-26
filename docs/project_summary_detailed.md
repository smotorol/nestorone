# 프로젝트 상세 정리

## 전체 아키텍처 설명

이 프로젝트는 Oracle DB 중심 구조를 기준으로 설계했다. 핵심 쓰기 트랜잭션은 Oracle PL/SQL package에서 처리하고, ASP.NET Core API는 HTTP 요청 처리, 예외 표준화, 응답 포맷, 계층 분리 역할에 집중한다. 상품 조회 같은 단순 read 시나리오는 EF Core DbContext를 사용해 read model처럼 다루고, Oracle 전용 변경은 별도의 SQL Script Migration과 DbMigrator로 관리한다.

## 프로젝트별 상세 역할

### OrderInventory.Api

간략 설명:
- HTTP API 진입점이다.

상세 설명:
- 왜 필요한가
  - 외부 클라이언트 요청을 받아 서비스 계층으로 연결하고, 공통 예외 응답, Swagger, Health Check를 제공하기 위해 필요하다.
- 중요한 파일
  - `Program.cs`
  - `Controllers/ProductsController.cs`
  - `Controllers/OrdersController.cs`
  - `Middlewares/GlobalExceptionMiddleware.cs`
- 연결 계층
  - WinForms / Swagger / HTTP Client 와 직접 연결되고, 내부적으로 Application, Infrastructure, Persistence를 주입받는다.
- 책임
  - DI 구성
  - 미들웨어 구성
  - Controller 라우팅
  - Health Check / Swagger 노출
- 면접 설명 포인트
  - “API 계층은 비즈니스 로직을 직접 처리하기보다 요청/응답과 예외 표준화, 의존성 조립에 집중했습니다.”

### OrderInventory.Application

간략 설명:
- 비즈니스 서비스 흐름과 DTO를 담당한다.

상세 설명:
- 왜 필요한가
  - API가 Repository나 DB 접근 구현과 직접 결합되지 않도록 중간 서비스/계약 계층이 필요하다.
- 중요한 파일
  - `Abstractions/IProductService.cs`
  - `Abstractions/IOrderService.cs`
  - `Abstractions/IProductRepository.cs`
  - `Abstractions/IOrderRepository.cs`
  - `Services/ProductService.cs`
  - `Services/OrderService.cs`
  - `Dtos/*`
- 연결 계층
  - Api ↔ Application ↔ Infrastructure/Persistence
- 책임
  - 서비스 인터페이스 정의
  - DTO 정의
  - API와 저장소 구현 사이 계약 분리
- 면접 설명 포인트
  - “서비스 계층은 복잡한 비즈니스 로직보다는 흐름 조율과 계층 분리를 위해 뒀고, 핵심 DB 트랜잭션은 Oracle package로 넘겼습니다.”

### OrderInventory.Domain

간략 설명:
- 공통 도메인 확장 지점이다.

상세 설명:
- 왜 필요한가
  - 현재 구현 범위에서는 비어 있지만, 공통 도메인 규칙이나 값 객체를 둘 수 있는 자리로 분리해 두었다.
- 중요한 파일
  - 현재는 csproj 중심의 얇은 계층이다.
- 연결 계층
  - Application, Infrastructure가 참조한다.
- 책임
  - 향후 확장 시 공통 도메인 모델 수용
- 면접 설명 포인트
  - “작게 시작했기 때문에 Domain은 얇지만, 계층 구조를 무너뜨리지 않기 위해 독립 프로젝트로 분리해 두었습니다.”

### OrderInventory.Infrastructure

간략 설명:
- Oracle stored procedure 호출과 외부 DB 접근을 담당한다.

상세 설명:
- 왜 필요한가
  - 상품 조회와 주문 처리의 DB 접근 방식을 한곳에 모으고, Oracle 특화 호출 코드를 API에서 분리하기 위해 필요하다.
- 중요한 파일
  - `Repositories/ProductRepository.cs`
  - `Repositories/OrderRepository.cs`
  - `Persistence/OracleConnectionFactory.cs`
- 연결 계층
  - Application이 정의한 인터페이스를 구현하고, Persistence DbContext 또는 OracleConnectionFactory를 사용한다.
- 책임
  - EF Core 기반 상품 조회
  - `pkg_order.create_order`, `pkg_order.cancel_order` 호출
  - OracleParameter, CLOB, output parameter 처리
- 면접 설명 포인트
  - “주문 생성/취소는 EF Core가 아니라 Oracle package 호출로 처리해서 DB 중심 트랜잭션 구조를 보여주도록 했습니다.”

### OrderInventory.Persistence

간략 설명:
- EF Core DbContext와 read/query 모델을 담당한다.

상세 설명:
- 왜 필요한가
  - EF Core read model과 엔티티 매핑을 독립 계층으로 관리하기 위해 필요하다.
- 중요한 파일
  - `Contexts/OrderInventoryDbContext.cs`
  - `Entities/ProductEntity.cs`
  - `Entities/CategoryEntity.cs`
  - `Configurations/*`
  - `Factories/OrderInventoryDbContextFactory.cs`
- 연결 계층
  - Api에서 DbContext DI 구성에 사용되고, Infrastructure의 ProductRepository가 사용한다.
- 책임
  - PRODUCTS / CATEGORIES read model 매핑
  - Oracle NUMBER 타입 매핑
  - design-time DbContext 제공
- 면접 설명 포인트
  - “조회 기능은 EF Core로 단순화하고, 쓰기 트랜잭션은 Oracle package로 분리해 역할을 나눴습니다.”

### OrderInventory.Migrations

간략 설명:
- EF Core migration 이력을 관리한다.

상세 설명:
- 왜 필요한가
  - 일반 테이블/컬럼 구조 변경 이력을 별도 프로젝트로 관리하기 위해 필요하다.
- 중요한 파일
  - `MigrationRunner.cs`
  - EF migration 산출물
- 연결 계층
  - Persistence DbContext를 참조하고, DbMigrator나 API에서 migration assembly로 사용한다.
- 책임
  - `__EFMigrationsHistory` 기반 schema versioning
- 면접 설명 포인트
  - “Oracle 전체를 EF로 관리하지는 않지만, 일반 엔티티 구조 변경은 EF migration 프로젝트로 별도 관리했습니다.”

### OrderInventory.DbMigrator

간략 설명:
- Oracle 전용 SQL script migration을 실행한다.

상세 설명:
- 왜 필요한가
  - package, seed, Oracle 전용 DDL을 EF migration과 분리해 실행하기 위해 필요하다.
- 중요한 파일
  - `Program.cs`
  - `Services/SqlScriptMigrationRunner.cs`
  - `db/migrations_sql/*`
- 연결 계층
  - Migrations와 Persistence를 참조하고, Oracle DB에 직접 연결한다.
- 책임
  - SQL script 실행
  - `DB_SCRIPT_MIGRATION_HISTORY` 기록
  - package compile error 검증
  - 간단 summary 출력
- 면접 설명 포인트
  - “EF Core Migration과 별도로 Oracle 전용 변경을 관리하기 위해 DbMigrator 콘솔을 뒀고, 적용 이력과 checksum도 함께 관리했습니다.”

### OrderInventory.Client.WinForms

간략 설명:
- Windows 데스크톱 테스트 클라이언트다.

상세 설명:
- 왜 필요한가
  - API를 브라우저/Swagger 외에 Windows 클라이언트 관점에서도 검증하기 위해 필요하다.
- 중요한 파일
  - `Forms/MainForm.cs`
  - `Services/ApiClient.cs`
  - `Models/*`
- 연결 계층
  - HTTP로 Api와 직접 연결된다.
- 책임
  - 상품 목록 조회
  - 선택 상품 주문 생성
  - 간단한 결과 메시지 표시
- 면접 설명 포인트
  - “운영용 프론트엔드보다는, Windows 데스크톱 클라이언트 경험과 API 검증 흐름을 보여주는 용도로 WinForms를 배치했습니다.”

## 전체 데이터 흐름

### 상품 조회 흐름 요약

1. WinForms 또는 Swagger에서 상품 조회 요청
2. `ProductsController` 가 요청 수신
3. `IProductService` 호출
4. `ProductRepository` 가 EF Core DbContext 사용
5. Oracle `PRODUCTS`, `CATEGORIES` 조회
6. DTO를 API 응답으로 반환

### 상품 조회 흐름 상세

- WinForms `ApiClient.GetProductsAsync()` 또는 Swagger 요청이 `/api/products` 로 들어간다.
- API는 `ProductsController.GetProducts()` 에서 Application 계층으로 전달한다.
- `ProductService` 는 `IProductRepository` 를 호출한다.
- `ProductRepository` 는 `OrderInventoryDbContext` 와 `AsNoTracking()` 기반 LINQ 조회를 사용한다.
- Oracle의 `PRODUCTS` 와 `CATEGORIES` 데이터를 `ProductSummaryDto` 로 투영해 반환한다.

### 주문 생성 흐름 요약

1. WinForms/API 요청
2. `OrdersController` 수신
3. `IOrderService` 호출
4. `OrderRepository` 실행
5. Oracle `pkg_order.create_order` 호출
6. `ORDERS`, `ORDER_ITEMS`, `STOCK_HISTORY` 반영
7. 결과 코드/메시지 반환

### 주문 생성 흐름 상세

- WinForms는 선택한 상품과 수량을 `CreateOrderRequest` 로 만든다.
- API는 `CreateOrderRequestDto` 를 받아 `OrderService` 로 전달한다.
- `OrderService` 는 `OrderRepository.CreateOrderAsync()` 를 호출한다.
- `OrderRepository` 는 `OracleCommand` 를 사용해 `pkg_order.create_order` stored procedure를 호출한다.
- 주문 아이템은 JSON 문자열로 직렬화해 CLOB 파라미터로 전달한다.
- Oracle package 내부에서 재고 확인, 주문 헤더 생성, 주문 상세 생성, 재고 차감, 이력 적재, 오류 로그 처리까지 수행한다.
- output parameter를 읽어 주문번호와 결과 메시지를 API 응답으로 돌려준다.

### 주문 취소 흐름 요약

1. API 요청
2. `OrdersController.Cancel()` 실행
3. `OrderRepository.CancelOrderAsync()` 호출
4. Oracle `pkg_order.cancel_order` 실행
5. 주문 상태 변경, 재고 복구, 이력 적재
6. 결과 반환

### 주문 취소 흐름 상세

- API는 주문 ID와 취소 사유를 받아 Application 계층으로 전달한다.
- Infrastructure는 `pkg_order.cancel_order` 를 호출한다.
- Oracle package는 주문 상태를 점검한 뒤, `ORDER_ITEMS` 기준으로 재고를 복구하고 `STOCK_HISTORY` 에 이력을 남긴다.
- 취소 완료 후 결과 코드를 반환한다.

## Oracle 변경 관리 전략

- Docker init SQL
  - Oracle 컨테이너 최초 기동 시 baseline schema, sequence, index를 만든다.
- EF Core Migration
  - 일반 테이블/컬럼/엔티티 구조 변경을 담당한다.
- SQL Script Migration
  - package, seed, Oracle 전용 변경을 담당한다.
- DbMigrator
  - SQL script 실행과 이력/검증을 담당한다.

## EF Core + SQL Script Migration 병행 이유

- EF Core는 일반 entity schema versioning 에 적합하다.
- Oracle package, advanced index, seed, partitioning은 SQL script가 더 명확하다.
- 둘을 섞지 않고 분리하면 변경 이력과 리뷰 포인트가 더 선명해진다.

## WinForms/WPF 비교 및 현재 선택 이유

- WinForms
  - 구현 속도가 빠르다.
  - 관리자 툴 느낌을 내기 쉽다.
  - API 검증용 클라이언트에 적합하다.
- WPF
  - MVVM, 데이터 바인딩, 고급 UI 구성에 유리하다.
  - 하지만 현재 프로젝트 범위에는 과하다고 판단했다.

현재 프로젝트는 Oracle/API 설명이 중심이므로, UI 프레임워크는 WinForms로 단순화했다.

## 테스트 흐름

1. Docker Compose로 Oracle 실행
2. DbMigrator 실행
3. API 실행
4. `/health`, `/swagger` 확인
5. 상품 조회 API 확인
6. 주문 생성/취소 API 확인
7. WinForms에서 동일 시나리오 확인

## 확장 가능성

- 주문 조회/취소 이력 화면 추가
- 관리자용 재고 조정 화면 추가
- 인증/권한 추가
- 장애 추적을 위한 audit logging 강화
- SQL script runner를 운영 배포 파이프라인에 연결
- WPF 또는 웹 프론트엔드로 UI 확장
