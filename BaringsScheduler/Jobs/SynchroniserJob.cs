namespace BaringsScheduler.Jobs;

using BaringsScheduler.Models;
using BaringsScheduler.Services;
using Quartz;
using Serilog;

public sealed class SynchroniserJob(CancellationToken cancellationToken = default) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            context.MergedJobDataMap.Clear();

            await SynchroniserService.SynchroniseJobs(cancellationToken);
            await SynchroniserService.SynchroniseTriggers(cancellationToken);
            await SynchroniserService.SynchroniseOneOffTriggers(cancellationToken);

            context.MergedJobDataMap.Add(Constants.SynchroniserJobName, Constants.SucceededMessage);
        }
        catch (Exception ex)
        {
            //We must catch and handle all exceptions in a job
            Log.Error(ex, "Error in SynchroniserJob.Execute failed");
            context.MergedJobDataMap.Add(Constants.SynchroniserJobName, ex.Message);
        }
    }
}
