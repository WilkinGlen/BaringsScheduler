namespace BaringsQuartzUI.Repositories;

using BaringsQuartzUI.Models;
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
        return await connection.QueryAsync<QuartzJobDetail>(sql);
    }
}
