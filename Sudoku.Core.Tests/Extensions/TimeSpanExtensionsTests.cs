using Sudoku.Extensions;

namespace Sudoku.Core.Tests.Extensions;

public class TimeSpanExtensionsTests
{
    [Fact]
    public void ReadableTime_Zero_ReturnsZeroMilliseconds()
    {
        Assert.Equal("0 ms", TimeSpan.Zero.ReadableTime());
    }

    [Fact]
    public void ReadableTime_SubMillisecond_ReturnsZeroMilliseconds()
    {
        Assert.Equal("0 ms", TimeSpan.FromTicks(5).ReadableTime());
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 250, "250ms")]
    [InlineData(0, 0, 0, 3, 0, "3s")]
    [InlineData(0, 0, 2, 0, 0, "2m")]
    [InlineData(0, 4, 0, 0, 0, "4h")]
    [InlineData(1, 0, 0, 0, 0, "1d")]
    [InlineData(1, 2, 3, 4, 5, "1d 2h 3m 4s 5ms")]
    [InlineData(0, 1, 0, 30, 0, "1h 30s")]
    public void ReadableTime_FormatsNonZeroParts(int days, int hours, int minutes, int seconds, int milliseconds, string expected)
    {
        TimeSpan time = new(days, hours, minutes, seconds, milliseconds);

        Assert.Equal(expected, time.ReadableTime());
    }
}
