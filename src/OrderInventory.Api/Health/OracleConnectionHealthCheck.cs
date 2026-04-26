using Microsoft.Extensions.Diagnostics.HealthChecks;
using Oracle.ManagedDataAccess.Client;

namespace OrderInventory.Api.Health;

public sealed class OracleConnectionHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public OracleConnectionHealthCheck(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1 FROM dual";
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("Oracle connection is healthy.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Oracle connection failed.", ex);
        }
    }
}
