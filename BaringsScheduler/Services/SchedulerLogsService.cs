namespace BaringsScheduler.Services;

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
    }
}
