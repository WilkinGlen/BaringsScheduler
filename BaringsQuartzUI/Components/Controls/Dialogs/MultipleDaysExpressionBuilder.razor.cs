namespace BaringsQuartzUI.Components.Controls.Dialogs;

using Microsoft.AspNetCore.Components;

public sealed partial class MultipleDaysExpressionBuilder
{
    private (bool Mon, bool Tue, bool Wed, bool Thu, bool Fri, bool Sat, bool Sun) days;
    private TimeSpan? selectedTime;
    private readonly List<DayOfWeek> daysOftheWeek = [];

    [Parameter]
    public EventCallback<(TimeSpan TimeSpan, DayOfWeek[] DaysOfTheWeek)> OnDaysTimeChanged { get; set; }

    private void MonChangedHandler(bool value)
    {
        this.days.Mon = value;
        if(value)
        {
            this.daysOftheWeek.Add(DayOfWeek.Monday);
        }
        else
        {
            _ = this.daysOftheWeek.Remove(DayOfWeek.Monday);
        }

        this.RaiseOnDayTimeChanged();
    }

    private void TueChangedHandler(bool value)
    {
        this.days.Tue = value;
        if (value)
        {
            this.daysOftheWeek.Add(DayOfWeek.Tuesday);
        }
        else
        {
            _ = this.daysOftheWeek.Remove(DayOfWeek.Tuesday);
        }

        this.RaiseOnDayTimeChanged();
    }

    private void WedChangedHandler(bool value)
    {
        this.days.Wed = value;
        if (value)
        {
            this.daysOftheWeek.Add(DayOfWeek.Wednesday);
        }
        else
        {
            _ = this.daysOftheWeek.Remove(DayOfWeek.Wednesday);
        }

        this.RaiseOnDayTimeChanged();
    }

    private void ThuChangedHandler(bool value)
    {
        this.days.Thu = value;
        if (value)
        {
            this.daysOftheWeek.Add(DayOfWeek.Thursday);
        }
        else
        {
            _ = this.daysOftheWeek.Remove(DayOfWeek.Thursday);
        }

        this.RaiseOnDayTimeChanged();
    }

    private void FriChangedHandler(bool value)
    {
        this.days.Fri = value;
        if (value)
        {
            this.daysOftheWeek.Add(DayOfWeek.Friday);
        }
        else
        {
            _ = this.daysOftheWeek.Remove(DayOfWeek.Friday);
        }

        this.RaiseOnDayTimeChanged();
    }

    private void SatChangedHandler(bool value)
    {
        this.days.Sat = value;
        if (value)
        {
            this.daysOftheWeek.Add(DayOfWeek.Saturday);
        }
        else
        {
            _ = this.daysOftheWeek.Remove(DayOfWeek.Saturday);
        }

        this.RaiseOnDayTimeChanged();
    }

    private void SunChangedHandler(bool value)
    {
        this.days.Sun = value;
        if (value)
        {
            this.daysOftheWeek.Add(DayOfWeek.Sunday);
        }
        else
        {
            _ = this.daysOftheWeek.Remove(DayOfWeek.Sunday);
        }

        this.RaiseOnDayTimeChanged();
    }

    private void TimeChangedHandler(TimeSpan? time)
    {
        this.selectedTime = time!.Value;
        this.RaiseOnDayTimeChanged();
    }

    private void RaiseOnDayTimeChanged()
    {
        if(this.daysOftheWeek?.Count > 0 && this.selectedTime.HasValue)
        {
            _ = this.OnDaysTimeChanged.InvokeAsync((this.selectedTime.Value, this.daysOftheWeek.ToArray()));
        }
    }
}
