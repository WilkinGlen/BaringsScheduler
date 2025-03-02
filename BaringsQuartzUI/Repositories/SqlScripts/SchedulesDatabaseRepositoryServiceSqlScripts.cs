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

    internal const string JobHasNotCompletedOneOffScheduleAsyncSql =
        @"SELECT COUNT(0) 
          FROM [dbo].[OneOffTriggerDefinitions] 
          WHERE [JobName] = @jobName 
          AND [JobGroupName] = @jobGroupName 
          AND [JobCompleted] IS NULL";

    internal const string GetJobHistoryAsyncSql =
        @"SELECT
            [Id],
            [RunCompleted],
            [JobName],
            [Message],
            [ExceptionMessage]
          FROM [dbo].[QuartzLogs]
          WHERE JobName = @jobName
          AND GroupName = @groupName
          ORDER BY [RunCompleted] DESC";
}
