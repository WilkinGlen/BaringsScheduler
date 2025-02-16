namespace BaringsScheduler.Services;

using Microsoft.Extensions.Configuration;
using Quartz;
using Serilog;

public sealed class SynchroniserServiceBuilder
{
    private SynchroniserService? synchroniserService;

    private SynchroniserServiceBuilder() { }

    public static SynchroniserServiceBuilder Create() => new();

    public SynchroniserServiceBuilder WithConfiguration(IConfiguration configuration)
    {
        this.synchroniserService = new SynchroniserService(configuration);
        Log.Information("SynchroniserServiceBuilder created with configuration");
        return this;
    }

    public SynchroniserServiceBuilder WithLoggingToFile(string logFilePath, RollingInterval rollingInterval = RollingInterval.Day)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logFilePath, rollingInterval: rollingInterval)
            .MinimumLevel.Information()
            .CreateLogger();
        Log.Information($"Logging to file {logFilePath}");

        return this;
    }

    public SynchroniserServiceBuilder WithScheduledJob<T>(string groupName, string jobName, string jobDescription) where T : class, IJob
    {
        SynchroniserService.AddScheduledJob<T>(groupName, jobName, jobDescription);
        return this;
    }

    public async Task Build()
    {
        if (this.synchroniserService == null)
        {
            throw new InvalidOperationException("SynchroniserService is not set, or WithConfigutation(IConfiguration configuration) not called");
        }

        Log.Information("SynchroniserServiceBuilder built");
        await SynchroniserService.Setup();
    }
}
