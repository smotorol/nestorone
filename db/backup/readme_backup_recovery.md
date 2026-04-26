# Backup / Recovery Practice

이 문서는 로컬 Oracle Free 환경에서 과장되지 않은 복구 시나리오를 설명하기 위한 메모다.

## 1. Data Pump Export

```bash
expdp system/oracle schemas=ORDERINV directory=DATA_PUMP_DIR dumpfile=orderinv_20260423.dmp logfile=orderinv_20260423_exp.log
```

Single table export example:

```bash
expdp system/oracle tables=ORDERINV.ORDERS,ORDERINV.ORDER_ITEMS,ORDERINV.STOCK_HISTORY directory=DATA_PUMP_DIR dumpfile=orderinv_orders_only.dmp logfile=orderinv_orders_only_exp.log
```

## 2. Delete Test Data

```sql
DELETE FROM stock_history WHERE order_id = 20260002;
DELETE FROM order_items WHERE order_id = 20260002;
DELETE FROM orders WHERE order_id = 20260002;
COMMIT;
```

## 3. Import Recovery

```bash
impdp system/oracle schemas=ORDERINV directory=DATA_PUMP_DIR dumpfile=orderinv_20260423.dmp logfile=orderinv_20260423_imp.log table_exists_action=replace
```

```bash
impdp system/oracle tables=ORDERINV.ORDERS,ORDERINV.ORDER_ITEMS,ORDERINV.STOCK_HISTORY directory=DATA_PUMP_DIR dumpfile=orderinv_orders_only.dmp logfile=orderinv_orders_only_imp.log table_exists_action=replace
```

## 4. Recovery Demo Scenario

1. 샘플 주문을 1건 생성한다.
2. `ORDERS`, `ORDER_ITEMS`, `STOCK_HISTORY` 데이터를 확인한다.
3. Data Pump로 export 한다.
4. 해당 주문 데이터를 수동 삭제한다.
5. import 후 주문/상세/재고이력이 복구되었는지 확인한다.

## 5. Verification SQL

```sql
SELECT * FROM orders WHERE order_id = 20260002;
SELECT * FROM order_items WHERE order_id = 20260002;
SELECT * FROM stock_history WHERE order_id = 20260002 ORDER BY stock_history_id;
```

## 6. RMAN Note

- RMAN은 인스턴스/데이터파일 단위 백업과 복구를 위한 도구다.
- 이번 미니 프로젝트에서는 Data Pump로 스키마 레벨 복구를 실습하고, RMAN은 "물리 백업/복구는 RMAN, 논리 백업/복구는 Data Pump" 수준으로 설명하면 충분하다.
