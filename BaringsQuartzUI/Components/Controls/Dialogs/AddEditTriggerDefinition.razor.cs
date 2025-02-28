namespace BaringsQuartzUI.Components.Controls.Dialogs;

using BaringsQuartzUI.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

public sealed partial class AddEditTriggerDefinition
{
    private readonly List<ScheduleTypes> scheduleTypes = [.. Enum.GetValues<ScheduleTypes>()];
    private ScheduleTypes selectedScheduleType;
    private string? cronExpression;
    private string? scheduleName;

    [CascadingParameter]
    private IMudDialogInstance? MudDialog { get; set; }

    private bool SubmitButtonDisabled =>
        string.IsNullOrWhiteSpace(this.scheduleName) || string.IsNullOrWhiteSpace(this.cronExpression);

    private void DailyExpressionBuilderTimeChangedHandler(TimeSpan time) =>
        this.cronExpression = CronExpressionBuilderService.BuildCronExpression(time);

    private void WeeklyExpressionBuilderDayTimeChangedHandler((DayOfWeek? DayOfWeek, TimeSpan? TimeSpan) values) =>
        this.cronExpression = CronExpressionBuilderService.BuildCronExpression(values.DayOfWeek!.Value, values.TimeSpan!.Value);

    private void MultipleDaysExpressionBuilderDaysTimeChangedHandler((TimeSpan TimeSpan, DayOfWeek[] DaysOfTheWeek) values) =>
        this.cronExpression = CronExpressionBuilderService.BuildCronExpression(values.TimeSpan, values.DaysOfTheWeek);

    private void MonthlyExpressionBuilderDayTimeChangedHandler((int Day, TimeSpan Time) values) =>
        this.cronExpression = CronExpressionBuilderService.BuildCronExpression(values.Day, values.Time);

    private void PeriodExpressionBuilderHoursMinutesChangedHandler((int Hour, int Minute) values) =>
        this.cronExpression = CronExpressionBuilderService.BuildCronExpression(values.Hour, values.Minute);

    private void SubmitClickedHandler()
    {
        if (!string.IsNullOrWhiteSpace(this.cronExpression) && !string.IsNullOrWhiteSpace(this.scheduleName))
        {
            this.MudDialog!.Close(DialogResult.Ok((this.scheduleName, this.cronExpression)));
        }
    }

    private void CancelClickedHandler()
    {
        this.cronExpression = null;
        this.scheduleName = null;
        this.MudDialog!.Close(DialogResult.Cancel());
    }

    private enum ScheduleTypes
    {
        Select,
        Daily,
        MultipleDays,
        Weekly,
        Monthly,
        Periodically
    }
}
