namespace BaringsQuartzUI.Repositories;

using BaringsQuartzUI.Models;
using BaringsQuartzUI.Repositories.SqlScripts;
using Dapper;
using Microsoft.Data.SqlClient;

public interface ISchedulesDatabaseRepositoryService
{
    Task<IEnumerable<TriggerDefinition>> GetAllTriggerDefinitionsAsync();

    Task<TriggerDefinition> InsertTriggerDefinitionAsync(TriggerDefinition triggerDefinition);

    Task DeleteTriggerDefinitionAsync(TriggerDefinition triggerDefinition);

    Task InsertOneOffTriggerDefinitionAsync(TriggerDefinition triggerDefinition);
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

    public async Task<TriggerDefinition> InsertTriggerDefinitionAsync(TriggerDefinition triggerDefinition)
    {
        var sql = SchedulesDatabaseRepositoryServiceSqlScripts.InsertTriggerDefinitionAsyncSql;
        var parameters = new DynamicParameters();
        parameters.Add("@scheduleName", triggerDefinition.ScheduleName);
        parameters.Add("@jobName", triggerDefinition.JobName);
        parameters.Add("@jobDescription", triggerDefinition.JobDescription);
        parameters.Add("@jobClassName", triggerDefinition.JobClassName);
        parameters.Add("@jobGroupName", triggerDefinition.JobGroupName);
        parameters.Add("@cronSchedule", triggerDefinition.CronSchedule);
        var connection = new SqlConnection(configuration.GetConnectionString("SchedulerDatabaseConnectionString"));
        triggerDefinition.Id = await connection.QuerySingleAsync<int>(sql, parameters);
        return triggerDefinition;
    }

    public async Task DeleteTriggerDefinitionAsync(TriggerDefinition triggerDefinition)
    {
        var sql = SchedulesDatabaseRepositoryServiceSqlScripts.DeleteTriggerDefinitionAsyncSql;
        var connection = new SqlConnection(configuration.GetConnectionString("SchedulerDatabaseConnectionString"));
        _ = await connection.ExecuteAsync(sql, new { triggerDefinition.Id });
    }

    public async Task InsertOneOffTriggerDefinitionAsync(TriggerDefinition triggerDefinition)
    {
        var sql = SchedulesDatabaseRepositoryServiceSqlScripts.InsertOneOffTriggerDefinitionAsyncSql;
        var parameters = new DynamicParameters();
        parameters.Add("@scheduleName", "OneOffTrigger");
        parameters.Add("@jobName", triggerDefinition.JobName);
        parameters.Add("@jobDescription", triggerDefinition.JobDescription);
        parameters.Add("@jobClassName", triggerDefinition.JobClassName);
        parameters.Add("@jobGroupName", triggerDefinition.JobGroupName);
        var connection = new SqlConnection(configuration.GetConnectionString("SchedulerDatabaseConnectionString"));
        _ = await connection.ExecuteAsync(sql, parameters);
    }
}
