# 면접 설명 정리

## 30초 설명

Oracle DB 기반 주문/재고 관리 프로젝트를 만들면서, 상품 조회는 SQL 기반 조회로 처리하고 주문 생성과 취소는 PL/SQL package로 분리했습니다. DB 변경 관리는 EF migration 대신 versioned SQL Script Migration과 DbMigrator 중심으로 정리했고, ASP.NET Core API와 WinForms 클라이언트로 전체 흐름을 직접 검증할 수 있게 만들었습니다.

## 1분 설명

이 프로젝트는 단순 CRUD 데모보다는 Oracle 실무 경험을 설명하기 위한 샘플입니다. 상품 조회는 Dapper 기반 SQL 조회로 구현했고, 주문 생성과 주문 취소는 Oracle `pkg_order` package에서 처리하도록 분리했습니다. 그래서 재고 차감, 재고 복구, 이력 적재, 오류 로그 같은 쓰기 트랜잭션은 DB 안에서 일관되게 처리하고, API는 요청/응답, 예외 표준화, Swagger, Health Check 같은 서버 역할에 집중하도록 했습니다. 또한 DB 변경 관리는 `db/migrations_sql` 과 `OrderInventory.DbMigrator` 중심으로 통일해 baseline schema, package, seed, Oracle 전용 DDL을 한 흐름으로 관리하도록 구성했습니다.

## SQL 조회 + PL/SQL 공존 설명

- SQL 조회는 단순 목록/상세 조회를 직관적으로 표현하기 좋다.
- PL/SQL은 Oracle 중심 트랜잭션 처리에 강점이 있다.
- 이 프로젝트는 두 기술을 역할별로 나눴다.
- 조회는 SQL, 핵심 쓰기는 PL/SQL package, 변경 관리는 versioned SQL Script Migration + DbMigrator로 분리했다.

## 왜 EF Core를 제거했나요?

이 프로젝트는 sequence, package, seed, partitioning 같은 Oracle 전용 요소 비중이 높아서 DB 변경 관리를 SQL 중심으로 통일하는 편이 더 자연스러웠습니다. 조회도 SQL로 맞추면 기술 스택이 더 일관되고, Oracle 중심 프로젝트라는 설명이 더 명확해집니다.
