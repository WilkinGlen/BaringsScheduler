namespace BaringsQuartzUI.Components.Pages;

using BaringsQuartzUI.Components.Controls.Dialogs;
using BaringsQuartzUI.Components.Controls.Dialogs.CommonDialogs;
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

    [Inject]
    private ICommonDialogsService? CommonDialogsService { get; set; }

    [Inject]
    private ISnackbar? Snackbar { get; set; }

    protected override async Task OnInitializedAsync() => await this.PopulateJobDetails();

    private static Func<JobHistoryItem, int, string> SucceededRowColour => (x, i) =>
        x.ExceptionMessage == null
            ? "background-color: rgba(100,255,100,0.5);"
            : "background-color: rgba(255,100,100,0.5);";

    private async Task PopulateJobDetails()
    {
        try
        {
            var jobsTask = this.JobsDatabaseRepository!.GetAllJobsAsync();
            var triggersTask = this.SchedulesDatabaseRepositoryService!.GetAllTriggerDefinitionsAsync();
            var tasks = new List<Task> { jobsTask, triggersTask };
            await Task.WhenAll(tasks);

            this.quartzJobDetails = [.. jobsTask.Result];
            this.triggerDefinitions = triggersTask.Result;
            if (this.quartzJobDetails.Count > 0 && this.triggerDefinitions.Any())
            {
                foreach (var job in this.quartzJobDetails)
                {
                    job.Triggers = [.. this.triggerDefinitions.Where(x =>
                    x.JobName == job.JobName &&
                    x.JobGroupName == job.JobGroup &&
                    x.JobClassName == job.JobClassName)];
                }
            }
        }
        catch
        {
            this.ShowErrorSnackbar();
        }
    }

    private async Task AddTrigger(QuartzJobDetail quartzJobDetail)
    {
        try
        {
            var dialogOptions = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, Position = DialogPosition.TopCenter, CloseButton = true };
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
                    _ = this.Snackbar!.Add($"Trigger {triggerDefinition.ScheduleName} added", Severity.Info);
                }
            }
        }
        catch
        {
            this.ShowErrorSnackbar();
        }
    }

    private async Task DeleteTrigger(TriggerDefinition triggerDefinition)
    {
        try
        {
            if (await this.CommonDialogsService!.GetConfirmationAsync(
                "Delete Trigger?",
                $"Are you sure you want to delete trigger <b>{triggerDefinition.ScheduleName}</b>?"))
            {
                await this.SchedulesDatabaseRepositoryService!.DeleteTriggerDefinitionAsync(triggerDefinition);
                var job = this.quartzJobDetails?.FirstOrDefault(x => x.JobName == triggerDefinition.JobName);
                _ = (job?.Triggers.Remove(triggerDefinition));
                _ = this.Snackbar!.Add($"Trigger {triggerDefinition.ScheduleName} deleted", Severity.Info);
            }
        }
        catch
        {
            this.ShowErrorSnackbar();
        }
    }

    private async Task AddOneOffTrigger(QuartzJobDetail quartzJobDetail)
    {
        try
        {
            if (quartzJobDetail != null)
            {
                if (await this.SchedulesDatabaseRepositoryService!.JobHasNotCompletedOneOffScheduleAsync(quartzJobDetail))
                {
                    _ = this.Snackbar!.Add($"Job {quartzJobDetail.JobName} already has a not completed one-off schedule", Severity.Warning);
                    return;
                }

                var oneOffTrigger = new TriggerDefinition
                {
                    JobName = quartzJobDetail.JobName,
                    JobDescription = quartzJobDetail.Description,
                    JobClassName = quartzJobDetail.JobClassName,
                    JobGroupName = quartzJobDetail.JobGroup
                };
                await this.SchedulesDatabaseRepositoryService!.InsertOneOffTriggerDefinitionAsync(oneOffTrigger);
                _ = this.Snackbar!.Add($"One-off schedule for job {quartzJobDetail.JobName} added", Severity.Info);
            }
        }
        catch
        {
            this.ShowErrorSnackbar();
        }
    }

    private async Task ShowHistory(QuartzJobDetail quartzJobDetail)
    {
        try
        {
            if (quartzJobDetail != null)
            {
                if (quartzJobDetail?.JobHistory?.Count > 0)
                {
                    quartzJobDetail.JobHistory = [];
                    return;
                }

                if (this.quartzJobDetails != null)
                {
                    foreach (var job in this.quartzJobDetails)
                    {
                        job.JobHistory = [];
                    }
                }

                var jobHistory = await this.SchedulesDatabaseRepositoryService!.GetJobHistoryAsync(quartzJobDetail!);
                quartzJobDetail!.JobHistory = [.. jobHistory];
            }
        }
        catch
        {
            this.ShowErrorSnackbar();
        }
    }

    private void ShowErrorSnackbar() =>
        _ = this.Snackbar!.Add("An error has occurred", Severity.Error);
}
