namespace BaringsJobScheduler.Jobs;

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
            await Console.Out.WriteLineAsync($"JobNumber1 executed: {DateTime.UtcNow}");
            Debug.WriteLine($"JobNumber1 executed: {DateTime.UtcNow}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "JobNumber1 failed");
        }
        finally
        {
            //Write result to database when we have the table
        }
    }
}
