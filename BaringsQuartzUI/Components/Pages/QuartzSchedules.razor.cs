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
    private IEnumerable<TriggerDefinition>? triggerDefinitions;

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
        triggerDefinitions = triggersTask.Result;
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
        var dialogOptions = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, Position = DialogPosition.TopCenter };
        var dialog = await this.DialogService!.ShowAsync<AddEditTriggerDefinition>("Add Trigger", options: dialogOptions);
        var result = await dialog.Result;
        if (result?.Data != null)
        {
            var (NewScheduleName, NewCronSchedule) = ((string ScheduleName, string CronSchedule))result?.Data!;

            if (this.triggerDefinitions?.FirstOrDefault(x => x.ScheduleName == NewScheduleName && x.JobName == quartzJobDetail.JobName) == null)
            {
                var triggerDefinition = new TriggerDefinition
                {
                    ScheduleName = NewScheduleName,
                    JobName = quartzJobDetail.JobName,
                    JobGroupName = quartzJobDetail.JobGroup,
                    JobClassName = quartzJobDetail.JobClassName,
                    CronSchedule = NewCronSchedule
                };
                triggerDefinition.Id = (await this.SchedulesDatabaseRepositoryService!.InsertTriggerDefinitionAsync(triggerDefinition)).Id;
                quartzJobDetail.Triggers.Add(triggerDefinition);
            }
        }
    }

    private async Task DeleteTrigger(TriggerDefinition triggerDefinition)
    {
        await this.SchedulesDatabaseRepositoryService!.DeleteTriggerDefinitionAsync(triggerDefinition);
        var job = this.quartzJobDetails?.FirstOrDefault(x => x.JobName == triggerDefinition.JobName);
        _ = (job?.Triggers.Remove(triggerDefinition));
    }
}
