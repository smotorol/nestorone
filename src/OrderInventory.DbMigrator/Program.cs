using Oracle.ManagedDataAccess.Client;
using OrderInventory.DbMigrator.Services;
using OrderInventory.Migrations;

var connectionString = Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING")
    ?? throw new InvalidOperationException("ORACLE_CONNECTION_STRING is required.");

var scriptsPath = ResolveScriptsPath();
var showSummary = !string.Equals(Environment.GetEnvironmentVariable("DBMIGRATOR_SHOW_SUMMARY"), "false", StringComparison.OrdinalIgnoreCase);

Console.WriteLine($"[DbMigrator] SQL scripts path: {scriptsPath}");
Console.WriteLine("[DbMigrator] Applying EF Core migrations...");
await MigrationRunner.ApplyMigrationsAsync(connectionString);

Console.WriteLine("[DbMigrator] Applying SQL script migrations...");
var runner = new SqlScriptMigrationRunner(connectionString, scriptsPath);
await runner.RunAsync();

Console.WriteLine("[DbMigrator] Migration completed successfully.");

if (showSummary)
{
    await PrintSummaryAsync(connectionString);
}

static string ResolveScriptsPath()
{
    var envPath = Environment.GetEnvironmentVariable("SQL_MIGRATIONS_PATH");
    if (!string.IsNullOrWhiteSpace(envPath) && Directory.Exists(envPath))
    {
        return envPath;
    }

    var candidates = new List<string>
    {
        Path.Combine(Directory.GetCurrentDirectory(), "db", "migrations_sql"),
        Path.Combine(AppContext.BaseDirectory, "db", "migrations_sql")
    };

    var current = new DirectoryInfo(AppContext.BaseDirectory);
    while (current is not null)
    {
        candidates.Add(Path.Combine(current.FullName, "db", "migrations_sql"));
        current = current.Parent;
    }

    foreach (var candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
    {
        if (Directory.Exists(candidate))
        {
            return candidate;
        }
    }

    throw new DirectoryNotFoundException(
        "SQL migrations directory was not found. Set SQL_MIGRATIONS_PATH or ensure db/migrations_sql exists relative to the workspace.");
}

static async Task PrintSummaryAsync(string connectionString)
{
    Console.WriteLine("[DbMigrator] Summary");

    await using var connection = new OracleConnection(connectionString);
    await connection.OpenAsync();

    await PrintPackageStatusAsync(connection);
    await PrintMigrationHistoryAsync(connection);
    await PrintProductsAsync(connection);
}

static async Task PrintPackageStatusAsync(OracleConnection connection)
{
    const string sql = @"
SELECT object_name, object_type, status
  FROM user_objects
 WHERE object_name = 'PKG_ORDER'
 ORDER BY object_type";

    Console.WriteLine("  - PKG_ORDER status");
    await using var command = connection.CreateCommand();
    command.CommandText = sql;
    await using var reader = await command.ExecuteReaderAsync();

    var hasRows = false;
    while (await reader.ReadAsync())
    {
        hasRows = true;
        Console.WriteLine($"    {reader.GetString(0)} | {reader.GetString(1)} | {reader.GetString(2)}");
    }

    if (!hasRows)
    {
        Console.WriteLine("    (no rows)");
    }
}

static async Task PrintMigrationHistoryAsync(OracleConnection connection)
{
    const string sql = @"
SELECT script_name, success_yn, TO_CHAR(applied_at, 'YYYY-MM-DD HH24:MI:SS') AS applied_at
  FROM (
        SELECT script_name, success_yn, applied_at
          FROM db_script_migration_history
         ORDER BY applied_at DESC, script_id DESC
       )
 WHERE ROWNUM <= 5";

    Console.WriteLine("  - Recent script history");
    await using var command = connection.CreateCommand();
    command.CommandText = sql;
    await using var reader = await command.ExecuteReaderAsync();

    var hasRows = false;
    while (await reader.ReadAsync())
    {
        hasRows = true;
        Console.WriteLine($"    {reader.GetString(0)} | {reader.GetString(1)} | {reader.GetString(2)}");
    }

    if (!hasRows)
    {
        Console.WriteLine("    (no rows)");
    }
}

static async Task PrintProductsAsync(OracleConnection connection)
{
    const string sql = @"
SELECT product_id, product_name, current_stock_qty
  FROM (
        SELECT product_id, product_name, current_stock_qty
          FROM products
         ORDER BY product_id
       )
 WHERE ROWNUM <= 5";

    Console.WriteLine("  - Sample products");
    await using var command = connection.CreateCommand();
    command.CommandText = sql;
    await using var reader = await command.ExecuteReaderAsync();

    var hasRows = false;
    while (await reader.ReadAsync())
    {
        hasRows = true;
        Console.WriteLine($"    {reader.GetDecimal(0)} | {reader.GetString(1)} | stock={reader.GetDecimal(2)}");
    }

    if (!hasRows)
    {
        Console.WriteLine("    (no rows)");
    }
}
