namespace BaringsQuartzUI.Components.Pages;

using BaringsQuartzUI.Models;
using BaringsQuartzUI.Repositories;
using Microsoft.AspNetCore.Components;
using MudBlazor;

public sealed partial class Home
{
    private IEnumerable<JobRunCountsItem>? jobRunCounts;
    private readonly ChartOptions ChartOptions = new() { ChartPalette = ["red", "green"] };

    [Inject]
    private IJobsDatabaseRepository? JobsDatabaseRepository { get; set; }

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            this.jobRunCounts = await this.JobsDatabaseRepository!.GetJobRunCountsAsync();
            this.StateHasChanged();
        }
    }
        
    private static double[] GetData(JobRunCountsItem item) =>
        [item.ErrorCount, item.SuccessCount];

    private static string[] GetLabels(JobRunCountsItem item) =>
        [$"Failed {item.ErrorCount}", $"Succeeded {item.SuccessCount}"];

    private static double GetTotal(JobRunCountsItem item) =>
        item.ErrorCount + item.SuccessCount;
}
