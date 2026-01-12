// <copyright file="TimeTests.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using VoxelGame.Core.Domain.Chrono;
using Xunit;

namespace VoxelGame.Core.Tests.Domain.Chrono;

[TestSubject(typeof(Time))]
public class TimeTests
{
    [Fact]
    public void Time_StartOfDay_ShouldHaveCorrectValues()
    {
        Time time = Time.StartOfDay;

        Assert.Equal(expected: 0, time.Hours);
        Assert.Equal(expected: 0, time.Minutes);
        Assert.Equal(expected: 0, time.Seconds);
        Assert.Equal(expected: 0, time.TotalSeconds);

        Assert.False(time.IsDaytime);
        Assert.True(time.IsNighttime);
    }

    [Theory]
    [InlineData(6, 0, true)]
    [InlineData(12, 0, true)]
    [InlineData(17, 59, true)]
    [InlineData(18, 0, false)]
    [InlineData(23, 0, false)]
    [InlineData(5, 59, false)]
    public void Time_IsDaytime_ShouldMatchSunriseAndSunset(Int32 hour, Int32 minute, Boolean isDaytime)
    {
        Time time = new(hour, minute);
        Assert.Equal(isDaytime, time.IsDaytime);
        Assert.Equal(!isDaytime, time.IsNighttime);
    }

    [Theory]
    [InlineData(6, 0, 0.0)]
    [InlineData(12, 0, 0.25)]
    [InlineData(18, 0, 0.5)]
    [InlineData(0, 0, 0.75)]
    public void Time_TimeOfDay_ShouldHaveExpectedPoints(Int32 hour, Int32 minute, Double expected)
    {
        Time time = new(hour, minute);
        Assert.Equal(expected, time.TimeOfDay, precision: 2);
    }

    [Theory]
    [InlineData(23, 59, 50, 10, 0, 0, 0)]
    [InlineData(0, 0, 5, -10, 23, 59, 55)]
    public void Time_AddingDuration_ShouldWrapAroundMidnight(
        Int32 h, Int32 m, Int32 s,
        Int32 deltaSeconds,
        Int32 expectedH, Int32 expectedM, Int32 expectedS)
    {
        Time time = new(h, m, s);
        Duration delta = Duration.FromTotalSeconds(deltaSeconds);

        Time result = time + delta;

        Assert.Equal(expectedH, result.Hours);
        Assert.Equal(expectedM, result.Minutes);
        Assert.Equal(expectedS, result.Seconds);
    }

    [Theory]
    [InlineData(6, 0, 12, 0)]
    [InlineData(12, 30, 18, 0)]
    [InlineData(0, 0, 23, 59)]
    public void Time_Comparison_ShouldRespectChronologicalOrder(
        Int32 h1, Int32 m1,
        Int32 h2, Int32 m2)
    {
        Time t1 = new(h1, m1);
        Time t2 = new(h2, m2);

        Assert.True(t1 < t2);
        Assert.True(t2 > t1);
        Assert.True(t1 != t2);
        Assert.True(t1 == new Time(h1, m1));
    }

    [Theory]
    [InlineData(8, 0, 9, 0, Calendar.TicksPerHour)]
    [InlineData(9, 0, 8, 0, -Calendar.TicksPerHour)]
    public void Time_Until_ShouldReturnCorrectSignedDuration(
        Int32 h1, Int32 m1,
        Int32 h2, Int32 m2,
        Int64 expectedTicks)
    {
        Time start = new(h1, m1);
        Time end = new(h2, m2);

        Duration duration = start.Until(end);

        Assert.Equal(expectedTicks, duration.TotalTicks);
    }

    [Theory]
    [InlineData(8, 0, 16, 0, 12, 0, true)]
    [InlineData(8, 0, 16, 0, 7, 0, false)]
    [InlineData(22, 0, 6, 0, 23, 0, true)]
    [InlineData(22, 0, 6, 0, 2, 0, true)]
    [InlineData(22, 0, 6, 0, 12, 0, false)]
    public void Time_IsBetween_ShouldHandleWrappingAndNonWrappingRanges(
        Int32 startH, Int32 startM,
        Int32 endH, Int32 endM,
        Int32 testH, Int32 testM,
        Boolean expected)
    {
        Time start = new(startH, startM);
        Time end = new(endH, endM);
        Time test = new(testH, testM);

        Assert.Equal(expected, test.IsBetween(start, end));
    }

    [Theory]
    [InlineData(9, 5, 7, "09:05", "09:05:07")]
    [InlineData(0, 0, 0, "00:00", "00:00:00")]
    [InlineData(23, 59, 59, "23:59", "23:59:59")]
    public void Time_ToString_ShouldBeStable(
        Int32 h, Int32 m, Int32 s,
        String shortFormat, String longFormat)
    {
        Time time = new(h, m, s);

        Assert.Equal(shortFormat, time.ToString("S"));
        Assert.Equal(longFormat, time.ToString("L"));
    }
}
