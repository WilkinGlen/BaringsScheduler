namespace BaringsQuartzUI.Repositories;

using BaringsQuartzUI.Models;
using BaringsQuartzUI.Repositories.Dtos;
using BaringsQuartzUI.Repositories.SqlScripts;
using Dapper;
using Microsoft.Data.SqlClient;

public interface IJobsDatabaseRepository
{
    Task<IEnumerable<QuartzJobDetail>> GetAllJobsAsync();
}

public sealed class JobsDatabaseRepository(IConfiguration configuration) : IJobsDatabaseRepository
{
    public async Task<IEnumerable<QuartzJobDetail>> GetAllJobsAsync()
    {
        var sql = JobsDatabaseRepositorySqlScripts.GetAllJobsAsyncSql;
        var connection = new SqlConnection(configuration.GetConnectionString("QuartzDatabaseConnectionString"));
        var jobDetails = await connection.QueryAsync<QuartzJobDetail>(sql);
        var runResults = await this.GetAllJobRunResultsAsync();
        PopulateResults(jobDetails, runResults);

        return jobDetails;
    }

    private static void PopulateResults(IEnumerable<QuartzJobDetail> jobDetails, IEnumerable<JobResultsDto> runResults)
    {
        foreach (var jobDetail in jobDetails)
        {
            PopulateLastResult(runResults, jobDetail);
            PopulateSecondResult(runResults, jobDetail);
            PopulateThirdResult(runResults, jobDetail);
        }
    }

    private static void PopulateLastResult(IEnumerable<JobResultsDto> runResults, QuartzJobDetail jobDetail)
    {
        var lastResult = runResults.FirstOrDefault(x =>
                        x.JobName == jobDetail.JobName &&
                        x.JobGroupName == jobDetail.JobGroup &&
                        x.RowNum == 1);
        if (lastResult != null)
        {
            jobDetail.LastRunResult = new()
            {
                ResultDateTime = lastResult.RunCompleted,
                ResultMessage = lastResult.ExceptionMessage ?? lastResult.Message,
                ResultStatus = string.IsNullOrWhiteSpace(lastResult.ExceptionMessage)
            };
        }
    }

    private static void PopulateSecondResult(IEnumerable<JobResultsDto> runResults, QuartzJobDetail jobDetail)
    {
        var secondLastResult = runResults.FirstOrDefault(x =>
                        x.JobName == jobDetail.JobName &&
                        x.JobGroupName == jobDetail.JobGroup &&
                        x.RowNum == 2);
        if (secondLastResult != null)
        {
            jobDetail.SecondRunResult = new()
            {
                ResultDateTime = secondLastResult.RunCompleted,
                ResultMessage = secondLastResult.ExceptionMessage ?? secondLastResult.Message,
                ResultStatus = string.IsNullOrWhiteSpace(secondLastResult.ExceptionMessage)
            };
        }
    }

    private static void PopulateThirdResult(IEnumerable<JobResultsDto> runResults, QuartzJobDetail jobDetail)
    {
        var thirdLastResult = runResults.FirstOrDefault(x =>
                        x.JobName == jobDetail.JobName &&
                        x.JobGroupName == jobDetail.JobGroup &&
                        x.RowNum == 3);
        if (thirdLastResult != null)
        {
            jobDetail.ThirdRunResult = new()
            {
                ResultDateTime = thirdLastResult.RunCompleted,
                ResultMessage = thirdLastResult.ExceptionMessage ?? thirdLastResult.Message,
                ResultStatus = string.IsNullOrWhiteSpace(thirdLastResult.ExceptionMessage)
            };
        }
    }

    private async Task<IEnumerable<JobResultsDto>> GetAllJobRunResultsAsync()
    {
        var sql = JobsDatabaseRepositorySqlScripts.GetAllJobRunResultsAsyncSql;
        var connection = new SqlConnection(configuration.GetConnectionString("QuartzDatabaseConnectionString"));
        return await connection.QueryAsync<JobResultsDto>(sql);
    }
}
