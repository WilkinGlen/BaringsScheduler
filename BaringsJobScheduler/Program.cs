using BaringsJobScheduler.Jobs;
using BaringsScheduler.Services;

var builder = WebApplication.CreateBuilder(args);

var logFilePath = "C:\\ApplicationLogs\\BaringsJobScheduler\\log.log";
var applicationName = Environment.GetEnvironmentVariable("APPLICATION_NAME") ?? "BaringsJobScheduler";

await SynchroniserServiceBuilder
    .Create()
    .WithLoggingToFile(logFilePath: logFilePath, Serilog.RollingInterval.Hour)
    .WithConfiguration(builder.Configuration)
    .WithScheduledJob<JobNumber1>(applicationName, "JobNumber1", "JobNumber1 description")
    .WithScheduledJob<JobNumber2>(applicationName, "JobNumber2", "JobNumber2 description")
    .Build();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
