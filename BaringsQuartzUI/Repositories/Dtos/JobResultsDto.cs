namespace BaringsQuartzUI.Repositories.Dtos;

public sealed class JobResultsDto
{
    public string? JobName { get; set; }

    public string? JobGroupName { get; set; }

    public string? Message { get; set; }

    public DateTime RunCompleted { get; set; }

    public int RowNum { get; set; }

    public string? ExceptionMessage { get; set; }
}
