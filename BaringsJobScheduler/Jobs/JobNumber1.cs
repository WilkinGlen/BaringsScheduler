namespace BaringsJobScheduler.Jobs;

using Quartz;
using System.Threading.Tasks;

public sealed class JobNumber1 : IJob
{
    public Task Execute(IJobExecutionContext context) => Console.Out.WriteLineAsync("JobNumber1 executed");
}
