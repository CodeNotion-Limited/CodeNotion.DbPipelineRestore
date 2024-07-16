using CodeNotion.DbPipelineRestore;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;

// Create a connection string for your SQL Server
var sourceDb = new SqlConnectionStringBuilder();
sourceDb.ConnectionString = "Server=127.0.0.1,1433;Database=DrSafety;uid=sa;pwd=yourStrong(!)Password;MultipleActiveResultSets=True;TrustServerCertificate=True";


var targetDb = new SqlConnectionStringBuilder();
targetDb.ConnectionString = "Server=127.0.0.1,1433;Database=DrSafety2;uid=sa;pwd=yourStrong(!)Password;MultipleActiveResultSets=True;TrustServerCertificate=True";

// Initialize DacServices
var ds = new DacServices(sourceDb.ConnectionString);

// Export the database to a .bacpac file
ds.ExportBacpac(@"./backup.bacpac", "DrSafety");

// Import the .bacpac file to the target database
var package = BacPackage.Load(@"./backup.bacpac");

var service = new SimpleDeleteTableStrategy();
await service.CycleDelete(targetDb.ConnectionString, "DrSafety2");


ds.ImportBacpac(package, "DrSafety2");



;
