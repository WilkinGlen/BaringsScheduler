namespace BaringsScheduler.Repositories.SqlScripts;

internal static class BaringsSchedulesRepositorySqlScripts
{
    internal const string GetAllTriggerDefinitionsAsyncSql =
        @"SELECT
          	[Id],
          	[ScheduleName],
          	[JobName],
          	[JobDescription],
          	[JobClassName],
          	[JobGroupName],
          	[CronSchedule]
          FROM [dbo].[TriggerDefinitions]";

    internal const string GetAllOneOffTriggerDefinitionsAsyncSql =
        @"SELECT 
          	[Id], 
          	[ScheduleName], 
          	[JobName], 
          	[JobDescription], 
          	[JobClassName], 
          	[JobGroupName]
          FROM [dbo].[OneOffTriggerDefinitions]
          WHERE JobCompleted IS NULL";
}
