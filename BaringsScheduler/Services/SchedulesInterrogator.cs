namespace BaringsScheduler.Services;

using Microsoft.Extensions.Configuration;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Serilog;

public interface ISchedulesInterrogator
{
    Task<IEnumerable<IJobDetail?>> GetAllJobsAsync();

    Task<IEnumerable<ITrigger?>> GetAllTriggersAsync();

    Task DeleteAllFailedTriggers();

    Task DeleteAllTriggers();
}

public sealed class SchedulesInterrogator(IConfiguration configuration) : ISchedulesInterrogator
{
    private IScheduler? scheduler;
    private IScheduler Scheduler
    {
        get
        {
            try
            {
                if (this.scheduler == null)
                {
                    if (configuration.GetConnectionString("QuartzDatabaseConnectionString") == null)
                    {
                        throw new Exception("QuartzDatabaseConnectionString is null");
                    }

                    var factoryProperties = SchedulerFactoryPropertiesService.GetFactoryProperties(configuration.GetConnectionString("QuartzDatabaseConnectionString")!);
                    var schedulerFactory = new StdSchedulerFactory(factoryProperties);
                    this.scheduler = schedulerFactory.GetScheduler().Result;
                }

                return this.scheduler;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in SynchroniserService.Scheduler getter");
                throw;
            }
        }
    }

    public async Task<IEnumerable<IJobDetail?>> GetAllJobsAsync() =>
        await this.Scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup())
            .ContinueWith(task => task.Result.Select(jobKey => this.Scheduler.GetJobDetail(jobKey).Result));

    public async Task<IEnumerable<ITrigger?>> GetAllTriggersAsync() =>
        await this.Scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup())
            .ContinueWith(task => task.Result.Select(triggerKey => this.Scheduler.GetTrigger(triggerKey).Result));

    public async Task DeleteAllFailedTriggers()
    {
        var triggers = await this.Scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());
        foreach (var triggerKey in triggers)
        {
            if (await this.Scheduler.GetTriggerState(triggerKey) == TriggerState.Error)
            {
                _ = await this.Scheduler.UnscheduleJob(triggerKey);
            }
        }
    }

    public async Task DeleteAllTriggers()
    {
        var triggers = await this.Scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());
        foreach (var triggerKey in triggers)
        {
            _ = await this.Scheduler.UnscheduleJob(triggerKey);
        }
    }
}
