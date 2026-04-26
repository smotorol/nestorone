# 성능 튜닝 메모

이 문서는 Oracle을 단순 저장소처럼 쓰지 않았다는 점을 설명하기 위한 성능 메모다.

## 1. 예상 병목 시나리오

- 주문 목록을 `ORDER_DATE` 범위로 조회하면서 상태 필터를 같이 줄 때 `ORDERS` 풀스캔 가능
- 주문 상세 조회 시 `ORDER_ITEMS.ORDER_ID` 인덱스가 없으면 헤더 1건당 상세 조회가 느려질 수 있음
- 재고 이력 화면에서 상품별 기간 조회 시 `STOCK_HISTORY`가 빠르게 커져 응답이 느려질 수 있음
- 주문 생성 시 상품별 재고 확인과 차감에서 동시성 잠금 경합이 생길 수 있음

## 2. 실행계획 확인 포인트

```sql
EXPLAIN PLAN FOR
SELECT o.order_id, o.order_no, o.order_status, o.order_date, o.total_amount
  FROM orders o
 WHERE o.order_date >= DATE '2026-04-01'
   AND o.order_date < DATE '2026-05-01'
   AND o.order_status = 'CREATED';

SELECT * FROM TABLE(DBMS_XPLAN.DISPLAY);
```

Check:

- `TABLE ACCESS FULL` 여부
- `INDEX RANGE SCAN` 전환 여부
- 파티션 테이블 사용 시 `PARTITION RANGE SINGLE` 또는 `PARTITION RANGE ITERATOR` 표시 여부

## 3. 인덱스 적용 전후 비교 관점

- 조회 건수는 같아도 `consistent gets` 감소 여부 확인
- 범위 조회에서 `idx_orders_order_date_status` 사용 여부 확인
- 주문 상세 조회에서 `idx_order_items_order_id` 사용 여부 확인
- 상품별 이력 조회에서 `idx_stock_history_product_date` 사용 여부 확인

## 4. 전체 스캔 방지 포인트

- `TRUNC(order_date)` 대신 `>=` 와 `<` 범위 조건 사용
- `WHERE TO_CHAR(order_date, 'YYYYMM') = '202604'` 같은 함수 사용 지양
- `LIKE '%키보드%'` 는 인덱스 효율이 떨어지므로 기본 시연은 접두 검색이나 카테고리/상태 조건 위주로 설명
- 상태 코드처럼 선택도가 낮은 컬럼은 단독보다 날짜 컬럼과 복합 인덱스로 설계

## 5. 주문 생성 튜닝 포인트

- 재고 차감 전 `SELECT ... FOR UPDATE` 로 대상 행만 잠금
- 한 주문 내 여러 상품 처리 시 반복 조회 최소화
- 주문/재고 이력 쓰기 작업은 같은 트랜잭션으로 묶고, 오류 로그만 `AUTONOMOUS_TRANSACTION` 사용
