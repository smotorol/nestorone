# 프로젝트 설계 초안

## 1. 프로젝트 개요

Why: 범위를 먼저 줄여야 실제 구현과 설명이 동시에 가능하다.

- 프로젝트명: Order & Inventory Mini System
- 목표: Oracle DB 실무 요소와 C# 서버/클라이언트 구조를 함께 보여주는 최소 완성형 포트폴리오
- 최소 범위
- 상품 조회
- 주문 생성
- 주문 취소
- 재고 차감/복구
- 주문/재고 이력 기록
- 오류 로그 기록

제외 범위:

- 회원관리
- 결제
- 대시보드 통계
- 배치/메시지 큐/외부 캐시

## 2. 전체 디렉터리 구조

```text
/db
  /schema
    01_tables.sql
    02_sequences.sql
    03_indexes.sql
    04_partitioning.sql
    05_sample_data.sql
  /plsql
    pkg_order_spec.sql
    pkg_order_body.sql
  /backup
    readme_backup_recovery.md
/src
  /OrderInventory.Api
  /OrderInventory.Application
  /OrderInventory.Domain
  /OrderInventory.Infrastructure
  /OrderInventory.Client.WinForms
/docs
  api_spec.md
  performance_tuning.md
  resume_points.md
/README.md
```

## 3. DB 설계

Why: 상품 마스터 + 주문 헤더/상세 + 재고이력 + 오류로그만으로도 Oracle 실무 포인트를 충분히 보여줄 수 있다.

### 3.1 테이블 요약

- `CATEGORIES`: 상품 분류
- `PRODUCTS`: 상품과 현재 재고
- `ORDERS`: 주문 헤더
- `ORDER_ITEMS`: 주문 상세
- `STOCK_HISTORY`: 재고 차감/복구 이력
- `ERROR_LOG`: 예외 로그

### 3.2 생성/수정일시 정책

- 모든 핵심 테이블에 `CREATED_AT`, `UPDATED_AT` 기본 컬럼 유지
- 기본값은 `SYSDATE`
- 변경 프로시저에서 `UPDATED_AT = SYSDATE` 갱신

### 3.3 예외 처리 전략

- 재고 부족: `-20001`
- 잘못된 상품 ID: `-20002`
- 주문 없음: `-20003`
- 이미 취소된 주문: `-20004`
- 오류 로그는 `AUTONOMOUS_TRANSACTION` 으로 별도 커밋

## 4. 시퀀스 설계

Why: 작은 프로젝트에서는 시퀀스 기반 PK가 가장 설명하기 쉽고 안정적이다.

- `SEQ_PRODUCTS`
- `SEQ_ORDERS`
- `SEQ_ORDER_ITEMS`
- `SEQ_STOCK_HISTORY`
- `SEQ_ERROR_LOG`

주문번호는 별도 문자열 규칙으로 생성:

```text
ORD-YYYYMM-####
```

## 5. 인덱스 설계

Why: 조회 패턴과 조인 경로를 기준으로 최소 3개 이상만 명확하게 제시한다.

1. `IDX_ORDERS_ORDER_DATE_STATUS`
- 목적: 주문일자 범위 + 상태 조회 최적화
- 사용 화면: 주문 조회 화면, 관리자 조회 화면

2. `IDX_ORDER_ITEMS_ORDER_ID`
- 목적: 주문 상세 조회 시 `ORDER_ID` 조인 비용 절감
- 사용 화면: 주문 상세 팝업

3. `IDX_STOCK_HISTORY_PRODUCT_DATE`
- 목적: 상품별 이력 조회 최적화
- 사용 화면: 관리자 재고 이력

4. `IDX_PRODUCTS_STATUS_CATEGORY`
- 목적: 활성 상품 목록 + 카테고리 필터 조회 지원

5. `IDX_ERROR_LOG_MODULE_DATE`
- 목적: 오류 로그를 최근 순/모듈 기준으로 빠르게 확인

## 6. 파티셔닝 설계

Why: 주문 테이블은 날짜 기반 조회와 보관 주기를 설명하기 좋다.

- 대상: `ORDERS`
- 기준: `ORDER_DATE` 월별 RANGE 파티셔닝
- 이유: 월 단위 조회 성능 설명, 오래된 파티션 관리, 파티션 프루닝 설명 가능

## 7. 샘플 데이터

Why: 수동 테스트 시나리오를 바로 돌릴 수 있어야 한다.

- 카테고리 2개
- 상품 5개 이상
- 주문 1건
- 주문 이력 1건

## 8. PL/SQL 패키지 설계

Why: 핵심 쓰기 로직을 DB로 모아 Oracle 실무 느낌을 살린다.

### 포함 기능

- 주문 생성
- 주문 취소
- 재고 조회
- 오류 로그 기록

### 주문 생성 흐름

1. 상품/재고 확인
2. 주문 헤더 생성
3. 주문 상세 생성
4. 재고 차감
5. 이력 기록
6. 실패 시 예외 로그 적재 후 롤백

### 주문 취소 흐름

1. 주문 존재 확인
2. 이미 취소 여부 확인
3. 상세 기준 재고 복구
4. 이력 기록
5. 상태 변경
6. 실패 시 예외 로그 적재 후 롤백

## 9. C# 서버 설계

Why: 서버는 API/예외 처리/DB 호출에 집중하고 비즈니스 핵심은 Oracle 패키지에 위임한다.

### 프로젝트 역할

- `Api`: Controller, Middleware, 응답 포맷
- `Application`: Use case, DTO, 인터페이스
- `Domain`: 엔티티, 상태 enum
- `Infrastructure`: Oracle 연결, Repository, 프로시저 실행

