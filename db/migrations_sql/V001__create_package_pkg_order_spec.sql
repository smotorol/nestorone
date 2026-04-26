CREATE OR REPLACE PACKAGE pkg_order AS
    c_success              CONSTANT VARCHAR2(10) := 'SUCCESS';
    c_error_stock_short    CONSTANT VARCHAR2(30) := 'ERR_STOCK_SHORTAGE';
    c_error_invalid_prod   CONSTANT VARCHAR2(30) := 'ERR_INVALID_PRODUCT';
    c_error_order_notfound CONSTANT VARCHAR2(30) := 'ERR_ORDER_NOT_FOUND';
    c_error_order_canceled CONSTANT VARCHAR2(30) := 'ERR_ALREADY_CANCELLED';

    PROCEDURE create_order (
        p_customer_name    IN  NVARCHAR2,
        p_created_by       IN  VARCHAR2,
        p_items_json       IN  CLOB,
        o_order_id         OUT NUMBER,
        o_order_no         OUT VARCHAR2,
        o_result_code      OUT VARCHAR2,
        o_result_message   OUT NVARCHAR2
    );

    PROCEDURE cancel_order (
        p_order_id         IN  NUMBER,
        p_cancel_reason    IN  NVARCHAR2,
        p_updated_by       IN  VARCHAR2,
        o_result_code      OUT VARCHAR2,
        o_result_message   OUT NVARCHAR2
    );

    PROCEDURE get_stock (
        p_product_id       IN  NUMBER,
        o_product_name     OUT NVARCHAR2,
        o_current_stock    OUT NUMBER,
        o_safety_stock     OUT NUMBER,
        o_result_code      OUT VARCHAR2,
        o_result_message   OUT NVARCHAR2
    );

    PROCEDURE log_error (
        p_module_name      IN VARCHAR2,
        p_ref_key          IN VARCHAR2,
        p_error_code       IN VARCHAR2,
        p_error_message    IN NVARCHAR2,
        p_backtrace        IN NVARCHAR2
    );
END pkg_order;
/
