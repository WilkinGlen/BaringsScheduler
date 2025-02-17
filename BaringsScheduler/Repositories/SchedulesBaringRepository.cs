namespace BaringsScheduler.Repositories;

using BaringsScheduler.Models;
using Serilog;

internal sealed class SchedulesBaringRepository
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
                    ScheduleName = "Every2Minute",
                    JobName = "JobNumber1",
                    JobDescription = "JobNumber1 description",
                    JobClassName = "BaringsJobScheduler.Jobs.JobNumber1, BaringsJobScheduler",
                    JobGroupName = "BaringsJobScheduler",
                    CronSchedule = "0 0/2 * * * ?"
                },
                new ()
                {
                    Id = 2,
                    ScheduleName = "Every3Minutes",
                    JobName = "JobNumber2",
                    JobDescription = "JobNumber2 description",
                    JobClassName = "BaringsJobScheduler.Jobs.JobNumber2, BaringsJobScheduler",
                    JobGroupName = "BaringsJobScheduler",
                    CronSchedule = "0 0/3 * * * ?"
                }
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SchedulesBaringRepository.GetAllTriggerDefinitionsAsync");
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
