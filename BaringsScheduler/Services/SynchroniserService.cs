namespace BaringsScheduler.Services;

using BaringsScheduler.Jobs;
using BaringsScheduler.Listeners;
using BaringsScheduler.Models;
using BaringsScheduler.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Serilog;

internal sealed class SynchroniserService
{
    private static readonly List<IJobDetail> scheduledJobDetails = [];
    private static readonly ServiceCollection serviceCollection = [];
    private static BaringsSchedulesRepository? schedulesBaringRepository;

    private static IScheduler? scheduler;
    private static IScheduler Scheduler
    {
        get
        {
            try
            {
                if (scheduler == null)
                {
                    var factoryProperties = SchedulerFactoryPropertiesService.GetFactoryProperties(Constants.QuartzDatabaseConnectionString!);
                    var schedulerFactory = new StdSchedulerFactory(factoryProperties);
                    scheduler = schedulerFactory.GetScheduler().Result;
                    Log.Information($"Scheduler created for database: {Constants.QuartzDatabaseConnectionString}");
                }

                return scheduler;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in SynchroniserService.Scheduler getter");
                throw;
            }
        }
    }

    internal SynchroniserService(IConfiguration configuration) => ExtractConfigSettings(configuration);

    internal static async Task Setup(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(Constants.SchedulerDatabaseConnectionString))
        {
            const string message = "SchedulerDatabaseConnectionString is null or empty. Cannot create SchedulesBaringRepository.";
            Log.Error(message);
            throw new ArgumentException(message);
        }

        schedulesBaringRepository = new BaringsSchedulesRepository(Constants.SchedulerDatabaseConnectionString);
        Log.Information($"SchedulesBaringRepository setup with database: {Constants.SchedulerDatabaseConnectionString}");

        await CreateOrEditSynchroniserJob(cancellationToken);
        Scheduler.JobFactory = new JobFactory(serviceCollection.BuildServiceProvider());
        Scheduler.ListenerManager.AddJobListener(new BaringsJobListener());

