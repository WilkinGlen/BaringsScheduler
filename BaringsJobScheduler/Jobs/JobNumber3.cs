namespace BaringsJobScheduler.Jobs;

using BaringsScheduler.Models;
using Quartz;
using Serilog;
using System.Diagnostics;
using System.Threading.Tasks;

public sealed class JobNumber3 : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            context.MergedJobDataMap.Clear();

            await Console.Out.WriteLineAsync($"JobNumber3 executed: {DateTime.UtcNow}");
            Debug.WriteLine($"JobNumber3 executed: {DateTime.UtcNow}");

            context.MergedJobDataMap.Add("JobNumber3", Constants.SucceededMessage);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "JobNumber3 failed");
            context.MergedJobDataMap.Add("JobNumber3", ex.Message);
        }
    }
}
