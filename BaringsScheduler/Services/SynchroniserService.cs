namespace BaringsScheduler.Services;

using BaringsScheduler.Jobs;
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

    private static readonly List<IJobDetail> scheduledJobDetails = [];

    private static ServiceCollection serviceCollection = [];

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
            //TOD: Implement SynchroniseTriggers
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.SynchroniseTriggers");
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
                    var job = JobBuilder.Create<SynchroniserJob>()
                        .WithIdentity(scheduledJob.Key)
                        .WithDescription(scheduledJob.Description)
                        .StoreDurably()
                        .PersistJobDataAfterExecution(true)
                        .DisallowConcurrentExecution(true)
                        .Build();
                    await Scheduler.AddJob(job, true);
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
