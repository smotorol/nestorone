using System.Data;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using OrderInventory.Application.Abstractions;
using OrderInventory.Application.Dtos.Orders;
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

    public async Task<CreateOrderResultDto> CreateOrderAsync(CreateOrderRequestDto request, CancellationToken cancellationToken)
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

        return new CreateOrderResultDto
        {
            OrderId = Convert.ToInt64(command.Parameters["o_order_id"].Value?.ToString()),
            OrderNo = command.Parameters["o_order_no"].Value?.ToString() ?? string.Empty,
            ResultCode = command.Parameters["o_result_code"].Value?.ToString() ?? string.Empty,
            ResultMessage = command.Parameters["o_result_message"].Value?.ToString() ?? string.Empty
        };
    }

    public async Task<CancelOrderResultDto> CancelOrderAsync(long orderId, CancelOrderRequestDto request, CancellationToken cancellationToken)
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

        return new CancelOrderResultDto
        {
            OrderId = orderId,
            ResultCode = command.Parameters["o_result_code"].Value?.ToString() ?? string.Empty,
            ResultMessage = command.Parameters["o_result_message"].Value?.ToString() ?? string.Empty
        };
    }

        private static OracleParameter CreateJsonClobParameter(IEnumerable<CreateOrderItemDto> items)
    {
        var json = JsonSerializer.Serialize(items, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return new OracleParameter("p_items_json", OracleDbType.Clob, json, ParameterDirection.Input);
    }
}
