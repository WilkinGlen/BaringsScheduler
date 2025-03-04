namespace CronExpressionBuilderServiceTests;
using BaringsQuartzUI.Services;
using Quartz;
using Xunit;

public class CronExpressionBuilderServiceTests
{
    [Theory]
    [InlineData(10, 30, "0 30 10 ? * *")]
    [InlineData(0, 0, null)]
    [InlineData(23, 59, "0 59 23 ? * *")]
    public void BuildCronExpression_TimeSpan_ReturnsExpectedCronExpression(int hours, int minutes, string? expected)
    {
        var timeSpan = new TimeSpan(hours, minutes, 0);

        var result = CronExpressionBuilderService.BuildCronExpression(timeSpan);

        Assert.Equal(expected, result);
        if (result is not null)
        {
            Assert.True(CronExpression.IsValidExpression(result));
        }
    }

    [Theory]
    [InlineData(DayOfWeek.Monday, 9, 0, "0 0 9 ? * 2")]
    [InlineData(DayOfWeek.Sunday, 15, 30, "0 30 15 ? * 1")]
    public void BuildCronExpression_DayOfWeekAndTime_ReturnsExpectedCronExpression(DayOfWeek day, int hours, int minutes, string expected)
    {
        var timeSpan = new TimeSpan(hours, minutes, 0);

        var result = CronExpressionBuilderService.BuildCronExpression(day, timeSpan);

        Assert.Equal(expected, result);
        if (result is not null)
        {
            Assert.True(CronExpression.IsValidExpression(result));
        }
    }

    [Theory]
    [InlineData(new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Wednesday }, 10, 30, "0 30 10 ? * 2,4")]
    [InlineData(new DayOfWeek[] { DayOfWeek.Saturday, DayOfWeek.Sunday }, 12, 0, "0 0 12 ? * 7,1")]
    [InlineData(new DayOfWeek[] { }, 12, 0, null)]
    public void BuildCronExpression_TimeSpanAndDaysOfWeek_ReturnsExpectedCronExpression(DayOfWeek[] days, int hours, int minutes, string? expected)
    {
        var timeSpan = new TimeSpan(hours, minutes, 0);

        var result = CronExpressionBuilderService.BuildCronExpression(timeSpan, days);

        Assert.Equal(expected, result);
        if (result is not null)
        {
            Assert.True(CronExpression.IsValidExpression(result));
        }
    }

    [Theory]
    [InlineData(1, 9, 0, "0 0 9 1 * ?")]
    [InlineData(15, 12, 30, "0 30 12 15 * ?")]
    [InlineData(31, 23, 59, "0 59 23 31 * ?")]
    [InlineData(0, 12, 0, null)]
    public void BuildCronExpression_DayAndTime_ReturnsExpectedCronExpression(int day, int hours, int minutes, string? expected)
    {
        var timeSpan = new TimeSpan(hours, minutes, 0);

        var result = CronExpressionBuilderService.BuildCronExpression(day, timeSpan);

        Assert.Equal(expected, result);
        if (result is not null)
        {
            Assert.True(CronExpression.IsValidExpression(result));
        }
    }

    [Theory]
    [InlineData(1, 0, "0 0 0/1 * * ?")]
    [InlineData(0, 30, "0 0/30 * * * ?")]
    [InlineData(0, 0, null)]
    [InlineData(23, 0, "0 0 0/23 * * ?")]
    public void BuildCronExpression_HourAndMinute_ReturnsExpectedCronExpression(int hour, int minute, string? expected)
    {
        var result = CronExpressionBuilderService.BuildCronExpression(hour, minute);

        Assert.Equal(expected, result);
        if (result is not null)
        {
            Assert.True(CronExpression.IsValidExpression(result));
        }
    }
}
