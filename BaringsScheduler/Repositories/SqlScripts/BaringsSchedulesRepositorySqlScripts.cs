namespace BaringsScheduler.Repositories.SqlScripts;

internal static class BaringsSchedulesRepositorySqlScripts
{
    internal const string GetAllTriggerDefinitionsAsyncSql =
        @"SELECT
          	[Id],
          	[ScheduleName],
          	[JobName],
          	[JobDecription],
          	[JobClassName],
          	[JobGroupName],
          	[CronSchedule]
          FROM [dbo].[TriggerDefinitions]";
}
