using BaringsJobScheduler.Jobs;
using BaringsScheduler.Services;

var builder = WebApplication.CreateBuilder(args);

await SynchroniserServiceBuilder
    .Create()
    .WithConfiguration(builder.Configuration)
    .WithScheduledJob<JobNumber1>("GroupNumber1", "JobNumber1", "JobNumber1 description")
    .WithScheduledJob<JobNumber2>("GroupNumber2", "JobNumber2", "JobNumber2 description")
    .Build();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
