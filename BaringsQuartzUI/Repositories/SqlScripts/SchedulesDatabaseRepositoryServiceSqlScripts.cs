namespace BaringsQuartzUI.Repositories.SqlScripts;

internal static class SchedulesDatabaseRepositoryServiceSqlScripts
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
