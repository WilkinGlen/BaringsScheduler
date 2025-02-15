namespace BaringsScheduler.Jobs;

using BaringsScheduler.Services;
using Quartz;

public sealed class SynchroniserJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var syncService = new SynchroniserService();
            await syncService.SynchroniseJobs();
        }
        catch
        {
            //We must catch and handle all exceptions in a job
        }
    }
}
