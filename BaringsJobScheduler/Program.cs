using BaringsScheduler.Services;

var builder = WebApplication.CreateBuilder(args);

var syncService = new SynchroniserService(builder.Configuration);
await syncService.Setup();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
