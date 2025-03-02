namespace BaringsQuartzUI.Models;

public sealed class JobHistoryItem
{
    public int Id { get; set; }

    public DateTime RunCompleted { get; set; }

    public string? JobName { get; set; }

    public string? TriggerName { get; set; }

    public string? Message { get; set; }

    public string? ExceptionMessage { get; set; }
}
