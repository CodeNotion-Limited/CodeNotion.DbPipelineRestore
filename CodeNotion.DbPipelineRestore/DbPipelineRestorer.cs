using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;

namespace CodeNotion.DbPipelineRestore;

public class DbPipelineRestorer
{
    public virtual async Task Restore(
        string sourceConnectionString,
        string targetConnectionString,
        string sourceDatabaseName,
        string targetDatabaseName)
    {
        // Create a connection string for your SQL Server
        var sourceDb = new SqlConnectionStringBuilder
        {
            ConnectionString = sourceConnectionString
        };

        var targetDb = new SqlConnectionStringBuilder
        {
            ConnectionString = targetConnectionString
        };

        // Initialize DacServices
        var ds = new DacServices(sourceDb.ConnectionString);

        // Export the database to a .bacpac file
        ds.ExportBacpac(@"./backup.bacpac", sourceDatabaseName);

        // Import the .bacpac file to the target database
        var package = BacPackage.Load(@"./backup.bacpac");

        var service = new SimpleDeleteTableStrategy();
        await service.CycleDelete(targetDb.ConnectionString, targetDatabaseName);

        ds.ImportBacpac(package, "DrSafety2");
    }
}