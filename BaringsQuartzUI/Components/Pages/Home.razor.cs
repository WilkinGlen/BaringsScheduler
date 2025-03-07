namespace BaringsQuartzUI.Components.Pages;

using BaringsQuartzUI.Models;
using BaringsQuartzUI.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

public sealed partial class Home : IDisposable
{
    private IEnumerable<JobRunCountsItem>? jobRunCounts;
    private readonly ChartOptions ChartOptions = new() { ChartPalette = ["red", "green"] };
    private DotNetObjectReference<Home>? dotNetObjectReference;

    [Inject]
    private IJobsDatabaseRepository? JobsDatabaseRepository { get; set; }

    [Inject]
    private IJSRuntime? JsRuntime { get; set; }

    protected override void OnInitialized() =>
        this.dotNetObjectReference = DotNetObjectReference.Create(this);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await this.RefreshJobRunCounts();
            await this.JsRuntime!.InvokeVoidAsync("refreshJobRunCounts", this.dotNetObjectReference);
            this.StateHasChanged();
        }
    }

    private static double[] GetData(JobRunCountsItem item) =>
        [item.ErrorCount, item.SuccessCount];

    private static string[] GetLabels(JobRunCountsItem item) =>
        [$"Failed {item.ErrorCount}", $"Succeeded {item.SuccessCount}"];

    private static double GetTotal(JobRunCountsItem item) =>
        item.ErrorCount + item.SuccessCount;

    [JSInvokable]
    public async Task RefreshJobRunCounts()
    {
        this.jobRunCounts = await this.JobsDatabaseRepository!.GetJobRunCountsAsync();
        this.StateHasChanged();
    }

    public void Dispose() =>
        this.dotNetObjectReference?.Dispose();
}
