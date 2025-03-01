namespace BaringsQuartzUI.Components.Controls;

using BaringsQuartzUI.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

public sealed partial class LastRunsIcons
{
    [Parameter]
    public QuartzJobDetail? JobDetail { get; set; }

    private string? lastExecutionDateTime => this.JobDetail?.LastRunResult?.ResultDateTime.ToString("dd/MM/yyyy HH:mm:ss");

    private (string? Icon, Color IconColor) GetLastIcon()
    {
        var icon = this.JobDetail?.LastRunResult != null ?
            this.JobDetail?.LastRunResult?.ResultStatus == true ? 
            Icons.Material.Filled.Done : 
            Icons.Material.Filled.Clear : 
            null;
        var iconColour = this.JobDetail?.LastRunResult != null ?
            this.JobDetail?.LastRunResult?.ResultStatus == true ?
            Color.Success :
            Color.Secondary :
            Color.Transparent;
        return (icon, iconColour);
    }

    private (string? Icon, Color IconColor) GetSecondLastIcon()
    {
        var icon = this.JobDetail?.SecondRunResult != null ?
            this.JobDetail?.LastRunResult?.ResultStatus == true ?
            Icons.Material.Filled.Done :
            Icons.Material.Filled.Clear :
            null;
        var iconColour = this.JobDetail?.SecondRunResult != null ?
            this.JobDetail?.LastRunResult?.ResultStatus == true ?
            Color.Success :
            Color.Secondary :
            Color.Transparent;
        return (icon, iconColour);
    }

    private (string? Icon, Color IconColor) GetThirdLastIcon()
    {
        var icon = this.JobDetail?.ThirdRunResult != null ?
            this.JobDetail?.LastRunResult?.ResultStatus == true ?
            Icons.Material.Filled.Done :
            Icons.Material.Filled.Clear :
            null;
        var iconColour = this.JobDetail?.ThirdRunResult != null ?
            this.JobDetail?.LastRunResult?.ResultStatus == true ?
            Color.Success :
            Color.Secondary :
            Color.Transparent;
        return (icon, iconColour);
    }
}
