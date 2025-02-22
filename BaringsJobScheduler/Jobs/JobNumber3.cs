namespace BaringsJobScheduler.Jobs;

using Quartz;
using Serilog;
using System.Diagnostics;

public class JobNumber3 : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            Task.Delay(3000).Wait();
            await Console.Out.WriteLineAsync($"JobNumber3 executed: {DateTime.UtcNow}");
            Debug.WriteLine($"JobNumber3 executed: {DateTime.UtcNow}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "JobNumber3 failed");
        }
        finally
        {
            //Write result to database when we have the table
        }
    }
}
