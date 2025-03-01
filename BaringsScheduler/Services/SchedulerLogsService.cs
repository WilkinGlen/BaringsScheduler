namespace BaringsScheduler.Services;

using BaringsScheduler.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Quartz;

internal static class SchedulerLogsService
{
    internal static async Task LogJobExecutionAsync(IJobExecutionContext jobExecutionContext, string? message, string quartzDatabaseConnectionString)
    {
        var sql = @"INSERT INTO [dbo].[QuartzLogs]([Succeeded], [RunCompleted], [GroupName], [JobName], [TriggerName], [Message], [ExceptionMessage])
                    VALUES(@succeeded, @runCompleted, @groupName, @jobName, @triggerName, @message, @exceptionMessage)";
        var connection = new SqlConnection(quartzDatabaseConnectionString);
        var parameters = new
        {
            succeeded = true,
            runCompleted = DateTime.UtcNow,
            groupName = jobExecutionContext.JobDetail.Key.Group,
            jobName = jobExecutionContext.JobDetail.Key.Name,
            triggerName = jobExecutionContext.Trigger.Key.Name,
            message,
            exceptionMessage = (string?)null
        };

        _ = await connection.ExecuteAsync(sql, parameters);

        if (jobExecutionContext.Trigger.Key.Name.Contains(Constants.OneOffTriggerName))
        {
            await LogOneOffAsCompletedAsync(jobExecutionContext);
        }
    }

    internal static async Task LogJobFailureAsync(IJobExecutionContext jobExecutionContext, string? message, string? exceptionMessage, string quartzDatabaseConnectionString)
    {
        var sql = @"INSERT INTO [dbo].[QuartzLogs]([Succeeded], [RunCompleted], [GroupName], [JobName], [TriggerName], [Message], [ExceptionMessage])
                    VALUES(@succeeded, @runCompleted, @groupName, @jobName, @triggerName, @message, @exceptionMessage)";
        var connection = new SqlConnection(quartzDatabaseConnectionString);
        var parameters = new
        {
            succeeded = false,
            runCompleted = DateTime.UtcNow,
            groupName = jobExecutionContext.JobDetail.Key.Group,
            jobName = jobExecutionContext.JobDetail.Key.Name,
            triggerName = jobExecutionContext.Trigger.Key.Name,
            message,
            exceptionMessage
        };

        _ = await connection.ExecuteAsync(sql, parameters);

        if (jobExecutionContext.Trigger.Key.Name.Contains(Constants.OneOffTriggerName))
        {
            await LogOneOffAsCompletedAsync(jobExecutionContext);
        }
    }

    private static async Task LogOneOffAsCompletedAsync(IJobExecutionContext jobExecutionContext)
    {
        var sql = @"UPDATE [dbo].[OneOffTriggerDefinitions]
                    SET JobCompleted = GETUTCDATE()
                    WHERE ScheduleName = @scheduleName 
                    AND JobName = @jobName
                    AND JobGroupName = @jobGroupName
                    AND JobCompleted IS NULL";
        var connection = new SqlConnection(Constants.SchedulerDatabaseConnectionString);
        var parameters = new
        {
            scheduleName = jobExecutionContext.Trigger.Key.Name,
            jobName = jobExecutionContext.JobDetail.Key.Name,
            jobGroupName = jobExecutionContext.JobDetail.Key.Group
        };
        _ = await connection.ExecuteAsync(sql, parameters);
    }
}
