namespace BaringsQuartzUI.Components.Controls.Dialogs;

using Microsoft.AspNetCore.Components;

public sealed partial class PeriodExpressionBuilder
{
    [Parameter]
    public EventCallback<(int Hours, int Minutes)> OnHoursMinutesChanged { get; set; }

    private readonly int[] Hours = Enumerable.Range(0, 24).ToArray();
    private readonly int[] Minutes = Enumerable.Range(0, 60).ToArray();

    private int selectedHour;
    private int selectedMinute;

    private int SelectedHour
    {
        get => this.selectedHour;
        set
        {
            if (value > 0)
            {
                this.selectedMinute = 0;
            }

            this.selectedHour = value;
            _ = this.OnHoursMinutesChanged.InvokeAsync((this.selectedHour, this.selectedMinute));
        }
    }

    private int SelectedMinute
    {
        get => this.selectedMinute;
        set
        {
            if (value > 0)
            {
                this.selectedHour = 0;
            }

            this.selectedMinute = value;
            _ = this.OnHoursMinutesChanged.InvokeAsync((this.selectedHour, this.selectedMinute));
        }
    }
}
