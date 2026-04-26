CREATE OR REPLACE PACKAGE BODY pkg_order AS

    PROCEDURE log_error (
        p_module_name      IN VARCHAR2,
        p_ref_key          IN VARCHAR2,
        p_error_code       IN VARCHAR2,
        p_error_message    IN NVARCHAR2,
        p_backtrace        IN NVARCHAR2
    ) IS
        PRAGMA AUTONOMOUS_TRANSACTION;
    BEGIN
        INSERT INTO error_log (
            error_log_id, module_name, ref_key, error_code, error_message, backtrace, created_at, created_by
        ) VALUES (
            seq_error_log.NEXTVAL,
            p_module_name,
            p_ref_key,
            p_error_code,
            p_error_message,
            p_backtrace,
            SYSDATE,
            USER
        );

        COMMIT;
    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
    END log_error;

    PROCEDURE get_stock (
        p_product_id       IN  NUMBER,
        o_product_name     OUT NVARCHAR2,
        o_current_stock    OUT NUMBER,
        o_safety_stock     OUT NUMBER,
        o_result_code      OUT VARCHAR2,
        o_result_message   OUT NVARCHAR2
    ) IS
    BEGIN
        SELECT product_name, current_stock_qty, safety_stock_qty
          INTO o_product_name, o_current_stock, o_safety_stock
          FROM products
         WHERE product_id = p_product_id
           AND status_code = 'ACTIVE';

        o_result_code := c_success;
        o_result_message := N'정상 조회';
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            o_result_code := c_error_invalid_prod;
            o_result_message := N'유효하지 않은 상품입니다.';
    END get_stock;

    PROCEDURE create_order (
        p_customer_name    IN  NVARCHAR2,
        p_created_by       IN  VARCHAR2,
        p_items_json       IN  CLOB,
        o_order_id         OUT NUMBER,
        o_order_no         OUT VARCHAR2,
        o_result_code      OUT VARCHAR2,
        o_result_message   OUT NVARCHAR2
    ) IS
        v_total_amount     NUMBER(14, 2) := 0;
        v_order_date       DATE := SYSDATE;
        v_current_stock    NUMBER;
        v_unit_price       NUMBER(12, 2);
        v_before_qty       NUMBER;
        v_after_qty        NUMBER;
    BEGIN
        o_order_id := seq_orders.NEXTVAL;
        o_order_no := 'ORD-' || TO_CHAR(v_order_date, 'YYYYMM') || '-' || LPAD(MOD(o_order_id, 10000), 4, '0');

        FOR rec IN (
            SELECT jt.product_id, jt.order_qty
              FROM JSON_TABLE(
                       p_items_json,
                       '$[*]' COLUMNS (
                           product_id NUMBER PATH '$.productId',
                           order_qty  NUMBER PATH '$.orderQty'
                       )
                   ) jt
        ) LOOP
            BEGIN
                SELECT unit_price, current_stock_qty
                  INTO v_unit_price, v_current_stock
                  FROM products
                 WHERE product_id = rec.product_id
                   AND status_code = 'ACTIVE'
                 FOR UPDATE;
            EXCEPTION
                WHEN NO_DATA_FOUND THEN
                    RAISE_APPLICATION_ERROR(-20002, 'Invalid product id: ' || rec.product_id);
            END;

            IF rec.order_qty <= 0 THEN
                RAISE_APPLICATION_ERROR(-20002, 'Invalid order quantity: ' || rec.order_qty);
            END IF;

            IF v_current_stock < rec.order_qty THEN
                RAISE_APPLICATION_ERROR(-20001, 'Insufficient stock for product id: ' || rec.product_id);
            END IF;

            v_total_amount := v_total_amount + (v_unit_price * rec.order_qty);
        END LOOP;

        IF v_total_amount = 0 THEN
            RAISE_APPLICATION_ERROR(-20002, 'Order item is empty');
        END IF;

        INSERT INTO orders (
            order_id, order_no, customer_name, order_status, total_amount,
            order_date, created_by, created_at, updated_at
        ) VALUES (
            o_order_id, o_order_no, p_customer_name, 'CREATED', v_total_amount,
            v_order_date, p_created_by, SYSDATE, SYSDATE
        );

        FOR rec IN (
            SELECT jt.product_id, jt.order_qty
              FROM JSON_TABLE(
                       p_items_json,
                       '$[*]' COLUMNS (
                           product_id NUMBER PATH '$.productId',
                           order_qty  NUMBER PATH '$.orderQty'
                       )
                   ) jt
        ) LOOP
            SELECT unit_price, current_stock_qty
              INTO v_unit_price, v_before_qty
              FROM products
             WHERE product_id = rec.product_id
             FOR UPDATE;

            INSERT INTO order_items (
                order_item_id, order_id, product_id, order_qty, unit_price, line_amount, created_at, updated_at
            ) VALUES (
                seq_order_items.NEXTVAL,
                o_order_id,
                rec.product_id,
                rec.order_qty,
                v_unit_price,
                v_unit_price * rec.order_qty,
                SYSDATE,
                SYSDATE
            );

            UPDATE products
               SET current_stock_qty = current_stock_qty - rec.order_qty,
                   updated_at = SYSDATE
             WHERE product_id = rec.product_id;

            v_after_qty := v_before_qty - rec.order_qty;

            INSERT INTO stock_history (
                stock_history_id, product_id, order_id, history_type, change_qty,
                before_qty, after_qty, memo, created_at, created_by
            ) VALUES (
                seq_stock_history.NEXTVAL,
                rec.product_id,
                o_order_id,
                'ORDER_DEDUCT',
                -rec.order_qty,
                v_before_qty,
                v_after_qty,
                N'주문 생성에 따른 재고 차감',
                SYSDATE,
                p_created_by
            );
        END LOOP;

        COMMIT;
        o_result_code := c_success;
        o_result_message := N'주문 생성이 완료되었습니다.';
    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            o_result_code := TO_CHAR(SQLCODE);
            o_result_message := SUBSTR(SQLERRM, 1, 1000);

            log_error(
                p_module_name   => 'pkg_order.create_order',
                p_ref_key       => TO_CHAR(o_order_id),
                p_error_code    => TO_CHAR(SQLCODE),
                p_error_message => SUBSTR(SQLERRM, 1, 1000),
                p_backtrace     => DBMS_UTILITY.format_error_backtrace
            );

            RAISE;
    END create_order;

    PROCEDURE cancel_order (
        p_order_id         IN  NUMBER,
        p_cancel_reason    IN  NVARCHAR2,
        p_updated_by       IN  VARCHAR2,
        o_result_code      OUT VARCHAR2,
        o_result_message   OUT NVARCHAR2
    ) IS
        v_status        orders.order_status%TYPE;
        v_before_qty    NUMBER;
        v_after_qty     NUMBER;
    BEGIN
        BEGIN
            SELECT order_status
              INTO v_status
              FROM orders
             WHERE order_id = p_order_id
             FOR UPDATE;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                RAISE_APPLICATION_ERROR(-20003, 'Order not found: ' || p_order_id);
        END;

        IF v_status = 'CANCELLED' THEN
            RAISE_APPLICATION_ERROR(-20004, 'Order already cancelled: ' || p_order_id);
        END IF;

        FOR rec IN (
            SELECT oi.product_id, oi.order_qty
              FROM order_items oi
             WHERE oi.order_id = p_order_id
        ) LOOP
            SELECT current_stock_qty
              INTO v_before_qty
              FROM products
             WHERE product_id = rec.product_id
             FOR UPDATE;

            UPDATE products
               SET current_stock_qty = current_stock_qty + rec.order_qty,
                   updated_at = SYSDATE
             WHERE product_id = rec.product_id;

            v_after_qty := v_before_qty + rec.order_qty;

            INSERT INTO stock_history (
                stock_history_id, product_id, order_id, history_type, change_qty,
                before_qty, after_qty, memo, created_at, created_by
            ) VALUES (
                seq_stock_history.NEXTVAL,
                rec.product_id,
                p_order_id,
                'ORDER_CANCEL_RESTORE',
                rec.order_qty,
                v_before_qty,
                v_after_qty,
                N'주문 취소에 따른 재고 복구',
                SYSDATE,
                p_updated_by
            );
        END LOOP;

        UPDATE orders
           SET order_status = 'CANCELLED',
               cancel_date = SYSDATE,
               cancel_reason = p_cancel_reason,
               updated_at = SYSDATE
         WHERE order_id = p_order_id;

        COMMIT;
        o_result_code := c_success;
        o_result_message := N'주문이 취소되었습니다.';
    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            o_result_code := TO_CHAR(SQLCODE);
            o_result_message := SUBSTR(SQLERRM, 1, 1000);

            log_error(
                p_module_name   => 'pkg_order.cancel_order',
                p_ref_key       => TO_CHAR(p_order_id),
                p_error_code    => TO_CHAR(SQLCODE),
                p_error_message => SUBSTR(SQLERRM, 1, 1000),
                p_backtrace     => DBMS_UTILITY.format_error_backtrace
            );

            RAISE;
    END cancel_order;

END pkg_order;
/
