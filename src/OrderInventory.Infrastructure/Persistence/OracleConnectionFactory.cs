using Oracle.ManagedDataAccess.Client;
using OrderInventory.Infrastructure.Options;

namespace OrderInventory.Infrastructure.Persistence;

public sealed class OracleConnectionFactory : IOracleConnectionFactory
{
    private readonly string _connectionString;

    public OracleConnectionFactory(OracleDbOptions options)
    {
        _connectionString = options.ConnectionString;
    }

    public async Task<OracleConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
