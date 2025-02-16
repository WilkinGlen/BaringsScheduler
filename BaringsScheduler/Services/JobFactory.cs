namespace BaringsScheduler.Services;

using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System.Collections.Concurrent;

internal class JobFactory(IServiceProvider serviceProvider) : IJobFactory
{
    protected readonly IServiceProvider? serviceProvider = serviceProvider;
    protected readonly ConcurrentDictionary<IJob, IServiceScope> scopes = new();

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        var scope = this.serviceProvider?.CreateScope();
        IJob? job;
        try
        {
            job = scope?.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
        }
        catch
        {
            scope?.Dispose();
            throw;
        }

        if (job == null || !this.scopes.TryAdd(job, scope!))
        {
            scope?.Dispose();
            throw new Exception("Failed to track DI scope");
        }

        return job;
    }

    public void ReturnJob(IJob job)
    {
        if (this.scopes.TryRemove(job, out var scope))
        {
            scope.Dispose();
        }
    }
}
