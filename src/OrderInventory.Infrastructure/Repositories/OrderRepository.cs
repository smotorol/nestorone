using Dapper;
using System.Data;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using OrderInventory.Application.Abstractions;
using OrderInventory.Contracts.Orders;
using OrderInventory.Infrastructure.Persistence;

namespace OrderInventory.Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly IOracleConnectionFactory _connectionFactory;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(IOracleConnectionFactory connectionFactory, ILogger<OrderRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<OrderSummary>> GetOrdersAsync(string? keyword, string? status, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT o.order_id      AS OrderId,
       o.order_no      AS OrderNo,
       o.customer_name AS CustomerName,
       o.order_status  AS OrderStatus,
       o.total_amount  AS TotalAmount,
       CAST(o.order_date AS TIMESTAMP) AS OrderDate,
       CAST(o.cancel_date AS TIMESTAMP) AS CancelDate,
       o.cancel_reason AS CancelReason
  FROM orders o
 WHERE (:keyword IS NULL
        OR o.order_no LIKE '%' || :keyword || '%'
        OR o.customer_name LIKE '%' || :keyword || '%')
   AND (:status IS NULL OR o.order_status = :status)
 ORDER BY o.order_id DESC";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { keyword, status }, cancellationToken: cancellationToken);
        var items = await connection.QueryAsync<OrderSummary>(command);
        return items.AsList();
    }

    public async Task<OrderDetail?> GetOrderByIdAsync(long orderId, CancellationToken cancellationToken)
    {
        const string headerSql = @"
SELECT o.order_id      AS OrderId,
       o.order_no      AS OrderNo,
       o.customer_name AS CustomerName,
       o.order_status  AS OrderStatus,
       o.total_amount  AS TotalAmount,
       CAST(o.order_date AS TIMESTAMP) AS OrderDate,
       CAST(o.cancel_date AS TIMESTAMP) AS CancelDate,
       o.cancel_reason AS CancelReason
  FROM orders o
 WHERE o.order_id = :orderId";

        const string itemsSql = @"
SELECT oi.order_item_id AS OrderItemId,
       oi.product_id    AS ProductId,
       p.product_code   AS ProductCode,
       p.product_name   AS ProductName,
       oi.order_qty     AS OrderQty,
       oi.unit_price    AS UnitPrice,
       oi.line_amount   AS LineAmount
  FROM order_items oi
  JOIN products p
    ON p.product_id = oi.product_id
 WHERE oi.order_id = :orderId
 ORDER BY oi.order_item_id";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var headerCommand = new CommandDefinition(headerSql, new { orderId }, cancellationToken: cancellationToken);
        var header = await connection.QuerySingleOrDefaultAsync<OrderDetail>(headerCommand);
        if (header is null)
        {
            return null;
        }

        var itemsCommand = new CommandDefinition(itemsSql, new { orderId }, cancellationToken: cancellationToken);
        var items = await connection.QueryAsync<OrderItemDetail>(itemsCommand);

        return new OrderDetail
        {
            OrderId = header.OrderId,
            OrderNo = header.OrderNo,
            CustomerName = header.CustomerName,
            OrderStatus = header.OrderStatus,
            TotalAmount = header.TotalAmount,
            OrderDate = header.OrderDate,
            CancelDate = header.CancelDate,
            CancelReason = header.CancelReason,
            Items = items.ToList()
        };
    }

    public async Task<CreateOrderResult> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();

        command.BindByName = true;
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "pkg_order.create_order";

        command.Parameters.Add(new OracleParameter("p_customer_name", OracleDbType.NVarchar2, request.CustomerName, ParameterDirection.Input));
        command.Parameters.Add(new OracleParameter("p_created_by", OracleDbType.Varchar2, request.CreatedBy, ParameterDirection.Input));
        command.Parameters.Add(CreateJsonClobParameter(request.Items));
        command.Parameters.Add(new OracleParameter("o_order_id", OracleDbType.Int64, ParameterDirection.Output));
        command.Parameters.Add(new OracleParameter("o_order_no", OracleDbType.Varchar2, 30) { Direction = ParameterDirection.Output });
        command.Parameters.Add(new OracleParameter("o_result_code", OracleDbType.Varchar2, 30) { Direction = ParameterDirection.Output });
        command.Parameters.Add(new OracleParameter("o_result_message", OracleDbType.NVarchar2, 500) { Direction = ParameterDirection.Output });

        _logger.LogInformation("Calling pkg_order.create_order for customer {CustomerName}", request.CustomerName);
        await command.ExecuteNonQueryAsync(cancellationToken);

        return new CreateOrderResult
        {
            OrderId = Convert.ToInt64(command.Parameters["o_order_id"].Value?.ToString()),
            OrderNo = command.Parameters["o_order_no"].Value?.ToString() ?? string.Empty,
            ResultCode = command.Parameters["o_result_code"].Value?.ToString() ?? string.Empty,
            ResultMessage = command.Parameters["o_result_message"].Value?.ToString() ?? string.Empty
        };
    }

    public async Task<CancelOrderResult> CancelOrderAsync(long orderId, CancelOrderRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();

        command.BindByName = true;
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "pkg_order.cancel_order";

        command.Parameters.Add(new OracleParameter("p_order_id", OracleDbType.Int64, orderId, ParameterDirection.Input));
        command.Parameters.Add(new OracleParameter("p_cancel_reason", OracleDbType.NVarchar2, request.CancelReason, ParameterDirection.Input));
        command.Parameters.Add(new OracleParameter("p_updated_by", OracleDbType.Varchar2, request.UpdatedBy, ParameterDirection.Input));
        command.Parameters.Add(new OracleParameter("o_result_code", OracleDbType.Varchar2, 30) { Direction = ParameterDirection.Output });
        command.Parameters.Add(new OracleParameter("o_result_message", OracleDbType.NVarchar2, 500) { Direction = ParameterDirection.Output });

        _logger.LogInformation("Calling pkg_order.cancel_order for orderId {OrderId}", orderId);
        await command.ExecuteNonQueryAsync(cancellationToken);

        return new CancelOrderResult
        {
            OrderId = orderId,
            ResultCode = command.Parameters["o_result_code"].Value?.ToString() ?? string.Empty,
            ResultMessage = command.Parameters["o_result_message"].Value?.ToString() ?? string.Empty
        };
    }

    private static OracleParameter CreateJsonClobParameter(IEnumerable<CreateOrderItem> items)
    {
        var json = JsonSerializer.Serialize(items, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return new OracleParameter("p_items_json", OracleDbType.Clob, json, ParameterDirection.Input);
    }
}
