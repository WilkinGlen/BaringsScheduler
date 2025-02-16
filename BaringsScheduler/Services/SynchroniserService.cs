namespace BaringsScheduler.Services;

using BaringsScheduler.Jobs;
using BaringsScheduler.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Serilog;

internal sealed class SynchroniserService
{
    private const int SyncJobPriority = 5;
    private const int StandardJobPriority = 10;

    private static readonly List<IJobDetail> scheduledJobDetails = [];
    private static readonly ServiceCollection serviceCollection = [];
    private static readonly SchedulesBaringRepository schedulesBaringRepository = new ();

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

    internal static async Task Setup()
    {
        await CreateOrEditSynchroniserJob();
        Scheduler.JobFactory = new JobFactory(serviceCollection.BuildServiceProvider());
    }

    internal static void AddScheduledJob<T>(string groupName, string jobName, string jobDescription) where T : class, IJob
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

    internal static async Task SynchroniseJobs()
    {
        try
        {
            await DeleteOldJobs();
            await InsertNewJobs();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.SynchroniseJobs");
            throw;
        }
    }

    internal static async Task SynchroniseTriggers()
    {
        try
        {
            var quartzGroupNames = (await Scheduler.GetJobGroupNames()).Where(x => x.Equals(Constants.SynchroniserGroupName) == false);
            if (quartzGroupNames.Any())
            {
                var triggerDefinitions = await schedulesBaringRepository.GetAllTriggerDefinitionsAsync();
                foreach (var groupName in quartzGroupNames)
                {
                    foreach (var triggerDefinition in triggerDefinitions.Where(x => x.JobGroupName == groupName))
                    {
                        var quartzTriggerKey = new TriggerKey(triggerDefinition.ScheduleName!, triggerDefinition.JobGroupName!);
                        var quartzTrigger = await Scheduler.GetTrigger(quartzTriggerKey);
                        if (quartzTrigger == null)
                        {
                            await AddTrigger(triggerDefinition);
                        }
                        else if (((ICronTrigger)quartzTrigger)?.CronExpressionString?.Equals(triggerDefinition.CronSchedule) == false)
                        {
                            await ChangeTriggerCronExpression(triggerDefinition, quartzTrigger);
                        }
                    }

                    await DeleteOldTriggers(triggerDefinitions, groupName);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.SynchroniseTriggers");
            throw;
        }
    }

    private static async Task DeleteOldTriggers(IEnumerable<Models.TriggerDefinition> triggerDefinitions, string groupName)
    {
        try
        {
            var oldTriggers = await Scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(groupName));
            if (oldTriggers?.Count > 0)
            {
                oldTriggers = [.. oldTriggers.Where(x => triggerDefinitions.Select(y => y.ScheduleName).Contains(x.Name) == false)];
                foreach (var trigger in oldTriggers)
                {
                    _ = await Scheduler.UnscheduleJob(trigger);
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

    private static async Task AddTrigger(Models.TriggerDefinition triggerDefinition)
    {
        try
        {
            var newTrigger = TriggerBuilder.Create()
                .WithIdentity(triggerDefinition.ScheduleName!, triggerDefinition.JobGroupName!)
                .ForJob(new JobKey(triggerDefinition.JobName!, triggerDefinition.JobGroupName!))
                .WithPriority(StandardJobPriority)
                .WithCronSchedule(
                    triggerDefinition.CronSchedule!,
                    x => x.WithMisfireHandlingInstructionFireAndProceed())
                .Build();
            _ = await Scheduler.ScheduleJob(newTrigger);
            Log.Information($"Trigger {triggerDefinition.ScheduleName} for job {newTrigger.JobKey.Name}  added to group {triggerDefinition.JobGroupName}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.AddTrigger");
            throw;
        }
    }

    private static async Task ChangeTriggerCronExpression(Models.TriggerDefinition triggerDefinition, ITrigger quartzTrigger)
    {
        try
        {
            var newTrigger = TriggerBuilder.Create()
                .WithIdentity(triggerDefinition.ScheduleName!, triggerDefinition.JobGroupName!)
                .ForJob(new JobKey(triggerDefinition.JobName!, triggerDefinition.JobGroupName!))
                .WithPriority(StandardJobPriority)
                .WithCronSchedule(
                    triggerDefinition.CronSchedule!,
                    x => x.WithMisfireHandlingInstructionFireAndProceed())
                .Build();
            _ = await Scheduler.RescheduleJob(quartzTrigger.Key, newTrigger);
            Log.Information($"Trigger {triggerDefinition.ScheduleName} for job {newTrigger.JobKey.Name}  in group {triggerDefinition.JobGroupName} updated to: {triggerDefinition.CronSchedule}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.ChangeTriggerCronExpression");
            throw;
        }
    }

    private static async Task DeleteOldJobs()
    {
        try
        {
            var existingJobs = await Scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            foreach (var existingJob in existingJobs.Where(x => x.Name != Constants.SynchroniserJobName && x.Group != Constants.SynchroniserGroupName))
            {
                if (scheduledJobDetails.FirstOrDefault(x => x.Key.Group == existingJob.Group && x.Key.Name == existingJob.Name) == null)
                {
                    _ = await Scheduler.DeleteJob(existingJob);
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

    private static async Task InsertNewJobs()
    {
        try
        {
            foreach (var scheduledJob in scheduledJobDetails)
            {
                var existingJob = await Scheduler.GetJobDetail(scheduledJob.Key);
                if (existingJob == null)
                {
                    var job = JobBuilder.Create(scheduledJob.JobType)
                        .WithIdentity(scheduledJob.Key)
                        .WithDescription(scheduledJob.Description)
                        .StoreDurably()
                        .PersistJobDataAfterExecution(true)
                        .DisallowConcurrentExecution(true)
                        .Build();
                    await Scheduler.AddJob(job, true);
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

    private static async Task CreateOrEditSynchroniserJob()
    {
        var expectedRunPeriodMinutes = $"0 0/{Constants.SynchroniserRunPeriodMinutes} * * * ?";
        try
        {
            var syncJob = await Scheduler.GetJobDetail(new JobKey(Constants.SynchroniserJobName, Constants.SynchroniserGroupName));
            if (syncJob == null)
            {
                await AddSyncJobAndTrigger(expectedRunPeriodMinutes);
            }
            else
            {
                await EnsureSyncTriggerIsSetCorrectly(expectedRunPeriodMinutes);
            }

            _ = serviceCollection.AddScoped<SynchroniserJob>();
            await Scheduler.Start();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.CreateOrEditSynchroniserJob");
            throw;
        }
    }

    private static async Task AddSyncJobAndTrigger(string expectedRunPeriodMinutes)
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
                .WithPriority(SyncJobPriority)
                .StartNow()
                .WithCronSchedule(expectedRunPeriodMinutes, x => x.WithMisfireHandlingInstructionFireAndProceed())
                .Build();
            _ = await Scheduler.ScheduleJob(job, trigger);
            Log.Information($"Job {Constants.SynchroniserJobName} in group {Constants.SynchroniserGroupName} added with trigger {Constants.SynchroniserTriggerName}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.AddSyncJobAndTrigger");
            throw;
        }
    }

    private static async Task EnsureSyncTriggerIsSetCorrectly(string expectedRunPeriodMinutes)
    {
        try
        {
            var syncTrigger = await Scheduler.GetTrigger(new TriggerKey(Constants.SynchroniserTriggerName, Constants.SynchroniserGroupName));
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
                        _ = await Scheduler.RescheduleJob(new TriggerKey(Constants.SynchroniserTriggerName, Constants.SynchroniserGroupName), trigger);
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
                _ = await Scheduler.ScheduleJob(trigger);
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
            Constants.SchedulerDatabaseConnectionString = configuration.GetConnectionString("SchedulerDatabaseConnectionString");
            var runPeriod = configuration.GetValue<int>("SynchroniserRunPeriodMinutes");
            Constants.SynchroniserRunPeriodMinutes = runPeriod == 0 ? 10 : runPeriod;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.ExtractConfigSettings");
            throw;
        }
    }
}
