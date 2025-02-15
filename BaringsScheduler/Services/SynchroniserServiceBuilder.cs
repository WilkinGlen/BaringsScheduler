namespace BaringsScheduler.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

public class SynchroniserServiceBuilder
{
    private SynchroniserService? synchroniserService;

    private SynchroniserServiceBuilder() { }

    public static SynchroniserServiceBuilder Create() => new();

    public SynchroniserServiceBuilder WithConfiguration(IConfiguration configuration)
    {
        this.synchroniserService = new SynchroniserService(configuration);
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

        await SynchroniserService.Setup();
    }
}
