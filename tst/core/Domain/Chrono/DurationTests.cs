// <copyright file="DurationTests.cs" company="VoxelGame">
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

[TestSubject(typeof(Duration))]
public class DurationTests
{
    [Fact]
    public void Duration_Zero_ShouldHaveCorrectValues()
    {
        Duration duration = Duration.Zero;

        Assert.True(duration.IsZero);
        Assert.False(duration.IsNegative);
        Assert.Equal(expected: 0, duration.TotalTicks);

        Assert.Equal(expected: 0, duration.Years);
        Assert.Equal(expected: 0, duration.Months);
        Assert.Equal(expected: 0, duration.Weeks);
        Assert.Equal(expected: 0, duration.Days);
        Assert.Equal(expected: 0, duration.Hours);
        Assert.Equal(expected: 0, duration.Minutes);
        Assert.Equal(expected: 0, duration.Seconds);

        Assert.Equal(expected: 0.0, duration.TotalSeconds, precision: 10);
        Assert.Equal(expected: 0.0, duration.TotalDays, precision: 10);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(12345)]
    [InlineData(-12345)]
    public void Duration_FromTicks_ShouldPreserveTotalTicks(Int64 ticks)
    {
        Duration duration = Duration.FromTicks(ticks);
        Assert.Equal(ticks, duration.TotalTicks);
        Assert.Equal(ticks == 0, duration.IsZero);
        Assert.Equal(ticks < 0, duration.IsNegative);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(30)]
    [InlineData(-30)]
    public void Duration_FromSeconds_ShouldMatchTickConversion(Int32 seconds)
    {
        Duration duration = Duration.FromSeconds(seconds);
        Assert.Equal(seconds * Calendar.TicksPerSecond, duration.TotalTicks);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(10)]
    [InlineData(-10)]
    public void Duration_FromMinutes_ShouldMatchTickConversion(Int32 minutes)
    {
        Duration duration = Duration.FromMinutes(minutes);
        Assert.Equal(minutes * Calendar.TicksPerMinute, duration.TotalTicks);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(5)]
    [InlineData(-5)]
    public void Duration_FromHours_ShouldMatchTickConversion(Int32 hours)
    {
        Duration duration = Duration.FromHours(hours);
        Assert.Equal(hours * Calendar.TicksPerHour, duration.TotalTicks);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(7)]
    [InlineData(-7)]
    public void Duration_FromDays_ShouldMatchTickConversion(Int32 days)
    {
        Duration duration = Duration.FromDays(days);
        Assert.Equal(days * Calendar.TicksPerDay, duration.TotalTicks);
    }

    [Fact]
    public void Duration_FromComponents_ShouldEqualSumOfParts()
    {
        Duration duration = Duration.From(
            years: 1,
            months: 2,
            weeks: 3,
            days: 4,
            hours: 5,
            minutes: 6,
            seconds: 7);

        Duration expected =
            Duration.FromYears(1) +
            Duration.FromMonths(2) +
            Duration.FromWeeks(3) +
            Duration.FromDays(4) +
            Duration.FromHours(5) +
            Duration.FromMinutes(6) +
            Duration.FromSeconds(7);

        Assert.Equal(expected, duration);
    }

    [Fact]
    public void Duration_From_ShouldDecomposeFullUnitsCorrectly()
    {
        Duration duration = Duration.From(
            years: 1,
            months: 2,
            weeks: 1,
            days: 3,
            hours: 4,
            minutes: 5,
            seconds: 6);

        Assert.Equal(expected: 1, duration.Years);
        Assert.Equal(expected: 2, duration.Months);
        Assert.Equal(expected: 1, duration.Weeks);
        Assert.Equal(expected: 3, duration.Days);
        Assert.Equal(expected: 4, duration.Hours);
        Assert.Equal(expected: 5, duration.Minutes);
        Assert.Equal(expected: 6, duration.Seconds);
    }

