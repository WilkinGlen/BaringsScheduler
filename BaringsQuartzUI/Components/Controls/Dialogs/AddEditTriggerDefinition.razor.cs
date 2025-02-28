namespace BaringsQuartzUI.Components.Controls.Dialogs;

using BaringsQuartzUI.Services;

public sealed partial class AddEditTriggerDefinition
{
    private readonly List<ScheduleTypes> scheduleTypes = [.. Enum.GetValues<ScheduleTypes>()];
    private ScheduleTypes selectedScheduleType;
    private string? cronExpression;

    private void DailyExpressionBuilderTimeChangedHandler(TimeSpan time) =>
        this.cronExpression = CronExpressionBuilderService.BuildCronExpression(time);

    private void WeeklyExpressionBuilderDayTimeChangedHandler((DayOfWeek? DayOfWeek, TimeSpan? TimeSpan) values) =>
        this.cronExpression = CronExpressionBuilderService.BuildCronExpression(values.DayOfWeek!.Value, values.TimeSpan!.Value);

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
