namespace BaringsJobScheduler.Jobs;

using BaringsScheduler.Models;
using Quartz;
using Serilog;
using System.Diagnostics;
using System.Threading.Tasks;

public sealed class JobNumber2 : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            context.MergedJobDataMap.Clear();
            await Task.Delay(5000);
            await Console.Out.WriteLineAsync($"JobNumber2 executed: {DateTime.UtcNow}");
            Debug.WriteLine($"JobNumber2 executed: {DateTime.UtcNow}");

            context.MergedJobDataMap.Add("JobNumber2", Constants.SucceededMessage);

        }
        catch (Exception ex)
        {
            Log.Error(ex, "JobNumber2 failed");
            context.MergedJobDataMap.Add("JobNumber2", ex.Message);
        }
    }
}
