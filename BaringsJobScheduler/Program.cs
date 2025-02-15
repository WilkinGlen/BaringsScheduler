using BaringsJobScheduler.Jobs;
using BaringsScheduler.Services;

var builder = WebApplication.CreateBuilder(args);

var syncService = new SynchroniserService(builder.Configuration);

syncService.AddScheduledJob<JobNumber1>("GroupNumber1", "JobNumber1", "JobNumber1 description");
syncService.AddScheduledJob<JobNumber2>("GroupNumber2", "JobNumber2", "JobNumber2 description");

await syncService.Setup();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
