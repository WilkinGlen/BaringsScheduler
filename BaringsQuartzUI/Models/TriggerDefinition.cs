namespace BaringsQuartzUI.Models;

public sealed class TriggerDefinition
{
    public int Id { get; set; }

    public string? ScheduleName { get; set; }

    public string? JobName { get; set; }

    public string? JobDescription { get; set; }

    public string? JobClassName { get; set; }

    public string? JobGroupName { get; set; }

    public string? CronSchedule { get; set; }
}
