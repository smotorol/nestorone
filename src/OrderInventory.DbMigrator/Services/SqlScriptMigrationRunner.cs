using System.Security.Cryptography;
using System.Text;
using Oracle.ManagedDataAccess.Client;

namespace OrderInventory.DbMigrator.Services;

public sealed class SqlScriptMigrationRunner
{
    private const int MaxErrorMessageLength = 1000;

    private readonly string _connectionString;
    private readonly string _scriptsDirectory;

    public SqlScriptMigrationRunner(string connectionString, string scriptsDirectory)
    {
        _connectionString = connectionString;
        _scriptsDirectory = scriptsDirectory;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await EnsureHistoryTableAsync(connection, cancellationToken);

        foreach (var file in Directory.GetFiles(_scriptsDirectory, "V*.sql").OrderBy(Path.GetFileName))
        {
            var scriptName = Path.GetFileName(file);
            var scriptText = NormalizeScript(File.ReadAllText(file));
            var checksum = ComputeChecksum(scriptText);

            if (await IsAppliedAsync(connection, scriptName, checksum, cancellationToken))
            {
                Console.WriteLine($"[SKIP] {scriptName}");
                continue;
            }

            Console.WriteLine($"[APPLY] {scriptName}");
            try
            {
                await ExecuteScriptAsync(connection, scriptText, cancellationToken);
                await VerifyPackageErrorsAsync(connection, scriptName, cancellationToken);
                await SaveHistoryAsync(connection, scriptName, checksum, "Y", null, cancellationToken);
            }
            catch (Exception ex)
            {
                await SaveHistoryAsync(connection, scriptName, checksum, "N", TruncateErrorMessage(ex), cancellationToken);
                throw;
            }
        }
    }

    private static string NormalizeScript(string scriptText)
    {
        var lines = scriptText.Replace("\r\n", "\n").Split('\n').ToList();
        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1])) lines.RemoveAt(lines.Count - 1);
        if (lines.Count > 0 && lines[^1].Trim() == "/") lines.RemoveAt(lines.Count - 1);
        return string.Join(Environment.NewLine, lines).Trim();
    }

    private static string ComputeChecksum(string content)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hash);
    }

    private static string TruncateErrorMessage(Exception exception)
    {
        var message = exception.ToString();
        return message.Length <= MaxErrorMessageLength
            ? message
            : message[..MaxErrorMessageLength];
    }

    private static async Task ExecuteScriptAsync(OracleConnection connection, string scriptText, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = scriptText;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task VerifyPackageErrorsAsync(OracleConnection connection, string scriptName, CancellationToken cancellationToken)
    {
        if (!scriptName.Contains("pkg_order", StringComparison.OrdinalIgnoreCase)) return;

        const string sql = @"
SELECT name, type, line, position, text
  FROM user_errors
 WHERE name = 'PKG_ORDER'
 ORDER BY sequence";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var errors = new List<string>();
        while (await reader.ReadAsync(cancellationToken))
        {
            errors.Add($"{reader.GetString(0)} {reader.GetString(1)} {reader.GetDecimal(2)}:{reader.GetDecimal(3)} {reader.GetString(4)}");
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException("PKG_ORDER compile errors detected: " + string.Join(" | ", errors));
        }
    }

    private static async Task<bool> IsAppliedAsync(OracleConnection connection, string scriptName, string checksum, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT COUNT(*)
  FROM db_script_migration_history
 WHERE script_name = :script_name
   AND checksum = :checksum
   AND success_yn = 'Y'";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(new OracleParameter("script_name", scriptName));
        command.Parameters.Add(new OracleParameter("checksum", checksum));
        var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        return count > 0;
    }

    private static async Task EnsureHistoryTableAsync(OracleConnection connection, CancellationToken cancellationToken)
    {
        const string plsql = @"
DECLARE
    v_count NUMBER := 0;
BEGIN
    SELECT COUNT(*) INTO v_count
      FROM user_tables
     WHERE table_name = 'DB_SCRIPT_MIGRATION_HISTORY';

    IF v_count = 0 THEN
        EXECUTE IMMEDIATE 'CREATE TABLE db_script_migration_history (
            script_id NUMBER(12) NOT NULL,
            script_name VARCHAR2(255 CHAR) NOT NULL,
            checksum VARCHAR2(128 CHAR) NOT NULL,
            applied_at DATE DEFAULT SYSDATE NOT NULL,
            applied_by VARCHAR2(50 CHAR) DEFAULT USER NOT NULL,
            success_yn CHAR(1) DEFAULT ''Y'' NOT NULL,
            error_message NVARCHAR2(1000) NULL,
            CONSTRAINT pk_db_script_mig_history PRIMARY KEY (script_id),
            CONSTRAINT uk_db_script_mig_history_name UNIQUE (script_name),
            CONSTRAINT ck_db_script_mig_history_success CHECK (success_yn IN (''Y'', ''N''))
        )';
    END IF;
END;";

        await using var command = connection.CreateCommand();
        command.CommandText = plsql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task SaveHistoryAsync(OracleConnection connection, string scriptName, string checksum, string successYn, string? errorMessage, CancellationToken cancellationToken)
    {
        const string sql = @"
MERGE INTO db_script_migration_history h
USING (SELECT :script_name AS script_name, :checksum AS checksum, :success_yn AS success_yn, :error_message AS error_message FROM dual) s
   ON (h.script_name = s.script_name)
WHEN MATCHED THEN
    UPDATE SET h.checksum = s.checksum,
               h.applied_at = SYSDATE,
               h.applied_by = USER,
               h.success_yn = s.success_yn,
               h.error_message = s.error_message
WHEN NOT MATCHED THEN
    INSERT (script_id, script_name, checksum, applied_at, applied_by, success_yn, error_message)
    VALUES ((SELECT NVL(MAX(script_id), 0) + 1 FROM db_script_migration_history), s.script_name, s.checksum, SYSDATE, USER, s.success_yn, s.error_message)";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(new OracleParameter("script_name", scriptName));
        command.Parameters.Add(new OracleParameter("checksum", checksum));
        command.Parameters.Add(new OracleParameter("success_yn", successYn));
        command.Parameters.Add(new OracleParameter("error_message", (object?)errorMessage ?? DBNull.Value));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
