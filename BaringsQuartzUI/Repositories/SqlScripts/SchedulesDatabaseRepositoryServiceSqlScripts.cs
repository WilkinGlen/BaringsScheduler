namespace BaringsQuartzUI.Repositories.SqlScripts;

internal static class SchedulesDatabaseRepositoryServiceSqlScripts
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

    internal const string InsertTriggerDefinitionAsyncSql =
        @"INSERT INTO [dbo].[TriggerDefinitions]([ScheduleName], [JobName], [JobDescription], [JobClassName], [JobGroupName], [CronSchedule])
          VALUES(@scheduleName, @jobName, @jobDescription, @jobClassName, @jobGroupName, @cronSchedule)
          SELECT @@IDENTITY AS Id";

    internal const string DeleteTriggerDefinitionAsyncSql =
        @"DELETE FROM [dbo].[TriggerDefinitions]
          WHERE [Id] = @id";

    internal const string InsertOneOffTriggerDefinitionAsyncSql =
        @"INSERT INTO [dbo].[OneOffTriggerDefinitions]([ScheduleName], [JobName], [JobDescription], [JobClassName], [JobGroupName])
          VALUES(@scheduleName, @jobName, @jobDescription, @jobClassName, @jobGroupName)";
}
