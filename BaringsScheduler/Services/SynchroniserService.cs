namespace BaringsScheduler.Services;

using BaringsScheduler.Jobs;
using Microsoft.Extensions.Configuration;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Serilog;

public interface ISynchroniserService
{
    Task Setup();

    Task SynchroniseJobs();

    Task SynchroniseTriggers();
}

public sealed class SynchroniserService : ISynchroniserService
{
    private static IScheduler? scheduler;
    private static IScheduler Scheduler
    {
        get
        {
            if (scheduler == null)
            {
                var factoryProperties = SchedulerFactoryPropertiesService.GetFactoryProperties(Constants.QuartzDatabaseConnectionString!);
                var schedulerFactory = new StdSchedulerFactory(factoryProperties);
                scheduler = schedulerFactory.GetScheduler().Result;
            }

            return scheduler;
        }
    }

    private static List<IJobDetail> scheduledJobDetails = [];

    public SynchroniserService() { }

    public SynchroniserService(IConfiguration configuration) => ExtractConfigSettings(configuration);

    public async Task Setup() => await this.CreateOrEditSynchroniserJob();

    public void AddScheduledJob<T>(string groupName, string jobName, string jobDescription) where T : IJob
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
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.AddScheduledJob");
            throw;
        }
    }

    public async Task SynchroniseJobs()
    {
        //Delete all jobs from quartz database that are not in scheduledJobDetails
        var existingJobs = await Scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
        foreach (var existingJob in existingJobs.Where(x => x.Name != Constants.SynchroniserJobName && x.Group != Constants.SynchroniserGroupName))
        {
            if(scheduledJobDetails.FirstOrDefault(x => x.Key.Group == existingJob.Group && x.Key.Name == existingJob.Name) == null)
            {
                _ = await Scheduler.DeleteJob(existingJob);
            }
        }
        //Insert all jobs in scheduledJobDetails that aren't already in quartz database
        foreach (var scheduledJob in scheduledJobDetails)
        {
            var existingJob = await Scheduler.GetJobDetail(scheduledJob.Key);
            if (existingJob == null)
            {
                var job = JobBuilder.Create<SynchroniserJob>()
                    .WithIdentity(scheduledJob.Key)
                    .WithDescription(Constants.SynchroniserJobDescription)
                    .StoreDurably()
                    .PersistJobDataAfterExecution(true)
                    .DisallowConcurrentExecution(true)
                    .Build();
                await Scheduler.AddJob(job, true);
            }
        }
    }

    public async Task SynchroniseTriggers()
    {

    }

    private async Task CreateOrEditSynchroniserJob()
    {
        var expectedRunPeriodMinutes = $"0 0/{Constants.SynchroniserRunPeriodMinutes} * * * ?";
        try
        {
            // Check if the job already exists
            var syncJob = await Scheduler.GetJobDetail(new JobKey(Constants.SynchroniserJobName, Constants.SynchroniserGroupName));
            if (syncJob == null)
            {
                // Create the job
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
                    .WithPriority(5)
                    .StartNow()
                    .WithCronSchedule(expectedRunPeriodMinutes, x => x.WithMisfireHandlingInstructionFireAndProceed())
                    .Build();
                _ = await Scheduler.ScheduleJob(job, trigger);
            }
            else
            {
                // Check if the trigger is set to the correct period
                var syncTrigger = await Scheduler.GetTrigger(new TriggerKey(Constants.SynchroniserTriggerName, Constants.SynchroniserGroupName));
                if (syncTrigger is ICronTrigger cronTrigger)
                {
                    if (cronTrigger?.CronExpressionString?.Equals(expectedRunPeriodMinutes) == false)
                    {
                        // Reschedule the job
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

            await Scheduler.Start();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SynchroniserService.CreateOrEditSynchroniserJob");
            throw;
        }
    }

    private static void ExtractConfigSettings(IConfiguration configuration)
    {
        Constants.QuartzDatabaseConnectionString = configuration.GetConnectionString("QuartzDatabaseConnectionString");
        Constants.SchedulerDatabaseConnectionString = configuration.GetConnectionString("SchedulerDatabaseConnectionString");
        var runPeriod = configuration.GetValue<int>("SynchroniserRunPeriodMinutes");
        Constants.SynchroniserRunPeriodMinutes = runPeriod == 0 ? 10 : runPeriod;
    }
}
