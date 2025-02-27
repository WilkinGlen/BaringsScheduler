namespace BaringsQuartzUI.Models;

public sealed class QuartzJobDetail
{
    public string? JobName { get; set; }

    public string? JobGroup { get; set; }

    public string? Description { get; set; }

    public string? JobClassName { get; set; }

    public List<TriggerDefinition> Triggers { get; set; } = [];
}
