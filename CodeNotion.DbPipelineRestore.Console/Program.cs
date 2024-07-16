using CodeNotion.DbPipelineRestore;
using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var strategy = new SimpleDeleteTableStrategy(loggerFactory.CreateLogger<SimpleDeleteTableStrategy>());
var restorer = new DbPipelineRestorer(strategy, loggerFactory.CreateLogger<DbPipelineRestorer>());

const string source = "Server=127.0.0.1,1433;Database=DrSafety;uid=sa;pwd=yourStrong(!)Password;MultipleActiveResultSets=True;TrustServerCertificate=True";
const string target = "Server=127.0.0.1,1433;Database=DrSafety2;uid=sa;pwd=yourStrong(!)Password;MultipleActiveResultSets=True;TrustServerCertificate=True";

await restorer.Restore(source, target, "DrSafety", "DrSafety2");