namespace BaringsQuartzUI.Models;

public sealed class JobRunResult
{
    public DateTime ResultDateTime { get; set; }

    public string? ResultMessage { get; set; }

    public bool? ResultStatus { get; set; }
}
