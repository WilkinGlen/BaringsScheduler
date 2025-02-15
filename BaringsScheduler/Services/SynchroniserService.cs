namespace BaringsScheduler.Services;

using BaringsScheduler.Jobs;
using Microsoft.Extensions.Configuration;
using Quartz;
using Quartz.Impl;
using Serilog;
using static Quartz.Logging.OperationName;

public interface ISynchroniserService
{
    Task Setup();

    Task SynchroniseJobs();
}

public sealed class SynchroniserService : ISynchroniserService
{
    private IScheduler? scheduler;
    private IScheduler Scheduler
    {
        get
        {
            if (this.scheduler == null)
            {
                var factoryProperties = SchedulerFactoryPropertiesService.GetFactoryProperties(Constants.QuartzDatabaseConnectionString!);
                var schedulerFactory = new StdSchedulerFactory(factoryProperties);
                this.scheduler = schedulerFactory.GetScheduler().Result;
            }

            return this.scheduler;
        }
    }

    public SynchroniserService() { }

    public SynchroniserService(IConfiguration configuration) => ExtractConfigSettings(configuration);

    public async Task Setup() => await this.CreateOrEditSynchroniserJob();

    public async Task SynchroniseJobs()
    {
        //Get all jobs from the quartz database
        //Get trigger definitions from the scheduler database
        //For each job, if trigger definition is different / non - existent / new then replace / delete / create trigger
        //Delete all triggers definitions that don't have matching jobs
    }

    private async Task CreateOrEditSynchroniserJob()
    {
        var expectedRunPeriodMinutes = $"0 0/{Constants.SynchroniserRunPeriodMinutes} * * * ?";
        try
        {
            // Check if the job already exists
            var syncJob = await this.Scheduler.GetJobDetail(new JobKey(Constants.SynchroniserJobName, Constants.SynchroniserGroupName));
            if (syncJob == null)
            {
                // Create the job
                var jobKey = new JobKey(Constants.SynchroniserJobName, Constants.SynchroniserGroupName);
                var job = JobBuilder.Create<SynchroniserJob>()
                    .WithIdentity(jobKey)
                    .StoreDurably()
                    .Build();
                var trigger = TriggerBuilder.Create()
                    .WithIdentity(Constants.SynchroniserTriggerName, Constants.SynchroniserGroupName)
                    .WithPriority(5)
                    .StartNow()
                    .WithCronSchedule(expectedRunPeriodMinutes, x => x.WithMisfireHandlingInstructionFireAndProceed())
                    .Build();
                _ = await this.Scheduler.ScheduleJob(job, trigger);
            }
            else
            {
                // Check if the trigger is set to the correct period
                var syncTrigger = await this.Scheduler.GetTrigger(new TriggerKey(Constants.SynchroniserTriggerName, Constants.SynchroniserGroupName));
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
                        _ = await this.Scheduler.RescheduleJob(new TriggerKey(Constants.SynchroniserTriggerName, Constants.SynchroniserGroupName), trigger);
                    }
                }
            }

            await this.Scheduler.Start();
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
