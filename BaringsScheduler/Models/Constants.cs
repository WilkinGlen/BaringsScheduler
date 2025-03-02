namespace BaringsScheduler.Models;

public static class Constants
{
    public const string SucceededMessage = "Succeeded";

    internal const int SyncJobPriority = 5;

    internal const int StandardJobPriority = 10;

    internal const string SynchroniserGroupName = "SYNC_GROUP_NAME";

    internal const string SynchroniserJobName = "SYNC_JOB_NAME";

    internal const string SynchroniserJobDescription = "This is the job that fires to ensure that Quartz and the client are synchronised";

    internal const string SynchroniserTriggerName = "SYNC_TRIGGER_NAME";

    internal const string SynchroniserTriggerDescription = "This is the trigger that schedules the job to synchronise Quartz and the client";

    internal const string OneOffTriggerName = "OneOffTrigger";

    internal const string OneOffTriggerDescription = "OneOffTriggerDescription";

    internal static string? QuartzDatabaseConnectionString { get; set; }

    internal static string? SchedulerDatabaseConnectionString { get; set; }

    internal static int SynchroniserRunPeriodMinutes { get; set; }
}
