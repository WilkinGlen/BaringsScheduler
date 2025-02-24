namespace BaringsScheduler.Services;

using Microsoft.Extensions.Configuration;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Serilog;
using System.Threading;

public interface ISchedulesService
{
    Task<IEnumerable<IJobDetail?>> GetAllJobsAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<ITrigger?>> GetAllTriggersAsync(CancellationToken cancellationToken = default);

    Task DeleteAllFailedTriggersAsync(CancellationToken cancellationToken = default);

    Task DeleteAllTriggersAsync(CancellationToken cancellationToken = default);

    Task DeleteAllFailedTriggersFromGroupAsync(string groupName, CancellationToken cancellationToken);

    Task DeleteAllTriggersFromGroupAsync(string groupName, CancellationToken cancellationToken);
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
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A collection of job details.</returns>
    public async Task<IEnumerable<IJobDetail?>> GetAllJobsAsync(CancellationToken cancellationToken = default)
    {
        var jobKeys = await this.Scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup(), cancellationToken);
        var jobDetails = new List<IJobDetail?>();
        foreach (var jobKey in jobKeys)
        {
            jobDetails.Add(await this.Scheduler.GetJobDetail(jobKey, cancellationToken));
        }

        return jobDetails;
    }

    /// <summary>
    /// Gets all triggers.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A collection of triggers.</returns>
    public async Task<IEnumerable<ITrigger?>> GetAllTriggersAsync(CancellationToken cancellationToken = default)
    {
        var triggerKeys = await this.Scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup(), cancellationToken);
        var triggers = new List<ITrigger?>();
        foreach (var triggerKey in triggerKeys)
        {
            triggers.Add(await this.Scheduler.GetTrigger(triggerKey, cancellationToken));
        }

        return triggers;
    }

    /// <summary>
    /// Deletes all failed triggers.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public async Task DeleteAllFailedTriggersAsync(CancellationToken cancellationToken)
    {
        var triggers = await this.Scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup(), cancellationToken);
        foreach (var triggerKey in triggers)
        {
            if (await this.Scheduler.GetTriggerState(triggerKey, cancellationToken) == TriggerState.Error)
            {
                _ = await this.Scheduler.UnscheduleJob(triggerKey, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Deletes all triggers.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public async Task DeleteAllTriggersAsync(CancellationToken cancellationToken)
    {
        var triggers = await this.Scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup(), cancellationToken);
        foreach (var triggerKey in triggers)
        {
            _ = await this.Scheduler.UnscheduleJob(triggerKey, cancellationToken);
        }
    }

    /// <summary>
    /// Deletes all failed triggers from a specific group.
    /// </summary>
    /// <param name="groupName">The name of the group.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public async Task DeleteAllFailedTriggersFromGroupAsync(string groupName, CancellationToken cancellationToken)
    {
        var triggers = await this.Scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(groupName), cancellationToken);
        foreach (var triggerKey in triggers)
        {
            if (await this.Scheduler.GetTriggerState(triggerKey, cancellationToken) == TriggerState.Error)
            {
                _ = await this.Scheduler.UnscheduleJob(triggerKey, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Deletes all triggers from a specific group.
    /// </summary>
    /// <param name="groupName">The name of the group.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public async Task DeleteAllTriggersFromGroupAsync(string groupName, CancellationToken cancellationToken)
    {
        var triggers = await this.Scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(groupName), cancellationToken);
        foreach (var triggerKey in triggers)
        {
            _ = await this.Scheduler.UnscheduleJob(triggerKey, cancellationToken);
        }
    }
}
