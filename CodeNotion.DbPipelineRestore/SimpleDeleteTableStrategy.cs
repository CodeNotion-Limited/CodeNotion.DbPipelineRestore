using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Smo;

namespace CodeNotion.DbPipelineRestore;

public interface IDbDeleteStrategy
{
}

public class SimpleDeleteTableStrategy : IDbDeleteStrategy
{
    public virtual async Task CycleDelete(string connectionString, string dbName)
    {
        var server = new Server(dbName)
        {
            ConnectionContext =
            {
                ConnectionString = connectionString
            }
        };
        var connection = new SqlConnection(connectionString);
        connection.Open();
        var database = server.Databases[dbName];
        var tables = database.Tables;

        for (var i = 0; i < 10; i++)
        {
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