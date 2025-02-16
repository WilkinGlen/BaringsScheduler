namespace BaringsScheduler.Repositories.SqlScripts;

internal static class SchedulesBaringRepositorySqlScripts
{
    internal const string GetAllTriggerDefinitionsAsync = @"
        SELECT
            Id,
            ScheduleName,
            JobName,
            JobDescription,
            JobClassName,
            JobGroupName,
            CronSchedule
        FROM dbo.TriggerDefinitions";
}
