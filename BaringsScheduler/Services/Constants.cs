﻿namespace BaringsScheduler.Services;

internal static class Constants
{
    internal const string SynchroniserGroupName = "SYNC_GROUP_NAME";

    internal const string SynchroniserJobName = "SYNC_JOB_NAME";

    internal const string SynchroniserJobDescription = "This is the job that fires to ensure that Quartz and the client are synchronised";

    internal const string SynchroniserTriggerName = "SYNC_TRIGGER_NAME";

    internal static string? QuartzDatabaseConnectionString { get; set; }

    internal static string? SchedulerDatabaseConnectionString { get; set; }

    internal static int SynchroniserRunPeriodMinutes { get; set; }
}
