-- Why: Monthly order history is a realistic reporting/retention target and easy to explain in an interview.
-- Apply this script instead of the non-partitioned ORDERS table when you want to demonstrate partitioning.

CREATE TABLE orders_partitioned (
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
    CONSTRAINT pk_orders_partitioned PRIMARY KEY (order_id),
    CONSTRAINT uk_orders_partitioned_no UNIQUE (order_no),
    CONSTRAINT ck_orders_partitioned_status CHECK (order_status IN ('CREATED', 'CANCELLED'))
)
PARTITION BY RANGE (order_date)
(
    PARTITION p_2026_01 VALUES LESS THAN (DATE '2026-02-01'),
    PARTITION p_2026_02 VALUES LESS THAN (DATE '2026-03-01'),
    PARTITION p_2026_03 VALUES LESS THAN (DATE '2026-04-01'),
    PARTITION p_2026_04 VALUES LESS THAN (DATE '2026-05-01'),
    PARTITION p_2026_05 VALUES LESS THAN (DATE '2026-06-01'),
    PARTITION p_max VALUES LESS THAN (MAXVALUE)
);

CREATE INDEX idx_orders_part_date_status
    ON orders_partitioned (order_date, order_status)
    LOCAL;