        await SynchroniseJobs(cancellationToken);
        await SynchroniseTriggers(cancellationToken);
        await SynchroniseOneOffTriggers(cancellationToken);
        await Scheduler.Start(cancellationToken);
    }

    internal static void AddScheduledJob<T>(
        string groupName,
        string jobName,
        string jobDescription) where T : class, IJob
    {
        try
        {
            scheduledJobDetails.Add(JobBuilder.Create<T>()
                .WithIdentity(new JobKey(jobName, groupName))
                .WithDescription(jobDescription)
                .StoreDurably(true)
                .PersistJobDataAfterExecution(true)
                .DisallowConcurrentExecution(true)
                .Build());
            _ = serviceCollection.AddScoped<T>();
            Log.Information($"Scheduled job {jobName} added to group {groupName}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.AddScheduledJob");
            throw;
        }
    }

    internal static async Task SynchroniseJobs(CancellationToken cancellationToken = default)
    {
        try
        {
            await DeleteOldJobs(cancellationToken);
            await InsertNewJobs(cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.SynchroniseJobs");
            throw;
        }
    }

    internal static async Task SynchroniseTriggers(CancellationToken cancellationToken = default)
    {
        try
        {
            var quartzGroupNames = (await Scheduler.GetJobGroupNames(cancellationToken)).Where(x => x.Equals(Constants.SynchroniserGroupName) == false);
            if (quartzGroupNames.Any())
            {
                var triggerDefinitions = await schedulesBaringRepository!.GetAllTriggerDefinitionsAsync();
                foreach (var groupName in quartzGroupNames)
                {
                    foreach (var triggerDefinition in triggerDefinitions.Where(x => x.JobGroupName == groupName))
                    {
                        var quartzTriggerKey = new TriggerKey(triggerDefinition.ScheduleName!, triggerDefinition.JobGroupName!);
                        var quartzTrigger = await Scheduler.GetTrigger(quartzTriggerKey, cancellationToken);
                        if (quartzTrigger == null)
                        {
                            await AddTrigger(triggerDefinition, cancellationToken);
                        }
                        else if (((ICronTrigger)quartzTrigger)?.CronExpressionString?.Equals(triggerDefinition.CronSchedule) == false)
                        {
                            await ChangeTriggerCronExpression(triggerDefinition, quartzTrigger, cancellationToken);
                        }
                    }

                    await DeleteOldTriggers(triggerDefinitions, groupName, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.SynchroniseTriggers");
            throw;
        }
    }

    internal static async Task SynchroniseOneOffTriggers(CancellationToken cancellationToken = default)
    {
        try
        {
            var oneOffTriggerDefinitions = await schedulesBaringRepository!.GetAllOneOffTriggerDefinitionsAsync();
            foreach (var oneOffTriggerDefinition in oneOffTriggerDefinitions)
            {
                var jobKey = new JobKey(oneOffTriggerDefinition.JobName!, oneOffTriggerDefinition.JobGroupName!);
                var job = await Scheduler.GetJobDetail(jobKey, cancellationToken);
                if (job == null)
                {
                    Log.Warning($"Job {jobKey.Name} in group {jobKey.Group} not found for one-off trigger");
                    continue;
                }

                var triggerKey = new TriggerKey(Constants.OneOffTriggerName, jobKey.Group);
                var trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerKey)
                    .WithDescription(oneOffTriggerDefinition.ScheduleDescription)
                    .ForJob(job)
                    .WithPriority(Constants.StandardJobPriority)
                    .StartNow()
                    .Build();
                _ = await Scheduler.ScheduleJob(trigger, cancellationToken);
                Log.Information($"One-off trigger for job {jobKey.Name} in group {jobKey.Group} added");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.SynchroniseOneOffTriggers");
            throw;
        }
    }

    private static async Task DeleteOldTriggers(
        IEnumerable<Models.TriggerDefinition> triggerDefinitions,
        string groupName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var oldTriggers = await Scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(groupName), cancellationToken);
            if (oldTriggers?.Count > 0)
            {
                oldTriggers = [.. oldTriggers.Where(x => triggerDefinitions.Select(y => y.ScheduleName).Contains(x.Name) == false)];
                foreach (var trigger in oldTriggers)
                {
                    _ = await Scheduler.UnscheduleJob(trigger, cancellationToken);
                    Log.Information($"Trigger {trigger.Name} in group {trigger.Group} deleted");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.DeleteOldTrigers");
            throw;
        }
    }

    private static async Task AddTrigger(
        Models.TriggerDefinition triggerDefinition,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (await Scheduler.CheckExists(new JobKey(triggerDefinition.JobName!, triggerDefinition.JobGroupName!), cancellationToken) == false)
            {
                Log.Warning($"Job {triggerDefinition.JobName} in group {triggerDefinition.JobGroupName} not found for trigger {triggerDefinition.ScheduleName}");
                return;
            }

            var newTrigger = TriggerBuilder.Create()
                .WithIdentity(triggerDefinition.ScheduleName!, triggerDefinition.JobGroupName!)
                .WithDescription(triggerDefinition.ScheduleDescription)
                .ForJob(new JobKey(triggerDefinition.JobName!, triggerDefinition.JobGroupName!))
                .WithPriority(Constants.StandardJobPriority)
                .WithCronSchedule(
                    triggerDefinition.CronSchedule!,
                    x => x.WithMisfireHandlingInstructionFireAndProceed())
                .Build();
            _ = await Scheduler.ScheduleJob(newTrigger, cancellationToken);
            Log.Information($"Trigger {triggerDefinition.ScheduleName} for job {newTrigger.JobKey.Name}  added to group {triggerDefinition.JobGroupName}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.AddTrigger");
            throw;
        }
    }

    private static async Task ChangeTriggerCronExpression(
        Models.TriggerDefinition triggerDefinition,
        ITrigger quartzTrigger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var newTrigger = TriggerBuilder.Create()
                .WithIdentity(triggerDefinition.ScheduleName!, triggerDefinition.JobGroupName!)
                .WithDescription(triggerDefinition.ScheduleDescription)
                .ForJob(new JobKey(triggerDefinition.JobName!, triggerDefinition.JobGroupName!))
                .WithPriority(Constants.StandardJobPriority)
                .WithCronSchedule(
                    triggerDefinition.CronSchedule!,
                    x => x.WithMisfireHandlingInstructionFireAndProceed())
                .Build();
            _ = await Scheduler.RescheduleJob(quartzTrigger.Key, newTrigger, cancellationToken);
            Log.Information($"Trigger {triggerDefinition.ScheduleName} for job {newTrigger.JobKey.Name}  in group {triggerDefinition.JobGroupName} updated to: {triggerDefinition.CronSchedule}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.ChangeTriggerCronExpression");
            throw;
        }
    }

    private static async Task DeleteOldJobs(CancellationToken cancellationToken = default)
    {
        try
        {
            var existingJobs = await Scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup(), cancellationToken);
            foreach (var existingJob in existingJobs.Where(x => x.Name != Constants.SynchroniserJobName && x.Group != Constants.SynchroniserGroupName))
            {
                if (scheduledJobDetails.FirstOrDefault(x => x.Key.Group == existingJob.Group && x.Key.Name == existingJob.Name) == null)
                {
                    _ = await Scheduler.DeleteJob(existingJob, cancellationToken);
                    Log.Information($"Job {existingJob.Name} in group {existingJob.Group} deleted");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.DeleteOldJobs");
            throw;
        }
    }

    private static async Task InsertNewJobs(CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var scheduledJob in scheduledJobDetails)
            {
                var existingJob = await Scheduler.GetJobDetail(scheduledJob.Key, cancellationToken);
                if (existingJob == null)
                {
                    var job = JobBuilder.Create(scheduledJob.JobType)
                        .WithIdentity(scheduledJob.Key)
                        .WithDescription(scheduledJob.Description)
                        .StoreDurably()
                        .PersistJobDataAfterExecution(true)
                        .DisallowConcurrentExecution(true)
                        .Build();
                    await Scheduler.AddJob(job, true, cancellationToken);
                    Log.Information($"Job {scheduledJob.Key.Name} in group {scheduledJob.Key.Group} added");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.InsertNewJobs");
            throw;
        }
    }

    private static async Task CreateOrEditSynchroniserJob(CancellationToken cancellationToken = default)
    {
        var expectedRunPeriodMinutes = $"0 0/{Constants.SynchroniserRunPeriodMinutes} * * * ?";
        try
        {
            var syncJob = await Scheduler.GetJobDetail(new JobKey(Constants.SynchroniserJobName, Constants.SynchroniserGroupName), cancellationToken);
            if (syncJob == null)
            {
                await AddSyncJobAndTrigger(expectedRunPeriodMinutes, cancellationToken);
            }
            else
            {
                await EnsureSyncTriggerIsSetCorrectly(expectedRunPeriodMinutes, cancellationToken);
            }

            _ = serviceCollection.AddScoped<SynchroniserJob>();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.CreateOrEditSynchroniserJob");
            throw;
        }
    }

    private static async Task AddSyncJobAndTrigger(
        string expectedRunPeriodMinutes,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var jobKey = new JobKey(Constants.SynchroniserJobName, Constants.SynchroniserGroupName);
            var job = JobBuilder.Create<SynchroniserJob>()
                .WithIdentity(jobKey)
                .WithDescription(Constants.SynchroniserJobDescription)
                .StoreDurably()
                .PersistJobDataAfterExecution(true)
                .DisallowConcurrentExecution(true)
                .Build();
            var trigger = TriggerBuilder.Create()
                .WithIdentity(Constants.SynchroniserTriggerName, Constants.SynchroniserGroupName)
                .WithDescription(Constants.SynchroniserTriggerDescription)
                .WithPriority(Constants.SyncJobPriority)
                .StartNow()
                .WithCronSchedule(expectedRunPeriodMinutes, x => x.WithMisfireHandlingInstructionFireAndProceed())
                .Build();
            _ = await Scheduler.ScheduleJob(job, trigger, cancellationToken);
            Log.Information($"Job {Constants.SynchroniserJobName} in group {Constants.SynchroniserGroupName} added with trigger {Constants.SynchroniserTriggerName}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.AddSyncJobAndTrigger");
            throw;
        }
    }

    private static async Task EnsureSyncTriggerIsSetCorrectly(
        string expectedRunPeriodMinutes,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var syncTrigger = await Scheduler.GetTrigger(new TriggerKey(Constants.SynchroniserTriggerName, Constants.SynchroniserGroupName), cancellationToken);
            if (syncTrigger != null)
            {
                if (syncTrigger is ICronTrigger cronTrigger)
                {
                    if (cronTrigger?.CronExpressionString?.Equals(expectedRunPeriodMinutes) == false)
                    {
                        var trigger = TriggerBuilder.Create()
                            .WithIdentity(Constants.SynchroniserTriggerName, Constants.SynchroniserGroupName)
                            .WithPriority(5)
                            .StartNow()
                            .WithCronSchedule(expectedRunPeriodMinutes, x => x.WithMisfireHandlingInstructionFireAndProceed())
                            .Build();
                        _ = await Scheduler.RescheduleJob(new TriggerKey(Constants.SynchroniserTriggerName, Constants.SynchroniserGroupName), trigger, cancellationToken);
                        Log.Information($"Trigger {Constants.SynchroniserTriggerName} for job {trigger.JobKey.Name} in group {Constants.SynchroniserGroupName} updated to: {expectedRunPeriodMinutes}");
                    }
                }
            }
            else
            {
                var trigger = TriggerBuilder.Create()
                            .WithIdentity(Constants.SynchroniserTriggerName, Constants.SynchroniserGroupName)
                            .WithPriority(5)
                            .StartNow()
                            .WithCronSchedule(expectedRunPeriodMinutes, x => x.WithMisfireHandlingInstructionFireAndProceed())
                            .Build();
                _ = await Scheduler.ScheduleJob(trigger, cancellationToken);
                Log.Information($"Trigger {Constants.SynchroniserTriggerName} for job {trigger.JobKey.Name} in group {Constants.SynchroniserGroupName} added with cron schedule: {expectedRunPeriodMinutes}");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.EnsureSyncTriggerIsSetCorrectly");
            throw;
        }
    }

    private static void ExtractConfigSettings(IConfiguration configuration)
    {
        try
        {
            Constants.QuartzDatabaseConnectionString = configuration.GetConnectionString("QuartzDatabaseConnectionString");
            if (string.IsNullOrWhiteSpace(Constants.QuartzDatabaseConnectionString))
            {
                throw new ArgumentException("QuartzDatabaseConnectionString is required but was not configured");
            }

            Constants.SchedulerDatabaseConnectionString = configuration.GetConnectionString("SchedulerDatabaseConnectionString");
            if (string.IsNullOrWhiteSpace(Constants.SchedulerDatabaseConnectionString))
            {
                throw new ArgumentException("SchedulerDatabaseConnectionString is required but was not configured");
            }

            if (!configuration.GetSection("SynchroniserRunPeriodMinutes").Exists())
            {
                throw new ArgumentException("SynchroniserRunPeriodMinutes is required but was not configured");
            }

            var runPeriod = configuration.GetValue<int>("SynchroniserRunPeriodMinutes");
            Constants.SynchroniserRunPeriodMinutes = runPeriod == 0 ? 10 : runPeriod;
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            Log.Error(ex, "Error in SynchroniserService.ExtractConfigSettings");
            throw;
        }
    }
}
