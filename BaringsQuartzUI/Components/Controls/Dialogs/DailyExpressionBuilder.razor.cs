namespace BaringsQuartzUI.Components.Controls.Dialogs;

using Microsoft.AspNetCore.Components;

public sealed partial class DailyExpressionBuilder
{
    [Parameter]
    public EventCallback<TimeSpan> OnTimeChanged { get; set; }

    private void TimeChangedHandler(TimeSpan? times)
    {
        if (times.HasValue)
        {
            _ = this.OnTimeChanged.InvokeAsync(times.Value);
        }
    }
}
