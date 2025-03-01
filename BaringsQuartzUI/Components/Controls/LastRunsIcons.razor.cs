namespace BaringsQuartzUI.Components.Controls;

using BaringsQuartzUI.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

public sealed partial class LastRunsIcons
{
    [Parameter]
    public QuartzJobDetail? JobDetail { get; set; }

    private string? LastExecutionDateTime => this.JobDetail?.LastRunResult?.ResultDateTime != null 
        ? $"({this.JobDetail?.LastRunResult?.ResultDateTime.ToString("dd/MM HH:mm:ss")})" 
        : null;

    private (string? Icon, Color IconColor, string? Message) GetLastIcon()
    {
        var icon = this.JobDetail?.LastRunResult != null
            ? this.JobDetail?.LastRunResult?.ResultStatus == true
            ? Icons.Material.Filled.Done
            : Icons.Material.Filled.Clear
            : null;
        var iconColour = this.JobDetail?.LastRunResult != null
            ? this.JobDetail?.LastRunResult?.ResultStatus == true
            ? Color.Success
            : Color.Secondary
            : Color.Transparent;
        var message = $"{this.JobDetail?.LastRunResult?.ResultMessage}: {this.JobDetail?.LastRunResult?.ResultDateTime.ToString("dd/MM HH:mm:ss")}";
        return (icon, iconColour, message);
    }

    private (string? Icon, Color IconColor, string? Message) GetSecondLastIcon()
    {
        var icon = this.JobDetail?.SecondRunResult != null
            ? this.JobDetail?.SecondRunResult?.ResultStatus == true
            ? Icons.Material.Filled.Done
            : Icons.Material.Filled.Clear
            : null;
        var iconColour = this.JobDetail?.SecondRunResult != null
            ? this.JobDetail?.SecondRunResult?.ResultStatus == true
            ? Color.Success
            : Color.Secondary
            : Color.Transparent;
        var message = $"{this.JobDetail?.SecondRunResult?.ResultMessage}: {this.JobDetail?.LastRunResult?.ResultDateTime.ToString("dd/MM HH:mm:ss")}";
        return (icon, iconColour, message);
    }

    private (string? Icon, Color IconColor, string? Message) GetThirdLastIcon()
    {
        var icon = this.JobDetail?.ThirdRunResult != null
            ? this.JobDetail?.ThirdRunResult?.ResultStatus == true
            ? Icons.Material.Filled.Done
            : Icons.Material.Filled.Clear
            : null;
        var iconColour = this.JobDetail?.ThirdRunResult != null
            ? this.JobDetail?.ThirdRunResult?.ResultStatus == true
            ? Color.Success
            : Color.Secondary
            : Color.Transparent;
        var message = $"{this.JobDetail?.ThirdRunResult?.ResultMessage}: {this.JobDetail?.LastRunResult?.ResultDateTime.ToString("dd/MM HH:mm:ss")}";
        return (icon, iconColour, message);
    }
}
