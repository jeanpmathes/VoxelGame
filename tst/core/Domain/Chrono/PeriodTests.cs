// <copyright file="PeriodTests.cs" company="VoxelGame">
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

[TestSubject(typeof(Period))]
public class PeriodTests
{
    [Fact]
    public void Period_Zero_ShouldHaveCorrectValues()
    {
        Period period = Period.FromDays(0);

        Assert.True(period.IsZero);
        Assert.False(period.IsPositive);
        Assert.False(period.IsNegative);

        Assert.Equal(expected: 0, period.TotalDays);

        Assert.Equal(expected: 0, period.Years);
        Assert.Equal(expected: 0, period.Months);
        Assert.Equal(expected: 0, period.Weeks);
        Assert.Equal(expected: 0, period.Days);

        Assert.Equal(expected: 0.0, period.TotalWeeks, precision: 10);
        Assert.Equal(expected: 0.0, period.TotalMonths, precision: 10);
        Assert.Equal(expected: 0.0, period.TotalYears, precision: 10);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(123)]
    [InlineData(-123)]
    public void Period_FromDays_ShouldPreserveTotalDays(Int32 totalDays)
    {
        Period period = Period.FromDays(totalDays);

        Assert.Equal(totalDays, period.TotalDays);
        Assert.Equal(totalDays == 0, period.IsZero);
        Assert.Equal(totalDays > 0, period.IsPositive);
        Assert.Equal(totalDays < 0, period.IsNegative);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(10)]
    [InlineData(-10)]
    public void Period_FromWeeks_ShouldMatchDayConversion(Int32 weeks)
    {
        Period period = Period.FromWeeks(weeks);

        Assert.Equal(weeks * Calendar.DaysPerWeek, period.TotalDays);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(10)]
    [InlineData(-10)]
    public void Period_FromMonths_ShouldMatchDayConversion(Int32 months)
    {
        Period period = Period.FromMonths(months);

        Assert.Equal(months * Calendar.DaysPerMonth, period.TotalDays);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(10)]
    [InlineData(-10)]
    public void Period_FromYears_ShouldMatchDayConversion(Int32 years)
    {
        Period period = Period.FromYears(years);

        Assert.Equal(years * Calendar.DaysPerYear, period.TotalDays);
    }

    [Fact]
    public void Period_Components_ShouldDecomposeFullUnitsCorrectly()
    {
        Period period =
            Period.FromYears(1) +
            Period.FromMonths(2) +
            Period.FromWeeks(3) +
            Period.FromDays(4);

        Assert.Equal(expected: 1, period.Years);
        Assert.Equal(expected: 2, period.Months);
        Assert.Equal(expected: 3, period.Weeks);
        Assert.Equal(expected: 4, period.Days);
    }

    [Fact]
    public void Period_Arithmetic_ShouldBeInverse()
    {
        Period a = Period.FromDays(50);
        Period b = Period.FromDays(12);

        Assert.Equal(a, a + b - b);
        Assert.Equal(a, a - b + b);
        Assert.Equal(Period.FromDays(0), a - a);
    }

    [Fact]
    public void Period_NegatedAndAbsolute_ShouldBehaveAsExpected()
    {
        Period period = Period.FromMonths(1) + Period.FromDays(3);

        Period negated = period.Negated();
        Assert.True(negated.IsNegative);
        Assert.Equal(-period.TotalDays, negated.TotalDays);

        Period absolute = negated.Absolute();
        Assert.False(absolute.IsNegative);
        Assert.Equal(period.TotalDays, absolute.TotalDays);

        Period unary = -period;
        Assert.Equal(negated, unary);
    }

    [Fact]
    public void Period_Comparison_ShouldRespectDayOrdering()
    {
        Period a = Period.FromWeeks(1);
        Period b = Period.FromWeeks(2);

        Assert.True(a < b);
        Assert.True(b > a);
        Assert.True(b >= a);
        Assert.True(a != b);
        Assert.True(a == Period.FromDays(a.TotalDays));
    }

    [Fact]
    public void Period_ToDuration_ShouldMatchDayConversion()
    {
        Period period = Period.FromWeeks(2) + Period.FromDays(3);
        var duration = period.ToDuration();

        Assert.Equal(period.TotalDays * Calendar.TicksPerDay, duration.TotalTicks);
    }

    [Theory]
    [InlineData(1.0, Calendar.DaysPerWeek)]
    [InlineData(0.5, Calendar.DaysPerWeek / 2 + 1)]
    [InlineData(-1.5, -(Calendar.DaysPerWeek + Calendar.DaysPerWeek / 2))]
    public void Period_FromTotalWeeks_ShouldRoundToEven(Double weeks, Int32 expectedDays)
    {
        Period period = Period.FromTotalWeeks(weeks);

        var expected = (Int32) Math.Round(weeks * Calendar.DaysPerWeek, MidpointRounding.ToEven);
        Assert.Equal(expected, period.TotalDays);

        Assert.Equal(expectedDays, period.TotalDays);
    }

    [Fact]
    public void Period_ToString_SimpleAndFull_ShouldBeStable()
    {
        Period period = Period.FromYears(1) + Period.FromMonths(2) + Period.FromWeeks(3) + Period.FromDays(4);

        var full = period.ToString("F");
        Assert.Contains(".", full, StringComparison.Ordinal);

        var simple = period.ToString("S");
        Assert.NotEmpty(simple);

        var zeroSimple = Period.FromDays(0).ToString("S");
        Assert.Equal("0d", zeroSimple);
    }

    [Fact]
    public void Period_ToString_Full_ShouldUseSingleLeadingSignForNegative()
    {
        Period period = Period.FromDays(1).Negated();

        var full = period.ToString("F");

        Assert.StartsWith("-", full, StringComparison.Ordinal);
    }
}
