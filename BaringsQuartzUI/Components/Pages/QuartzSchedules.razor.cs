namespace BaringsQuartzUI.Components.Pages;

using BaringsQuartzUI.Models;
using BaringsQuartzUI.Repositories;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

public sealed partial class QuartzSchedules
{
    private List<QuartzJobDetail>? quartzJobDetails;

    [Inject]
    private IJobsDatabaseRepository? JobsDatabaseRepository { get; set; }

    protected override async Task OnInitializedAsync() =>
        this.quartzJobDetails = [.. await this.JobsDatabaseRepository!.GetAllJobsAsync()];
}
