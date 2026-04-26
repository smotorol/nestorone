BEGIN
    MERGE INTO categories c
    USING (SELECT 1 AS category_id, 'ELEC' AS category_code, N'전자기기' AS category_name FROM dual) s
       ON (c.category_id = s.category_id)
    WHEN MATCHED THEN
        UPDATE SET c.category_code = s.category_code,
                   c.category_name = s.category_name,
                   c.updated_at = SYSDATE
    WHEN NOT MATCHED THEN
        INSERT (category_id, category_code, category_name, use_yn, created_at, updated_at)
        VALUES (s.category_id, s.category_code, s.category_name, 'Y', SYSDATE, SYSDATE);

    MERGE INTO categories c
    USING (SELECT 2 AS category_id, 'OFFICE' AS category_code, N'사무용품' AS category_name FROM dual) s
       ON (c.category_id = s.category_id)
    WHEN MATCHED THEN
        UPDATE SET c.category_code = s.category_code,
                   c.category_name = s.category_name,
                   c.updated_at = SYSDATE
    WHEN NOT MATCHED THEN
        INSERT (category_id, category_code, category_name, use_yn, created_at, updated_at)
        VALUES (s.category_id, s.category_code, s.category_name, 'Y', SYSDATE, SYSDATE);

    MERGE INTO products p
    USING (SELECT 1001 AS product_id, 1 AS category_id, 'P-KEYBOARD-01' AS product_code, N'기계식 키보드' AS product_name, 85000 AS unit_price, 30 AS current_stock_qty, 5 AS safety_stock_qty, 'ACTIVE' AS status_code FROM dual) s
       ON (p.product_id = s.product_id)
    WHEN MATCHED THEN
        UPDATE SET p.category_id = s.category_id, p.product_code = s.product_code, p.product_name = s.product_name,
                   p.unit_price = s.unit_price, p.current_stock_qty = s.current_stock_qty, p.safety_stock_qty = s.safety_stock_qty,
                   p.status_code = s.status_code, p.updated_at = SYSDATE
    WHEN NOT MATCHED THEN
        INSERT (product_id, category_id, product_code, product_name, unit_price, current_stock_qty, safety_stock_qty, status_code, created_at, updated_at)
        VALUES (s.product_id, s.category_id, s.product_code, s.product_name, s.unit_price, s.current_stock_qty, s.safety_stock_qty, s.status_code, SYSDATE, SYSDATE);

    MERGE INTO products p
    USING (SELECT 1002 AS product_id, 1 AS category_id, 'P-MOUSE-01' AS product_code, N'무선 마우스' AS product_name, 35000 AS unit_price, 50 AS current_stock_qty, 10 AS safety_stock_qty, 'ACTIVE' AS status_code FROM dual) s
       ON (p.product_id = s.product_id)
    WHEN MATCHED THEN
        UPDATE SET p.category_id = s.category_id, p.product_code = s.product_code, p.product_name = s.product_name,
                   p.unit_price = s.unit_price, p.current_stock_qty = s.current_stock_qty, p.safety_stock_qty = s.safety_stock_qty,
                   p.status_code = s.status_code, p.updated_at = SYSDATE
    WHEN NOT MATCHED THEN
        INSERT (product_id, category_id, product_code, product_name, unit_price, current_stock_qty, safety_stock_qty, status_code, created_at, updated_at)
        VALUES (s.product_id, s.category_id, s.product_code, s.product_name, s.unit_price, s.current_stock_qty, s.safety_stock_qty, s.status_code, SYSDATE, SYSDATE);

    MERGE INTO products p
    USING (SELECT 1003 AS product_id, 1 AS category_id, 'P-MONITOR-01' AS product_code, N'27인치 모니터' AS product_name, 240000 AS unit_price, 12 AS current_stock_qty, 3 AS safety_stock_qty, 'ACTIVE' AS status_code FROM dual) s
       ON (p.product_id = s.product_id)
    WHEN MATCHED THEN
        UPDATE SET p.category_id = s.category_id, p.product_code = s.product_code, p.product_name = s.product_name,
                   p.unit_price = s.unit_price, p.current_stock_qty = s.current_stock_qty, p.safety_stock_qty = s.safety_stock_qty,
                   p.status_code = s.status_code, p.updated_at = SYSDATE
    WHEN NOT MATCHED THEN
        INSERT (product_id, category_id, product_code, product_name, unit_price, current_stock_qty, safety_stock_qty, status_code, created_at, updated_at)
        VALUES (s.product_id, s.category_id, s.product_code, s.product_name, s.unit_price, s.current_stock_qty, s.safety_stock_qty, s.status_code, SYSDATE, SYSDATE);

    MERGE INTO products p
    USING (SELECT 1004 AS product_id, 2 AS category_id, 'P-NOTE-01' AS product_code, N'A4 노트' AS product_name, 3000 AS unit_price, 100 AS current_stock_qty, 20 AS safety_stock_qty, 'ACTIVE' AS status_code FROM dual) s
       ON (p.product_id = s.product_id)
    WHEN MATCHED THEN
        UPDATE SET p.category_id = s.category_id, p.product_code = s.product_code, p.product_name = s.product_name,
                   p.unit_price = s.unit_price, p.current_stock_qty = s.current_stock_qty, p.safety_stock_qty = s.safety_stock_qty,
                   p.status_code = s.status_code, p.updated_at = SYSDATE
    WHEN NOT MATCHED THEN
        INSERT (product_id, category_id, product_code, product_name, unit_price, current_stock_qty, safety_stock_qty, status_code, created_at, updated_at)
        VALUES (s.product_id, s.category_id, s.product_code, s.product_name, s.unit_price, s.current_stock_qty, s.safety_stock_qty, s.status_code, SYSDATE, SYSDATE);

    MERGE INTO products p
    USING (SELECT 1005 AS product_id, 2 AS category_id, 'P-PEN-01' AS product_code, N'볼펜 세트' AS product_name, 5000 AS unit_price, 200 AS current_stock_qty, 30 AS safety_stock_qty, 'ACTIVE' AS status_code FROM dual) s
       ON (p.product_id = s.product_id)
    WHEN MATCHED THEN
        UPDATE SET p.category_id = s.category_id, p.product_code = s.product_code, p.product_name = s.product_name,
                   p.unit_price = s.unit_price, p.current_stock_qty = s.current_stock_qty, p.safety_stock_qty = s.safety_stock_qty,
                   p.status_code = s.status_code, p.updated_at = SYSDATE
    WHEN NOT MATCHED THEN
        INSERT (product_id, category_id, product_code, product_name, unit_price, current_stock_qty, safety_stock_qty, status_code, created_at, updated_at)
        VALUES (s.product_id, s.category_id, s.product_code, s.product_name, s.unit_price, s.current_stock_qty, s.safety_stock_qty, s.status_code, SYSDATE, SYSDATE);
END;
/
