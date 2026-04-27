# 아키텍처 결정 기록

## ADR-001: Oracle SQL 조회 + PL/SQL 구조

### 결정

- 상품 조회는 Dapper 기반 SQL 조회 사용
- 핵심 주문 로직은 Oracle PL/SQL package 유지
- DB 변경 관리는 versioned SQL Script Migration + DbMigrator 로 통일

### 이유

- 주문/재고 로직은 Oracle 내부 잠금/예외/트랜잭션이 중요하다.
- 단순 조회는 SQL이 더 직접적이고 Oracle 중심 구조와도 잘 맞는다.
- package/sequence/index/partitioning 같은 Oracle 객체는 SQL script 로 관리하는 편이 더 검토가 쉽다.
