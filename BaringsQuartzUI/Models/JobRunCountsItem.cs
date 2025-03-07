namespace BaringsQuartzUI.Models;

public sealed class JobRunCountsItem
{
    public string? GroupName { get; set; }

    public string? JobName { get; set; }

    public int SuccessCount { get; set; }

    public int ErrorCount { get; set; }
}
