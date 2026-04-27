DECLARE
    PROCEDURE create_table_if_missing(p_table_name IN VARCHAR2, p_sql IN CLOB) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(*)
          INTO v_count
          FROM user_tables
         WHERE table_name = UPPER(p_table_name);

        IF v_count = 0 THEN
            EXECUTE IMMEDIATE p_sql;
        END IF;
    END;

    PROCEDURE create_sequence_if_missing(p_sequence_name IN VARCHAR2, p_sql IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(*)
          INTO v_count
          FROM user_sequences
         WHERE sequence_name = UPPER(p_sequence_name);

        IF v_count = 0 THEN
            EXECUTE IMMEDIATE p_sql;
        END IF;
    END;

    PROCEDURE create_index_if_missing(p_index_name IN VARCHAR2, p_sql IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(*)
          INTO v_count
          FROM user_indexes
         WHERE index_name = UPPER(p_index_name);

        IF v_count = 0 THEN
            EXECUTE IMMEDIATE p_sql;
        END IF;
    END;
BEGIN
    create_table_if_missing('CATEGORIES', q'[
        CREATE TABLE categories (
            category_id      NUMBER(10)           NOT NULL,
            category_code    VARCHAR2(30 CHAR)    NOT NULL,
            category_name    NVARCHAR2(100)       NOT NULL,
            use_yn           CHAR(1)              DEFAULT 'Y' NOT NULL,
            created_at       DATE                 DEFAULT SYSDATE NOT NULL,
            updated_at       DATE                 DEFAULT SYSDATE NOT NULL,
            CONSTRAINT pk_categories PRIMARY KEY (category_id),
            CONSTRAINT uk_categories_code UNIQUE (category_code),
            CONSTRAINT ck_categories_use_yn CHECK (use_yn IN ('Y', 'N'))
        )
    ]');

    create_table_if_missing('PRODUCTS', q'[
        CREATE TABLE products (
            product_id          NUMBER(10)            NOT NULL,
            category_id         NUMBER(10)            NOT NULL,
            product_code        VARCHAR2(30 CHAR)     NOT NULL,
            product_name        NVARCHAR2(200)        NOT NULL,
            unit_price          NUMBER(12, 2)         NOT NULL,
            current_stock_qty   NUMBER(10)            DEFAULT 0 NOT NULL,
            safety_stock_qty    NUMBER(10)            DEFAULT 0 NOT NULL,
            status_code         VARCHAR2(20 CHAR)     DEFAULT 'ACTIVE' NOT NULL,
            created_at          DATE                  DEFAULT SYSDATE NOT NULL,
            updated_at          DATE                  DEFAULT SYSDATE NOT NULL,
            CONSTRAINT pk_products PRIMARY KEY (product_id),
            CONSTRAINT uk_products_code UNIQUE (product_code),
            CONSTRAINT fk_products_category FOREIGN KEY (category_id) REFERENCES categories(category_id),
            CONSTRAINT ck_products_price CHECK (unit_price >= 0),
            CONSTRAINT ck_products_stock CHECK (current_stock_qty >= 0),
            CONSTRAINT ck_products_safety_stock CHECK (safety_stock_qty >= 0),
            CONSTRAINT ck_products_status CHECK (status_code IN ('ACTIVE', 'INACTIVE'))
        )
    ]');

    create_table_if_missing('ORDERS', q'[
        CREATE TABLE orders (
            order_id            NUMBER(12)            NOT NULL,
            order_no            VARCHAR2(30 CHAR)     NOT NULL,
            customer_name       NVARCHAR2(100)        NOT NULL,
            order_status        VARCHAR2(20 CHAR)     DEFAULT 'CREATED' NOT NULL,
            total_amount        NUMBER(14, 2)         DEFAULT 0 NOT NULL,
            order_date          DATE                  DEFAULT SYSDATE NOT NULL,
            cancel_date         DATE                  NULL,
            cancel_reason       NVARCHAR2(200)        NULL,
            created_by          VARCHAR2(50 CHAR)     DEFAULT USER NOT NULL,
            created_at          DATE                  DEFAULT SYSDATE NOT NULL,
            updated_at          DATE                  DEFAULT SYSDATE NOT NULL,
            CONSTRAINT pk_orders PRIMARY KEY (order_id),
            CONSTRAINT uk_orders_order_no UNIQUE (order_no),
            CONSTRAINT ck_orders_status CHECK (order_status IN ('CREATED', 'CANCELLED')),
            CONSTRAINT ck_orders_total_amount CHECK (total_amount >= 0)
        )
    ]');

    create_table_if_missing('ORDER_ITEMS', q'[
        CREATE TABLE order_items (
            order_item_id       NUMBER(12)            NOT NULL,
            order_id            NUMBER(12)            NOT NULL,
            product_id          NUMBER(10)            NOT NULL,
            order_qty           NUMBER(10)            NOT NULL,
            unit_price          NUMBER(12, 2)         NOT NULL,
            line_amount         NUMBER(14, 2)         NOT NULL,
            created_at          DATE                  DEFAULT SYSDATE NOT NULL,
            updated_at          DATE                  DEFAULT SYSDATE NOT NULL,
            CONSTRAINT pk_order_items PRIMARY KEY (order_item_id),
            CONSTRAINT fk_order_items_order FOREIGN KEY (order_id) REFERENCES orders(order_id),
            CONSTRAINT fk_order_items_product FOREIGN KEY (product_id) REFERENCES products(product_id),
            CONSTRAINT ck_order_items_qty CHECK (order_qty > 0),
            CONSTRAINT ck_order_items_unit_price CHECK (unit_price >= 0),
            CONSTRAINT ck_order_items_line_amount CHECK (line_amount >= 0)
        )
    ]');

    create_table_if_missing('STOCK_HISTORY', q'[
        CREATE TABLE stock_history (
            stock_history_id    NUMBER(12)            NOT NULL,
            product_id          NUMBER(10)            NOT NULL,
            order_id            NUMBER(12)            NULL,
            history_type        VARCHAR2(20 CHAR)     NOT NULL,
            change_qty          NUMBER(10)            NOT NULL,
            before_qty          NUMBER(10)            NOT NULL,
            after_qty           NUMBER(10)            NOT NULL,
            memo                NVARCHAR2(200)        NULL,
            created_at          DATE                  DEFAULT SYSDATE NOT NULL,
            created_by          VARCHAR2(50 CHAR)     DEFAULT USER NOT NULL,
            CONSTRAINT pk_stock_history PRIMARY KEY (stock_history_id),
            CONSTRAINT fk_stock_history_product FOREIGN KEY (product_id) REFERENCES products(product_id),
            CONSTRAINT fk_stock_history_order FOREIGN KEY (order_id) REFERENCES orders(order_id),
            CONSTRAINT ck_stock_history_type CHECK (history_type IN ('ORDER_DEDUCT', 'ORDER_CANCEL_RESTORE', 'MANUAL_ADJUST')),
            CONSTRAINT ck_stock_history_before CHECK (before_qty >= 0),
            CONSTRAINT ck_stock_history_after CHECK (after_qty >= 0)
        )
    ]');

    create_table_if_missing('ERROR_LOG', q'[
        CREATE TABLE error_log (
            error_log_id        NUMBER(12)            NOT NULL,
            module_name         VARCHAR2(100 CHAR)    NOT NULL,
            ref_key             VARCHAR2(100 CHAR)    NULL,
            error_code          VARCHAR2(50 CHAR)     NOT NULL,
            error_message       NVARCHAR2(1000)       NOT NULL,
            backtrace           NVARCHAR2(2000)       NULL,
            created_at          DATE                  DEFAULT SYSDATE NOT NULL,
            created_by          VARCHAR2(50 CHAR)     DEFAULT USER NOT NULL,
            CONSTRAINT pk_error_log PRIMARY KEY (error_log_id)
        )
    ]');

    create_sequence_if_missing('SEQ_CATEGORIES', 'CREATE SEQUENCE seq_categories START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE');
    create_sequence_if_missing('SEQ_PRODUCTS', 'CREATE SEQUENCE seq_products START WITH 1001 INCREMENT BY 1 NOCACHE NOCYCLE');
    create_sequence_if_missing('SEQ_ORDERS', 'CREATE SEQUENCE seq_orders START WITH 20260001 INCREMENT BY 1 NOCACHE NOCYCLE');
    create_sequence_if_missing('SEQ_ORDER_ITEMS', 'CREATE SEQUENCE seq_order_items START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE');
    create_sequence_if_missing('SEQ_STOCK_HISTORY', 'CREATE SEQUENCE seq_stock_history START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE');
    create_sequence_if_missing('SEQ_ERROR_LOG', 'CREATE SEQUENCE seq_error_log START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE');

    create_index_if_missing('IDX_PRODUCTS_STATUS_CATEGORY', 'CREATE INDEX idx_products_status_category ON products (status_code, category_id)');
    create_index_if_missing('IDX_ORDERS_ORDER_DATE_STATUS', 'CREATE INDEX idx_orders_order_date_status ON orders (order_date, order_status)');
    create_index_if_missing('IDX_ORDER_ITEMS_ORDER_ID', 'CREATE INDEX idx_order_items_order_id ON order_items (order_id)');
    create_index_if_missing('IDX_ORDER_ITEMS_PRODUCT_ID', 'CREATE INDEX idx_order_items_product_id ON order_items (product_id)');
    create_index_if_missing('IDX_STOCK_HISTORY_PRODUCT_DATE', 'CREATE INDEX idx_stock_history_product_date ON stock_history (product_id, created_at)');
    create_index_if_missing('IDX_ERROR_LOG_MODULE_DATE', 'CREATE INDEX idx_error_log_module_date ON error_log (module_name, created_at)');
END;
/
