namespace BaringsQuartzUI.Services;

using Quartz;
using Quartz.Impl.Triggers;

public static class CronExpressionBuilderService
{
    public static string? BuildCronExpression(TimeSpan timeSpan) =>
        timeSpan != TimeSpan.Zero
            ? ((CronTriggerImpl)CronScheduleBuilder
                .DailyAtHourAndMinute(timeSpan.Hours, timeSpan.Minutes)
                .Build())
                .CronExpressionString
            : null;

    public static string? BuildCronExpression(DayOfWeek dayOfWeek, TimeSpan timeSpan) =>
        timeSpan != TimeSpan.Zero
            ? ((CronTriggerImpl)CronScheduleBuilder
                .WeeklyOnDayAndHourAndMinute(dayOfWeek, timeSpan.Hours, timeSpan.Minutes)
                .Build())
                .CronExpressionString
            : null;

    public static string? BuildCronExpression(TimeSpan timeSpan, params DayOfWeek[] daysOfWeek) =>
        timeSpan != TimeSpan.Zero && daysOfWeek.Length > 0
            ? ((CronTriggerImpl)CronScheduleBuilder
                .AtHourAndMinuteOnGivenDaysOfWeek(timeSpan.Hours, timeSpan.Minutes, daysOfWeek)
                .Build())
                .CronExpressionString
            : null;

    public static string? BuildCronExpression(int day, TimeSpan timeSpan) =>
        timeSpan != TimeSpan.Zero && day > 0
            ? ((CronTriggerImpl)CronScheduleBuilder
                .MonthlyOnDayAndHourAndMinute(day, timeSpan.Hours, timeSpan.Minutes)
                .Build())
                .CronExpressionString
            : null;

    public static string? BuildCronExpression(int hour, int minute) =>
        hour is > 0 and < 24 ? $"0 0 0/{hour} * * ?" :
        minute is > 0 and < 60 ? $"0 0/{minute} * * * ?" :
        null;
}
