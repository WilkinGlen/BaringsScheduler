namespace BaringsScheduler.Services;

using Microsoft.Extensions.Configuration;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Serilog;

public interface ISchedulesService
{
    Task<IEnumerable<IJobDetail?>> GetAllJobsAsync();

    Task<IEnumerable<ITrigger?>> GetAllTriggersAsync();

    Task DeleteAllFailedTriggersAsync();

    Task DeleteAllTriggersAsync();

    Task DeleteAllFailedTriggersFromGroupAsync(string groupName);

    Task DeleteAllTriggersFromGroupAsync(string groupName);
}

/// <summary>
/// Service for managing and interrogating Quartz schedules.
/// </summary>
public sealed class SchedulesService(IConfiguration configuration) : ISchedulesService
{
    private IScheduler? scheduler;

    /// <summary>
    /// Gets the Quartz scheduler instance.
    /// </summary>
    private IScheduler Scheduler
    {
        get
        {
            try
            {
                if (this.scheduler == null)
                {
                    if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("QuartzDatabaseConnectionString")))
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

    /// <summary>
    /// Gets all job details.
    /// </summary>
    /// <returns>A collection of job details.</returns>
    public async Task<IEnumerable<IJobDetail?>> GetAllJobsAsync() =>
        await this.Scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup())
            .ContinueWith(task => task.Result.Select(jobKey => this.Scheduler.GetJobDetail(jobKey).Result));

    /// <summary>
    /// Gets all triggers.
    /// </summary>
    /// <returns>A collection of triggers.</returns>
    public async Task<IEnumerable<ITrigger?>> GetAllTriggersAsync() =>
        await this.Scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup())
            .ContinueWith(task => task.Result.Select(triggerKey => this.Scheduler.GetTrigger(triggerKey).Result));

    /// <summary>
    /// Deletes all failed triggers.
    /// </summary>
    public async Task DeleteAllFailedTriggersAsync()
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

    /// <summary>
    /// Deletes all triggers.
    /// </summary>
    public async Task DeleteAllTriggersAsync()
    {
        var triggers = await this.Scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());
        foreach (var triggerKey in triggers)
        {
            _ = await this.Scheduler.UnscheduleJob(triggerKey);
        }
    }

    /// <summary>
    /// Deletes all failed triggers from a specific group.
    /// </summary>
    /// <param name="groupName">The name of the group.</param>
    public async Task DeleteAllFailedTriggersFromGroupAsync(string groupName)
    {
        var triggers = await this.Scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(groupName));
        foreach (var triggerKey in triggers)
        {
            if (await this.Scheduler.GetTriggerState(triggerKey) == TriggerState.Error)
            {
                _ = await this.Scheduler.UnscheduleJob(triggerKey);
            }
        }
    }

    /// <summary>
    /// Deletes all triggers from a specific group.
    /// </summary>
    /// <param name="groupName">The name of the group.</param>
    public async Task DeleteAllTriggersFromGroupAsync(string groupName)
    {
        var triggers = await this.Scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(groupName));
        foreach (var triggerKey in triggers)
        {
            _ = await this.Scheduler.UnscheduleJob(triggerKey);
        }
    }
}
