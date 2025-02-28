namespace BaringsQuartzUI.Components.Controls.Dialogs;

using Microsoft.AspNetCore.Components;

public sealed partial class WeeklyExpressionBuilder
{
    private DayOfWeek? selectedDay;
    private TimeSpan? selectedTime;

    [Parameter]
    public EventCallback<(DayOfWeek? day, TimeSpan? time)> OnDayTimeChanged { get; set; }

    private void DaySelected(string day)
    {
        this.selectedDay = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), day);
        this.RaiseOnDayTimeChanged();
    }

    private void TimeChangedHandler(TimeSpan? time)
    {
        if (time.HasValue)
        {
            this.selectedTime = time.Value;
            this.RaiseOnDayTimeChanged();
        }
    }

    private void RaiseOnDayTimeChanged()
    {
        if(this.selectedDay.HasValue && this.selectedTime.HasValue)
        {
            _ = this.OnDayTimeChanged.InvokeAsync((this.selectedDay, this.selectedTime));
        }
    }
}
