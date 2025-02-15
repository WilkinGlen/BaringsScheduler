namespace BaringsScheduler.Jobs;

using BaringsScheduler.Services;
using Quartz;

public sealed class SynchroniserJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await SynchroniserService.SynchroniseJobs();
            await SynchroniserService.SynchroniseTriggers();
        }
        catch (Exception)
        {
            //We must catch and handle all exceptions in a job
        }
        finally
        {
            //Write result to database when we have the table
        }
    }
}
