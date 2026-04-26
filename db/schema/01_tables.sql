ALTER SESSION SET NLS_DATE_FORMAT = 'YYYY-MM-DD HH24:MI:SS';

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
);

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
);

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
);

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
);

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
);

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
);

CREATE TABLE db_script_migration_history (
    script_id           NUMBER(12)            NOT NULL,
    script_name         VARCHAR2(255 CHAR)    NOT NULL,
    checksum            VARCHAR2(128 CHAR)    NOT NULL,
    applied_at          DATE                  DEFAULT SYSDATE NOT NULL,
    applied_by          VARCHAR2(50 CHAR)     DEFAULT USER NOT NULL,
    success_yn          CHAR(1)               DEFAULT 'Y' NOT NULL,
    error_message       NVARCHAR2(1000)       NULL,
    CONSTRAINT pk_db_script_mig_history PRIMARY KEY (script_id),
    CONSTRAINT uk_db_script_mig_history_name UNIQUE (script_name),
    CONSTRAINT ck_db_script_mig_history_success CHECK (success_yn IN ('Y', 'N'))
);
