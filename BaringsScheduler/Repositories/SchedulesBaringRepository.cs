namespace BaringsScheduler.Repositories;

using BaringsScheduler.Models;
using Serilog;

internal class SchedulesBaringRepository
{
    internal async Task<IEnumerable<TriggerDefinition>> GetAllTriggerDefinitionsAsync()
    {
        try
        {
            return await Task.FromResult(new List<TriggerDefinition>()
            {
                new ()
                {
                    Id = 1,
                    ScheduleName = "Every2Minutes",
                    JobName = "JobNumber1",
                    JobDescription = "JobNumber1 description",
                    JobClassName = "BaringsJobScheduler.Jobs.JobNumber1, BaringsJobScheduler",
                    JobGroupName = "GroupNumber1",
                    CronSchedule = "0 0/2 * * * ?"
                },
                new ()
                {
                    Id = 2,
                    ScheduleName = "Every5Minutes",
                    JobName = "JobNumber2",
                    JobDescription = "JobNumber2 description",
                    JobClassName = "BaringsJobScheduler.Jobs.JobNumber2, BaringsJobScheduler",
                    JobGroupName = "GroupNumber1",
                    CronSchedule = "0 0/5 * * * ?"
                }
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SchedulesBaringRepository.GetAllTriggerDefinitionsAsync");
            throw;
        }
    }
}
