# 면접 설명 정리

## 30초 설명

Oracle DB 기반 주문/재고 관리 프로젝트를 만들면서, 상품 조회는 EF Core read model로 처리하고 주문 생성과 취소는 PL/SQL package로 분리했습니다. DB 변경 관리도 EF Core Migration과 Oracle 전용 SQL Script Migration을 나눠서 운영성 있게 구성했고, ASP.NET Core API와 WinForms 클라이언트로 전체 흐름을 직접 검증할 수 있게 만들었습니다.

## 1분 설명

이 프로젝트는 단순 CRUD 데모보다는 Oracle 실무 경험을 설명하기 위한 샘플입니다. 상품 조회는 EF Core DbContext를 이용해 비교적 단순한 read 흐름으로 구현했고, 주문 생성과 주문 취소는 Oracle `pkg_order` package에서 처리하도록 분리했습니다. 그래서 재고 차감, 재고 복구, 이력 적재, 오류 로그 같은 쓰기 트랜잭션은 DB 안에서 일관되게 처리하고, API는 요청/응답, 예외 표준화, Swagger, Health Check 같은 서버 역할에 집중하도록 했습니다. 또한 EF Core Migration과 별도로 SQL Script Migration + DbMigrator 구조를 두어 Oracle package, seed, Oracle 전용 DDL을 따로 관리했습니다.

## 3분 설명

이 프로젝트는 Oracle DB, ASP.NET Core API, WinForms Client를 함께 보여주는 포트폴리오용 주문/재고 관리 시스템입니다. 설계할 때 가장 중요하게 본 것은 “무엇을 EF Core로 처리하고, 무엇을 Oracle PL/SQL로 처리할 것인가”였습니다.

상품 조회는 EF Core가 더 단순하고 설명하기 쉬워서 `OrderInventory.Persistence` 에 DbContext, Entity, Configuration을 두고 `ProductRepository` 에서 `AsNoTracking()` 기반 LINQ 조회로 구현했습니다. 반면 주문 생성과 취소는 재고 검증, 재고 차감/복구, 주문 헤더/상세 생성, 이력 적재, 오류 로그 적재까지 하나의 트랜잭션 안에서 처리되어야 해서 Oracle `pkg_order.create_order`, `pkg_order.cancel_order` 로 분리했습니다. C# 쪽에서는 `OrderRepository` 가 OracleParameter, CLOB, output parameter 를 이용해 package를 호출합니다.

DB 변경 관리도 두 갈래로 나눴습니다. 일반 테이블/컬럼 구조는 EF Core Migration으로 관리하고, package, seed, Oracle 전용 변경은 `db/migrations_sql` 과 `OrderInventory.DbMigrator` 로 관리합니다. DbMigrator는 SQL script를 순서대로 실행하고, `DB_SCRIPT_MIGRATION_HISTORY` 에 checksum과 적용 이력을 남기며, package compile error도 확인합니다.

마지막으로 WinForms 클라이언트를 붙여서 Swagger뿐 아니라 실제 Windows 데스크톱 클라이언트에서도 상품 조회와 주문 생성 흐름을 검증할 수 있게 했습니다. 이 프로젝트의 포인트는 UI가 아니라 Oracle 중심 업무 처리 구조를 C# 서버와 함께 설명 가능하게 만든 점입니다.

## Oracle 중심 설명

- 주문 생성/취소 같은 핵심 쓰기 트랜잭션은 PL/SQL package로 처리했다.
- package 내부에서 재고 검증, 주문 헤더/상세 생성, 이력 적재, 오류 로그를 한 흐름으로 묶었다.
- Oracle 전용 변경은 EF Core가 아니라 SQL Script Migration으로 분리했다.
- package compile error와 migration history를 별도로 관리하도록 DbMigrator를 만들었다.

## C# 서버 중심 설명

- API는 Controller / Service / Repository 구조로 분리했다.
- 상품 조회는 EF Core 기반 read model로 처리하고, 주문 생성/취소는 Oracle stored procedure 호출로 분리했다.
- 글로벌 예외 미들웨어와 공통 응답 포맷을 두어 Oracle 예외도 표준화된 API 응답으로 반환하도록 했다.
- Health Check, Swagger, Serilog로 개발 단계 검증 편의성을 높였다.

## EF Core + PL/SQL 공존 설명

- EF Core는 일반 조회와 schema versioning 에 강점이 있다.
- PL/SQL은 Oracle 중심 트랜잭션 처리에 강점이 있다.
- 이 프로젝트는 두 기술을 경쟁시키지 않고 역할을 나눴다.
- 조회는 EF Core, 핵심 쓰기는 PL/SQL package, 변경 관리는 EF Migration + SQL Script Migration 으로 분리했다.

## WinForms 클라이언트 설명

- WinForms는 운영용 프론트엔드가 아니라 Windows 데스크톱 클라이언트 경험과 API 검증을 보여주기 위한 용도다.
- 현재 구현은 상품 조회와 주문 생성 중심으로 단순하지만, 실제 API 흐름을 직접 확인할 수 있다.
- WPF보다 구현 복잡도가 낮아 포트폴리오 범위를 과도하게 키우지 않는 장점이 있다.

## Docker / DbMigrator 설명

- Docker Compose는 Oracle 개발 DB 재현에 집중했다.
- API와 DbMigrator는 로컬 실행으로 디버깅 편의성을 높였다.
- DbMigrator는 Oracle 전용 SQL script를 실행하고, 성공/실패 이력을 테이블에 남긴다.
- 이 구조 덕분에 baseline schema, EF Migration, SQL Script Migration 책임이 분리된다.

## 예상 질문 / 답변

### 왜 주문 로직을 EF Core가 아니라 PL/SQL로 처리했나요?

주문 생성과 취소는 재고 검증, 재고 변경, 주문 헤더/상세 생성, 이력 적재가 하나의 DB 트랜잭션처럼 처리되어야 해서 Oracle package로 두는 편이 더 자연스럽다고 판단했습니다.

### 왜 EF Core Migration만 쓰지 않았나요?

Oracle package, seed, partitioning, advanced index 같은 변경은 EF Core보다 SQL script가 더 명확하고 운영 리뷰에도 유리합니다. 그래서 일반 schema는 EF Migration, Oracle 전용 변경은 SQL Script Migration으로 나눴습니다.

### DbMigrator는 왜 별도 프로젝트로 만들었나요?

API startup 에 migration 책임을 몰아주면 실패 지점이 모호해집니다. DbMigrator를 별도 콘솔로 두면 Oracle init, EF Migration, SQL Script Migration, API runtime 문제를 분리해서 볼 수 있습니다.

### WinForms를 선택한 이유는 무엇인가요?

이 프로젝트의 중심은 Oracle과 API 구조입니다. UI 프레임워크에 과도한 복잡도를 주기보다, Windows 데스크톱 클라이언트 연동 경험을 빠르게 보여줄 수 있는 WinForms가 더 적합했습니다.

### 실무형으로 확장한다면 다음 단계는 무엇인가요?

인증/권한, 주문 조회 상세 화면, 관리자 재고 조정, audit logging, CI/CD 배포 파이프라인, 운영용 migration 검증 단계를 추가하는 방향으로 확장할 수 있습니다.
