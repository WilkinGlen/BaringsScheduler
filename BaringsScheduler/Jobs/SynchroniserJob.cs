namespace BaringsScheduler.Jobs;

using BaringsScheduler.Services;
using Quartz;
using Serilog;

public sealed class SynchroniserJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await SynchroniserService.SynchroniseJobs();
            await SynchroniserService.SynchroniseTriggers();
            await SynchroniserService.SynchroniseOneOffTriggers();
        }
        catch (Exception ex)
        {
            //We must catch and handle all exceptions in a job
            Log.Error(ex, "Error in SynchroniserJob.Execute failed");
        }
        finally
        {
            //Write result to database when we have the table
        }
    }
}
