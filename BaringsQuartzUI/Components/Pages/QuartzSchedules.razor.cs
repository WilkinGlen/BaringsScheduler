namespace BaringsQuartzUI.Components.Pages;

using BaringsQuartzUI.Components.Controls.Dialogs;
using BaringsQuartzUI.Models;
using BaringsQuartzUI.Repositories;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Threading.Tasks;

public sealed partial class QuartzSchedules
{
    private List<QuartzJobDetail>? quartzJobDetails;

    [Inject]
    private IJobsDatabaseRepository? JobsDatabaseRepository { get; set; }

    [Inject]
    private ISchedulesDatabaseRepositoryService? SchedulesDatabaseRepositoryService { get; set; }

    [Inject]
    private IDialogService? DialogService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await this.PopulateJobDetails();
    }

    private async Task PopulateJobDetails()
    {
        var jobsTask = this.JobsDatabaseRepository!.GetAllJobsAsync();
        var triggersTask = this.SchedulesDatabaseRepositoryService!.GetAllTriggerDefinitionsAsync();
        var tasks = new List<Task> { jobsTask, triggersTask };
        await Task.WhenAll(tasks);

        this.quartzJobDetails = [.. jobsTask.Result];
        var triggerDefinitions = triggersTask.Result;
        if (this.quartzJobDetails.Count > 0 && triggerDefinitions.Any())
        {
            foreach (var job in this.quartzJobDetails)
            {
                job.Triggers = [.. triggerDefinitions.Where(x =>
                    x.JobName == job.JobName &&
                    x.JobGroupName == job.JobGroup &&
                    x.JobClassName == job.JobClassName)];
            }
        }
    }

    private async Task AddTrigger(QuartzJobDetail quartzJobDetail)
    {
        var dialog = await this.DialogService!.ShowAsync<AddEditTriggerDefinition>("Add Trigger");
        //var triggerDefinition = new TriggerDefinition
        //{
        //    ScheduleName = "Every3Minutes",
        //    JobName = quartzJobDetail.JobName,
        //    JobGroupName = quartzJobDetail.JobGroup,
        //    JobClassName = quartzJobDetail.JobClassName,
        //    CronSchedule = "0 0/3 * * * ?"
        //};
        //triggerDefinition.Id = (await this.SchedulesDatabaseRepositoryService!.InsertTriggerDefinitionAsync(triggerDefinition)).Id;
        //quartzJobDetail.Triggers.Add(triggerDefinition);
    }
}
