namespace BaringsJobScheduler.Jobs;

using BaringsScheduler.Models;
using Quartz;
using Serilog;
using System.Diagnostics;
using System.Threading.Tasks;

public sealed class JobNumber1 : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            context.MergedJobDataMap.Clear();
            await Task.Delay(2500);
            await Console.Out.WriteLineAsync($"JobNumber1 executed: {DateTime.UtcNow}");
            Debug.WriteLine($"JobNumber1 executed: {DateTime.UtcNow}");
            context.MergedJobDataMap.Add("JobNumber1", Constants.SucceededMessage);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "JobNumber1 failed");
            context.MergedJobDataMap.Add("JobNumber1", "Failed");
        }
    }
}
