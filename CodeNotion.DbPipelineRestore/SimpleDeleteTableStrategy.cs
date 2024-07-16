using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Smo;

namespace CodeNotion.DbPipelineRestore;

public interface IDbDeleteStrategy
{
    Task CycleDelete(string connectionString, string dbName);
}

public class SimpleDeleteTableStrategy(ILogger<SimpleDeleteTableStrategy> logger) : IDbDeleteStrategy
{
    public virtual async Task CycleDelete(string connectionString, string dbName)
    {
        logger.LogInformation("Deleting tables in {dbName}", dbName);
        var connection = new SqlConnection(connectionString);
        connection.Open();
        var server = new Server(dbName)
        {
            ConnectionContext =
            {
                ConnectionString = connectionString
            }
        };
        var database = server.Databases[dbName];

        for (var i = 0; i < 10; i++)
        {
            database.Tables.Refresh();
            var tables = database.Tables;

            logger.LogInformation("Deletion cycle {i} of 10, remaining tables: {tables.Count}", i, tables.Count);
            foreach (Table t in tables)
            {
                try
                {
                    await using var command = connection.CreateCommand();
                    GenerateDeleteCommand(t, command);
                    await command.ExecuteNonQueryAsync();
                }
                catch
                {
                    // In this strategy we try deleting the tables, if the delete fails its fine since
                    // it will be deleted by the subsequent cycle
                }
            }
        }

        connection.Close();
    }

    public virtual void GenerateDeleteCommand(Table table, SqlCommand command)
    {
        if (table.TemporalType == TableTemporalType.SystemVersioned)
        {
            command.CommandText = $"ALTER TABLE {table} SET (SYSTEM_VERSIONING = OFF);" +
                                  $"DROP TABLE {table};" +
                                  $"DROP TABLE [{table.Schema}].[{table.Name}_History];";
            return;
        }

        command.CommandText += $"DROP TABLE {table}";
    }
}