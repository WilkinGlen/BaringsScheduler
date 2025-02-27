namespace BaringsQuartzUI.Components.Pages;

using BaringsQuartzUI.Models;
using BaringsQuartzUI.Repositories;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

public sealed partial class QuartzSchedules
{
    private List<QuartzJobDetail>? quartzJobDetails;

    [Inject]
    private IJobsDatabaseRepository? JobsDatabaseRepository { get; set; }

    [Inject]
    private ISchedulesDatabaseRepositoryService? SchedulesDatabaseRepositoryService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        this.quartzJobDetails = [.. await this.JobsDatabaseRepository!.GetAllJobsAsync()];
        var triggerDefinitions = await this.SchedulesDatabaseRepositoryService!.GetAllTriggerDefinitionsAsync();
        if(this.quartzJobDetails.Count > 0)
        {
            foreach (var job in this.quartzJobDetails)
            {
                job.Triggers = triggerDefinitions.Where(x =>
                    x.JobName == job.JobName &&
                    x.JobGroupName == job.JobGroup &&
                    x.JobClassName == job.JobClassName).ToList();
            }
        }
    }

    private async Task AddTrigger(QuartzJobDetail quartzJobDetail)
    {

    }
}
