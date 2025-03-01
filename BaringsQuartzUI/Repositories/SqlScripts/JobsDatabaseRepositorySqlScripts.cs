namespace BaringsQuartzUI.Repositories.SqlScripts;

internal static class JobsDatabaseRepositorySqlScripts
{
    internal const string GetAllJobsAsyncSql =
        @"SELECT
          	[JOB_NAME] AS JobName,
          	[JOB_GROUP] AS JobGroup,
          	[DESCRIPTION] AS [Description],
          	[JOB_CLASS_NAME] AS JobClassName
          FROM [dbo].[QRTZ_JOB_DETAILS]
          WHERE [JOB_NAME] <> 'SYNC_JOB_NAME'";

    internal const string GetAllJobRunResultsAsyncSql =
        @"WITH RankedLogs_CTE AS (
          	SELECT Id, GroupName, JobName, [Message], RunCompleted, ExceptionMessage,
          	ROW_NUMBER() OVER(
          		PARTITION BY GroupName, JobName
          		ORDER BY RunCompleted DESC) AS RowNum
          	FROM [dbo].[QuartzLogs]
          	WHERE GroupName <> 'SYNC_GROUP_NAME' AND JobName <> 'SYNC_JOB_NAME')
          
          SELECT J.[JOB_NAME] AS JobName, J.[JOB_GROUP] AS JobGroupName, RL.[Message] AS [Message], RL.[RunCompleted], RL.[RowNum] AS RowNum, RL.ExceptionMessage AS ExceptionMessage
          FROM [dbo].[QRTZ_JOB_DETAILS] AS J
          	LEFT JOIN RankedLogs_CTE AS RL ON J.JOB_GROUP = RL.GroupName AND J.JOB_NAME = RL.JobName
          WHERE (RL.RowNum IS NULL OR RL.RowNum <= 3)
          AND J.JOB_GROUP <> 'SYNC_GROUP_NAME' AND J.JOB_NAME <> 'SYNC_JOB_NAME'";
}
