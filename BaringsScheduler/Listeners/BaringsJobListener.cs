namespace BaringsScheduler.Listeners;

using BaringsScheduler.Services;
using Quartz;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

public class BaringsJobListener : IJobListener
{
    public string Name => "BaringsJobListener";

    public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
    {
        try
        {
            if (context.MergedJobDataMap.ContainsValue(Constants.SucceededMessage))
            {
                await SchedulerLogsService.LogJobExecutionAsync(
                    context,
                    $"{context.JobDetail.Key.Name} executed",
                    Constants.QuartzDatabaseConnectionString!);
                return;
            }

            await SchedulerLogsService.LogJobFailureAsync(
                context,
                $"{context.JobDetail.Key.Name} failed",
                context.MergedJobDataMap[context.JobDetail.Key.Name].ToString(),
                Constants.QuartzDatabaseConnectionString!);
        }
        catch (Exception ex)
        {
            // We must catch and handle all exceptions in a listener
            Log.Error(ex, $"Failed to log job execution for {context.JobDetail.Key.Name}");
        }
    }

    public async Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default) => await Task.CompletedTask;

    public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default) => await Task.CompletedTask;
}
