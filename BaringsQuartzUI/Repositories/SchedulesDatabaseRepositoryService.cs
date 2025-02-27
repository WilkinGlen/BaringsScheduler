namespace BaringsQuartzUI.Repositories;

using BaringsQuartzUI.Models;
using BaringsQuartzUI.Repositories.SqlScripts;
using Dapper;
using Microsoft.Data.SqlClient;

public interface ISchedulesDatabaseRepositoryService
{
    Task<IEnumerable<TriggerDefinition>> GetAllTriggerDefinitionsAsync();
}

public sealed class SchedulesDatabaseRepositoryService(IConfiguration configuration) : ISchedulesDatabaseRepositoryService
{
    public async Task<IEnumerable<TriggerDefinition>> GetAllTriggerDefinitionsAsync()
    {
        try
        {
            var sql = SchedulesDatabaseRepositoryServiceSqlScripts.GetAllTriggerDefinitionsAsyncSql;
            var connection = new SqlConnection(configuration.GetConnectionString("SchedulerDatabaseConnectionString"));
            return await connection.QueryAsync<TriggerDefinition>(sql);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
}
