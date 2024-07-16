﻿using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Dac;

namespace CodeNotion.DbPipelineRestore;

public class DbPipelineRestorer(IDbDeleteStrategy dbDeleteStrategy, ILogger<DbPipelineRestorer> logger)
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

        var ds = new DacServices(sourceDb.ConnectionString);

        logger.LogInformation("Exporting bacpac from {sourceDatabaseName}", sourceDatabaseName);
        ds.ExportBacpac("./backup.bacpac", sourceDatabaseName);
        logger.LogInformation("Exported bacpac completed");

        var package = BacPackage.Load("./backup.bacpac");

        await dbDeleteStrategy.CycleDelete(targetDb.ConnectionString, targetDatabaseName);
        logger.LogInformation("Delete completed");
        logger.LogInformation("Importing bacpac to {targetDatabaseName}", targetDatabaseName);
        ds.ImportBacpac(package, targetDatabaseName);
        
        logger.LogInformation("Import completed");
    }
}