### Oracle 호출 예시

```csharp
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Text.Json;

public sealed class CreateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public sealed class CreateOrderItemRequest
{
    public long ProductId { get; set; }
    public int OrderQty { get; set; }
}

public sealed class CreateOrderResult
{
    public long OrderId { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public string ResultCode { get; set; } = string.Empty;
    public string ResultMessage { get; set; } = string.Empty;
}

public sealed class OrderProcedureExecutor
{
    private readonly string _connectionString;

    public OrderProcedureExecutor(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<CreateOrderResult> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        await using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "pkg_order.create_order";

        command.Parameters.Add("p_customer_name", OracleDbType.NVarchar2).Value = request.CustomerName;
        command.Parameters.Add("p_created_by", OracleDbType.Varchar2).Value = request.CreatedBy;
        command.Parameters.Add("p_items_json", OracleDbType.Clob).Value = JsonSerializer.Serialize(request.Items);
        command.Parameters.Add("o_order_id", OracleDbType.Int64).Direction = ParameterDirection.Output;
        command.Parameters.Add("o_order_no", OracleDbType.Varchar2, 30).Direction = ParameterDirection.Output;
        command.Parameters.Add("o_result_code", OracleDbType.Varchar2, 30).Direction = ParameterDirection.Output;
        command.Parameters.Add("o_result_message", OracleDbType.NVarchar2, 500).Direction = ParameterDirection.Output;

        await command.ExecuteNonQueryAsync(cancellationToken);

        return new CreateOrderResult
        {
            OrderId = Convert.ToInt64(command.Parameters["o_order_id"].Value.ToString()),
            OrderNo = command.Parameters["o_order_no"].Value.ToString() ?? string.Empty,
            ResultCode = command.Parameters["o_result_code"].Value.ToString() ?? string.Empty,
            ResultMessage = command.Parameters["o_result_message"].Value.ToString() ?? string.Empty
        };
    }
}
```

## 10. API 명세

- `GET /api/products`
- `GET /api/products/{productId}`
- `GET /api/stocks/{productId}`
- `POST /api/orders`
- `GET /api/orders/{orderId}`
- `GET /api/orders?fromDate&toDate`
- `POST /api/orders/{orderId}/cancel`

## 11. WinForms 클라이언트 설계

### 상품 목록 화면

- `DataGridView dgvProducts`
- `TextBox txtKeyword`
- `ComboBox cboCategory`
- `Button btnSearch`
- `Button btnCreateOrder`

### 주문 등록 화면

- `TextBox txtCustomerName`
- `DataGridView dgvOrderItems`
- `Button btnSubmitOrder`

### 주문 조회/취소 화면

- `DateTimePicker dtpFrom`
- `DateTimePicker dtpTo`
- `DataGridView dgvOrders`
- `Button btnCancelOrder`

### 관리자 재고/로그 화면

- `TabControl`
- `DataGridView dgvStockHistory`
- `DataGridView dgvErrorLog`

## 12. 성능 튜닝 포인트

- 예상 병목: 주문 기간 조회, 주문 상세 조회, 재고 이력 조회
- 실행계획 확인: `EXPLAIN PLAN`, `DBMS_XPLAN.DISPLAY`
- Full Scan 방지: 함수 적용 없는 날짜 범위 조건 사용
- 인덱스 비교: `consistent gets`, `INDEX RANGE SCAN` 확인

## 13. 백업/복구 시나리오

1. 샘플 주문 생성
2. `expdp` 로 export
3. 일부 데이터 삭제
4. `impdp` 로 복구
5. 복구 결과 검증

## 14. README 초안

루트 `README.md` 참고.

## 15. 이력서/면접용 정리

### 포트폴리오 핵심 문장 5개

1. Oracle PL/SQL 패키지 중심으로 주문 생성/취소와 재고 반영 로직을 구현한 미니 주문/재고 시스템 설계
2. ASP.NET Core Web API와 Oracle.ManagedDataAccess를 사용한 Oracle 프로시저 기반 API 계층 설계
3. WinForms 클라이언트에서 상품 조회, 주문 등록, 주문 취소, 관리자 로그 조회 흐름 구성
4. 주문일자 기반 복합 인덱스와 월별 RANGE 파티셔닝으로 Oracle 성능 최적화 포인트 정리
5. Data Pump 기반 export/import 복구 시나리오까지 포함한 Oracle 실무형 포트폴리오 초안

### 1주 / 2주 / 4주 일정표

#### 1주

- DB/PLSQL 초안 작성
- 조회/주문/취소 API 구현
- 기본 WinForms 화면 구현

#### 2주

- 관리자 화면, 공통 예외 처리, 문서 보강
- 실행계획 및 인덱스 비교 정리

#### 4주

- 파티셔닝 시연, 백업/복구 실습, 포트폴리오 마감

### 면접 1분 요약

이 프로젝트는 Oracle DB와 C# 기반 서버/클라이언트 경험을 짧은 시간 안에 보여주기 위해 만든 최소 실전형 주문/재고 시스템입니다. 단순 CRUD 대신 주문 생성과 취소, 재고 차감과 복구, 오류 로그 적재 같은 업무 핵심 로직을 Oracle PL/SQL 패키지에 넣었고, ASP.NET Core Web API는 이를 호출하는 얇은 서비스 계층으로 설계했습니다. WinForms에서는 상품 조회, 주문 등록, 주문 취소, 관리자 이력 조회 흐름을 구성했고, 여기에 인덱스, 월별 파티셔닝, Data Pump 복구 시나리오까지 포함해 Oracle 실무 포인트를 설명할 수 있게 만들었습니다.
