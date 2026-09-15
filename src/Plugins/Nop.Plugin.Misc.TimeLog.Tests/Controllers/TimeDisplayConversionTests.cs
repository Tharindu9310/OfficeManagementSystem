using System.Reflection;
using Nop.Plugin.Misc.TimeLog.Controllers;
using Xunit;

namespace Nop.Plugin.Misc.TimeLog.Tests.Controllers;

/// <summary>
/// Unit tests for the HH:mm &lt;-&gt; decimal round-trip conversion helpers
/// (<c>ToTimeDisplay</c>/<c>FromTimeDisplay</c>, private static methods on
/// <see cref="TimeLogController"/>, per TT-011's implementation).
/// Invoked via reflection since the helpers are intentionally private
/// (UI-layer-only concern, per <see cref="Models.Admin.TimeLogModel.TimeDisplay"/>'s remarks) -
/// this test does not change their accessibility.
/// Covers the no-lossy-rounding design decision (see Domain.TimeLog.Time remarks):
/// values are round-tripped via minutes = round(hours * 60), not by formatting the decimal directly,
/// so non-terminating fractions such as 20 minutes (1/3 hour) reconstruct exactly.
/// </summary>
public class TimeDisplayConversionTests
{
    private static string ToTimeDisplay(decimal hours)
    {
        var method = typeof(TimeLogController).GetMethod("ToTimeDisplay",
            BindingFlags.NonPublic | BindingFlags.Static);

        return (string)method!.Invoke(null, new object[] { hours });
    }

    private static decimal? FromTimeDisplay(string timeDisplay)
    {
        var method = typeof(TimeLogController).GetMethod("FromTimeDisplay",
            BindingFlags.NonPublic | BindingFlags.Static);

        return (decimal?)method!.Invoke(null, new object[] { timeDisplay });
    }

    [Theory]
    [InlineData(6.5, "06:30")]
    [InlineData(0, "00:00")]
    [InlineData(24, "24:00")]
    [InlineData(1, "01:00")]
    public void ToTimeDisplay_ConvertsDecimalHoursToExpectedHhMm(double hours, string expected)
    {
        var result = ToTimeDisplay((decimal)hours);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("08:15", 8.25)]
    [InlineData("00:00", 0)]
    [InlineData("06:30", 6.5)]
    public void FromTimeDisplay_ParsesHhMmToExpectedDecimalHours(string timeDisplay, double expected)
    {
        var result = FromTimeDisplay(timeDisplay);

        Assert.Equal((decimal)expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("garbage")]
    [InlineData("08")]
    [InlineData("08:xx")]
    public void FromTimeDisplay_InvalidFormat_ReturnsNull(string timeDisplay)
    {
        var result = FromTimeDisplay(timeDisplay);

        Assert.Null(result);
    }

    /// <summary>
    /// The critical non-trivial precision case from the design doc: 20 minutes is a
    /// non-terminating decimal fraction of an hour (1/3 = 0.3333...). The round-trip
    /// MUST reconstruct exactly "00:20" - not "00:19" or "00:21" - confirming
    /// minutes = round(hours * 60) is used rather than naive decimal formatting.
    /// </summary>
    [Fact]
    public void RoundTrip_TwentyMinutes_ReconstructsExactlyWithoutLossyRounding()
    {
        var twentyMinutesAsHours = 20m / 60m; // 0.3333...

        var display = ToTimeDisplay(twentyMinutesAsHours);
        Assert.Equal("00:20", display);

        var roundTripped = FromTimeDisplay(display);
        Assert.NotNull(roundTripped);

        var displayAgain = ToTimeDisplay(roundTripped.Value);
        Assert.Equal("00:20", displayAgain);
    }

    /// <summary>
    /// Same non-terminating-fraction scenario for a non-zero hour component (1h 20m).
    /// </summary>
    [Fact]
    public void RoundTrip_OneHourTwentyMinutes_ReconstructsExactly()
    {
        var oneHourTwentyMinutesAsHours = 1m + 20m / 60m;

        var display = ToTimeDisplay(oneHourTwentyMinutesAsHours);

        Assert.Equal("01:20", display);
    }

    /// <summary>
    /// 40 minutes (2/3 hour) is the mirror non-terminating case and must also round-trip exactly.
    /// </summary>
    [Fact]
    public void RoundTrip_FortyMinutes_ReconstructsExactly()
    {
        var fortyMinutesAsHours = 40m / 60m;

        var display = ToTimeDisplay(fortyMinutesAsHours);

        Assert.Equal("00:40", display);
    }
}
