using Oracle.ManagedDataAccess.Client;

namespace OrderInventory.Infrastructure.Persistence;

public interface IOracleConnectionFactory
{
    Task<OracleConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken);
}
