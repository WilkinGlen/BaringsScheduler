namespace BaringsJobScheduler.Jobs;

using Quartz;
using System.Threading.Tasks;

public sealed class JobNumber2 : IJob
{
    public Task Execute(IJobExecutionContext context) => Console.Out.WriteLineAsync("JobNumber2 executed");
}
