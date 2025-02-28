namespace BaringsScheduler.Repositories;

using BaringsScheduler.Models;
using BaringsScheduler.Repositories.SqlScripts;
using Dapper;
using Microsoft.Data.SqlClient;
using Serilog;

internal sealed class BaringsSchedulesRepository(string connectionString)
{
    internal async Task<IEnumerable<TriggerDefinition>> GetAllTriggerDefinitionsAsync()
    {
        try
        {
            var sql = BaringsSchedulesRepositorySqlScripts.GetAllTriggerDefinitionsAsyncSql;
            var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<TriggerDefinition>(sql);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    internal async Task<IEnumerable<TriggerDefinition>> GetAllOneOffTriggerDefinitionsAsync()
    {
        try
        {
            return await Task.FromResult(new List<TriggerDefinition>()
            {
                new ()
                {
                    Id = 1,
                    ScheduleName = "OneOff",
                    ScheduleDescription = "OneOff",
                    JobName = "JobNumber3",
                    JobDescription = "JobNumber3 description",
                    JobClassName = "BaringsJobScheduler.Jobs.JobNumber3, BaringsJobScheduler",
                    JobGroupName = "BaringsJobScheduler"
                }
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SchedulesBaringRepository.GetAllOneOffTriggerDefinitionsAsync");
            throw;
        }
    }
}
