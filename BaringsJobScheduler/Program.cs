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
    .WithScheduledJob<JobNumber3>(applicationName, "JobNumber3", "JobNumber3 description")
    .Build();

ISchedulesService schedulesInterrogator = new SchedulesService(builder.Configuration);
foreach (var job in await schedulesInterrogator.GetAllJobsAsync())
{
    Console.WriteLine(job?.Description);
}

var triggers = (await schedulesInterrogator.GetAllTriggersAsync()).ToList();
foreach (var trigger in triggers)
{
    Console.WriteLine(trigger?.Description);
}

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