    [Fact]
    public void Duration_From_ShouldRedistributeExcessUnitsCorrectly()
    {
        Duration duration = Duration.From(
            years: 0,
            Calendar.MonthsPerYear,
            weeks: 0,
            Calendar.DaysPerYear,
            hours: 0,
            minutes: 0,
            seconds: 0);

        Assert.Equal(expected: 2, duration.Years);
        Assert.Equal(expected: 0, duration.Months);
        Assert.Equal(expected: 0, duration.Weeks);
        Assert.Equal(expected: 0, duration.Days);
        Assert.Equal(expected: 0, duration.Hours);
        Assert.Equal(expected: 0, duration.Minutes);
        Assert.Equal(expected: 0, duration.Seconds);
    }

    [Fact]
    public void Duration_NegatedAndAbsolute_ShouldBehaveAsExpected()
    {
        Duration duration = Duration.FromHours(3) + Duration.FromMinutes(5);
        Duration negated = duration.Negated();

        Assert.True(negated.IsNegative);
        Assert.Equal(-duration.TotalTicks, negated.TotalTicks);

        Duration absolute = negated.Absolute();
        Assert.False(absolute.IsNegative);
        Assert.Equal(duration.TotalTicks, absolute.TotalTicks);
    }

    [Fact]
    public void Duration_Arithmetic_ShouldBeInverse()
    {
        Duration a = Duration.FromDays(2) + Duration.FromHours(3);
        Duration b = Duration.FromMinutes(90);

        Assert.Equal(a, a + b - b);
        Assert.Equal(a, a - b + b);
        Assert.Equal(Duration.Zero, a - a);
    }

    [Fact]
    public void Duration_Scaling_ShouldWorkCorrectly()
    {
        Duration duration = Duration.FromHours(2) + Duration.FromMinutes(30);

        Duration doubled = duration * 2;
        Assert.Equal(duration + duration, doubled);

        Duration halved = duration / 2;
        Assert.Equal(Duration.FromHours(1) + Duration.FromMinutes(15), halved);
    }

    [Fact]
    public void Duration_Comparison_ShouldRespectTickOrdering()
    {
        Duration a = Duration.FromMinutes(1);
        Duration b = Duration.FromMinutes(2);

        Assert.True(a < b);
        Assert.True(b > a);
        Assert.True(b >= a);
        Assert.True(a != b);
        Assert.True(a == Duration.FromTicks(a.TotalTicks));
    }

    [Fact]
    public void Duration_MinMaxClamp_ShouldWork()
    {
        Duration min = Duration.FromSeconds(10);
        Duration max = Duration.FromSeconds(20);

        Duration below = Duration.FromSeconds(5);
        Duration inside = Duration.FromSeconds(15);
        Duration above = Duration.FromSeconds(25);

        Assert.Equal(min, Duration.Min(min, max));
        Assert.Equal(max, Duration.Max(min, max));

        Assert.Equal(min, Duration.Clamp(below, min, max));
        Assert.Equal(inside, Duration.Clamp(inside, min, max));
        Assert.Equal(max, Duration.Clamp(above, min, max));
    }

    [Theory]
    [InlineData(2.0)]
    [InlineData(0.5)]
    [InlineData(-1.5)]
    public void Duration_MultiplicationByDouble_ShouldRoundToEven(Double factor)
    {
        Duration baseDuration = Duration.FromTicks(3);
        Duration scaled = baseDuration * factor;

        var expected = (Int64) Math.Round(baseDuration.TotalTicks * factor, MidpointRounding.ToEven);
        Assert.Equal(expected, scaled.TotalTicks);
    }

    [Fact]
    public void Duration_ToString_SimpleAndFull_ShouldBeStable()
    {
        Duration duration = Duration.From(hours: 2, minutes: 3, seconds: 4);

        var full = duration.ToString("F");
        Assert.Contains(":", full, StringComparison.Ordinal);

        var simple = duration.ToString("S");
        Assert.NotEmpty(simple);

        var zeroSimple = Duration.Zero.ToString("S");
        Assert.Equal("0s", zeroSimple);
    }

    [Fact]
    public void Duration_ToString_Full_ShouldUseSingleLeadingSignForNegative()
    {
        Duration duration = Duration.FromMinutes(1).Negated();

        var full = duration.ToString("F");

        Assert.StartsWith("-", full, StringComparison.Ordinal);
        Assert.DoesNotContain("--", full, StringComparison.Ordinal);
    }
}
