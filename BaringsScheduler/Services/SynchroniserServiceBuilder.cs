namespace BaringsScheduler.Services;

using Microsoft.Extensions.Configuration;
using Quartz;
using Serilog;

/// <summary>
/// Builder class for creating and configuring a <see cref="SynchroniserService"/>.
/// </summary>
public sealed class SynchroniserServiceBuilder
{
    private SynchroniserService? synchroniserService;

    private SynchroniserServiceBuilder() { }

    /// <summary>
    /// Creates a new instance of <see cref="SynchroniserServiceBuilder"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="SynchroniserServiceBuilder"/>.</returns>
    public static SynchroniserServiceBuilder Create() => new();

    /// <summary>
    /// Configures the <see cref="SynchroniserService"/> with the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration to use.</param>
    /// <returns>The current instance of <see cref="SynchroniserServiceBuilder"/>.</returns>
    public SynchroniserServiceBuilder WithConfiguration(IConfiguration configuration)
    {
        this.synchroniserService = new SynchroniserService(configuration);
        Log.Information("SynchroniserServiceBuilder created with configuration");
        return this;
    }

    /// <summary>
    /// Configures logging to a file with the specified log file path and rolling interval.
    /// </summary>
    /// <param name="logFilePath">The path to the log file.</param>
    /// <param name="rollingInterval">The rolling interval for the log file. Default is <see cref="RollingInterval.Day"/>.</param>
    /// <returns>The current instance of <see cref="SynchroniserServiceBuilder"/>.</returns>
    public SynchroniserServiceBuilder WithLoggingToFile(string logFilePath, RollingInterval rollingInterval = RollingInterval.Day)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logFilePath, rollingInterval: rollingInterval)
            .MinimumLevel.Information()
            .CreateLogger();
        Log.Information($"Logging to file {logFilePath}");

        return this;
    }

    /// <summary>
    /// Adds a scheduled job to the <see cref="SynchroniserService"/>.
    /// </summary>
    /// <typeparam name="T">The type of the job, which must implement <see cref="IJob"/>.</typeparam>
    /// <param name="groupName">The group name of the job.</param>
    /// <param name="jobName">The name of the job.</param>
    /// <param name="jobDescription">The description of the job.</param>
    /// <returns>The current instance of <see cref="SynchroniserServiceBuilder"/>.</returns>
    public SynchroniserServiceBuilder WithScheduledJob<T>(string groupName, string jobName, string jobDescription) where T : class, IJob
    {
        SynchroniserService.AddScheduledJob<T>(groupName, jobName, jobDescription);
        return this;
    }

    /// <summary>
    /// Builds the <see cref="SynchroniserService"/> and sets it up.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="SynchroniserService"/> is not set or <see cref="WithConfiguration(IConfiguration)"/> was not called.</exception>
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
