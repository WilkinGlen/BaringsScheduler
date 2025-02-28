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

    internal const string InsertTriggerDefinitionAsyncSql =
        @"INSERT INTO [dbo].[TriggerDefinitions]([ScheduleName], [JobName], [JobDecription], [JobClassName], [JobGroupName], [CronSchedule])
          VALUES(@scheduleName, @jobName, @jobDecription, @jobClassName, @jobGroupName, @cronSchedule)
          SELECT @@IDENTITY AS Id";

    internal const string DeleteTriggerDefinitionAsyncSql =
        @"DELETE FROM [dbo].[TriggerDefinitions]
          WHERE [Id] = @id";
}
