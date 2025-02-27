namespace BaringsQuartzUI.Repositories.SqlScripts;

public static class JobsDatabaseRepositorySqlScripts
{
    public const string GetAllJobsAsyncSql =
        @"SELECT
          	[JOB_NAME] AS JobName,
          	[JOB_GROUP] AS JobGroup,
          	[DESCRIPTION] AS [Description],
          	[JOB_CLASS_NAME] AS JobClassName
          FROM [dbo].[QRTZ_JOB_DETAILS]";
}
