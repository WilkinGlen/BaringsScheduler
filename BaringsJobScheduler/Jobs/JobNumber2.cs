namespace BaringsJobScheduler.Jobs;

using Quartz;
using Serilog;
using System.Threading.Tasks;

public sealed class JobNumber2 : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await Console.Out.WriteLineAsync("JobNumber2 executed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "JobNumber2 failed");
        }
        finally
        {
            //Write result to database when we have the table
        }
    }
}
