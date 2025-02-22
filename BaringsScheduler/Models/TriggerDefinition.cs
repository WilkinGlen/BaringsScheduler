namespace BaringsScheduler.Models;

internal sealed class TriggerDefinition
{
    internal int Id { get; set; }

    internal string? ScheduleName { get; set; }

    internal string? ScheduleDescription { get; set; }

    internal string? JobName { get; set; }

    internal string? JobDescription { get; set; }

    internal string? JobClassName { get; set; }

    internal string? JobGroupName { get; set; }

    internal string? CronSchedule { get; set; }
}
