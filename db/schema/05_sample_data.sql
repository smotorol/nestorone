-- Why: Seed enough data to run the requested manual scenarios immediately.

INSERT INTO categories (category_id, category_code, category_name)
VALUES (seq_categories.NEXTVAL, 'ELEC', N'전자기기');

INSERT INTO categories (category_id, category_code, category_name)
VALUES (seq_categories.NEXTVAL, 'OFFICE', N'사무용품');

INSERT INTO products (
    product_id, category_id, product_code, product_name,
    unit_price, current_stock_qty, safety_stock_qty, status_code
)
VALUES (seq_products.NEXTVAL, 1, 'P-KEYBOARD-01', N'기계식 키보드', 85000, 30, 5, 'ACTIVE');

INSERT INTO products (
    product_id, category_id, product_code, product_name,
    unit_price, current_stock_qty, safety_stock_qty, status_code
)
VALUES (seq_products.NEXTVAL, 1, 'P-MOUSE-01', N'무선 마우스', 35000, 50, 10, 'ACTIVE');

INSERT INTO products (
    product_id, category_id, product_code, product_name,
    unit_price, current_stock_qty, safety_stock_qty, status_code
)
VALUES (seq_products.NEXTVAL, 1, 'P-MONITOR-01', N'27인치 모니터', 240000, 12, 3, 'ACTIVE');

INSERT INTO products (
    product_id, category_id, product_code, product_name,
    unit_price, current_stock_qty, safety_stock_qty, status_code
)
VALUES (seq_products.NEXTVAL, 2, 'P-NOTE-01', N'A4 노트', 3000, 100, 20, 'ACTIVE');

INSERT INTO products (
    product_id, category_id, product_code, product_name,
    unit_price, current_stock_qty, safety_stock_qty, status_code
)
VALUES (seq_products.NEXTVAL, 2, 'P-PEN-01', N'볼펜 세트', 5000, 200, 30, 'ACTIVE');

INSERT INTO orders (
    order_id, order_no, customer_name, order_status, total_amount, order_date, created_by
)
VALUES (
    seq_orders.NEXTVAL, 'ORD-202604-0001', N'홍길동', 'CREATED', 70000, TO_DATE('2026-04-20 10:00:00', 'YYYY-MM-DD HH24:MI:SS'), 'seed'
);

INSERT INTO order_items (
    order_item_id, order_id, product_id, order_qty, unit_price, line_amount
)
VALUES (
    seq_order_items.NEXTVAL, 20260001, 1002, 2, 35000, 70000
);

INSERT INTO stock_history (
    stock_history_id, product_id, order_id, history_type, change_qty, before_qty, after_qty, memo, created_at, created_by
)
VALUES (
    seq_stock_history.NEXTVAL, 1002, 20260001, 'ORDER_DEDUCT', -2, 52, 50, N'샘플 주문 차감', TO_DATE('2026-04-20 10:00:05', 'YYYY-MM-DD HH24:MI:SS'), 'seed'
);

COMMIT;
