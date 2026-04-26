-- Why: Focus indexes on the access paths that support order creation, order lookup, and date-range reporting.

CREATE INDEX idx_products_status_category
    ON products (status_code, category_id);

CREATE INDEX idx_orders_order_date_status
    ON orders (order_date, order_status);

CREATE INDEX idx_order_items_order_id
    ON order_items (order_id);

CREATE INDEX idx_order_items_product_id
    ON order_items (product_id);

CREATE INDEX idx_stock_history_product_date
    ON stock_history (product_id, created_at);

CREATE INDEX idx_error_log_module_date
    ON error_log (module_name, created_at);
