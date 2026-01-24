// <copyright file="DateTests.cs" company="VoxelGame">
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
using DayOfWeek = VoxelGame.Core.Domain.Chrono.DayOfWeek;

namespace VoxelGame.Core.Tests.Domain.Chrono;

[TestSubject(typeof(Date))]
public class DateTests
{
    [Fact]
    public void Date_StartOfCalendar_ShouldHaveCorrectValues()
    {
        Date start = Date.StartOfCalendar;
        Assert.Equal(expected: 1, start.Day);
        Assert.Equal(Month.Spring, start.Month);
        Assert.Equal(expected: 1, start.Year);
        Assert.Equal(expected: 0, start.TotalDaysSinceStart);
        Assert.Equal(expected: 1, start.DayOfYear);
        Assert.Equal(DayOfWeek.Monday, start.DayOfWeek);
    }

    [Fact]
    public void Date_NextDay_ShouldWorkAtMonthBoundary()
    {
        Date lastDayOfMonth = new(Calendar.DaysPerMonth, Month.Spring, year: 1);
        Assert.Equal(Calendar.DaysPerMonth, lastDayOfMonth.Day);
        Assert.Equal(Month.Spring, lastDayOfMonth.Month);

        Date next = lastDayOfMonth.NextDay;
        Assert.Equal(expected: 1, next.Day);
        Assert.Equal(Month.Summer, next.Month);
        Assert.Equal(expected: 1, next.Year);
    }

    [Fact]
    public void Date_NextDay_ShouldWorkAtYearBoundary()
    {
        Date endOfYear = new(Calendar.DaysPerMonth, Month.Winter, year: 1);
        Assert.Equal(Calendar.DaysPerYear, endOfYear.DayOfYear);

        Date next = endOfYear.NextDay;
        Assert.Equal(expected: 1, next.Day);
        Assert.Equal(expected: 1, next.Month.ToNumber());
        Assert.Equal(expected: 2, next.Year);
        Assert.Equal(Calendar.DaysPerYear, next.TotalDaysSinceStart);
    }

    [Fact]
    public void Date_Comparisons_ShouldConsiderAllParts()
    {
        Date d1 = new(day: 1, Month.Spring, year: 1);
        Date d2 = new(day: 2, Month.Spring, year: 1);
        Date d3 = new(day: 1, Month.Summer, year: 1);
        Date d4 = new(day: 1, Month.Spring, year: 2);

        Assert.True(d1 < d2);
        Assert.True(d2 < d3);
        Assert.True(d3 < d4);
        Assert.True(d4 > d1);
        Assert.True(d1 != d2);
        Assert.True(d1 == new Date(day: 1, Month.Spring, year: 1));
    }

    [Fact]
    public void Date_Operations_ShouldBeInverse()
    {
        Date date = new(day: 15, Month.Summer, year: 3);
        Period period = Period.FromDays(10);

        Assert.Equal(date, date + period - period);
        Assert.Equal(date, date - period + period);
    }

    [Fact]
    public void Date_FromTotalDaysSinceStart_ShouldDecomposeCorrectly()
    {
        for (var day = 0; day < Calendar.DaysPerYear * 2; day++)
        {
            Date date = Date.FromTotalDaysSinceStart(day);

            Assert.Equal(day, date.TotalDaysSinceStart);
            Assert.True(date.Day is >= 1 and <= Calendar.DaysPerMonth);
            Assert.True(date.Month.ToNumber() is >= 1 and <= Calendar.MonthsPerYear);
            Assert.True(date.Year >= 1);
            Assert.True(date.DayOfYear is >= 1 and <= Calendar.DaysPerYear);
            Assert.True(date.DayOfWeek.ToNumber() is >= 1 and <= Calendar.DaysPerWeek);
        }
    }

    [Fact]
    public void Date_ToString_ShouldBeStable()
    {
        Date date = new(day: 3, Month.Winter, year: 12);

        Assert.Equal("03", date.ToString("D"));
        Assert.Equal("04", date.ToString("M"));
        Assert.Equal("0012", date.ToString("Y"));
        Assert.Equal("0012/04", date.ToString("YM"));
        Assert.Equal("0012/04/03", date.ToString("YMD"));

        var s = date.ToString("S");
        Assert.Contains(" ", s, StringComparison.Ordinal);

        var l = date.ToString("L");
        Assert.Contains(",", l, StringComparison.Ordinal);
    }

    [Fact]
    public void Date_DayOfWeek_ShouldBeCorrect()
    {
        Date date = Date.StartOfCalendar;

        Assert.Equal(DayOfWeek.Monday, date.DayOfWeek);

        date = date.NextDay;
        Assert.Equal(DayOfWeek.Tuesday, date.DayOfWeek);

        date = date.NextDay;
        Assert.Equal(DayOfWeek.Wednesday, date.DayOfWeek);

        date = date.NextDay;
        Assert.Equal(DayOfWeek.Thursday, date.DayOfWeek);

        date = date.NextDay;
        Assert.Equal(DayOfWeek.Friday, date.DayOfWeek);

        date = date.NextDay;
        Assert.Equal(DayOfWeek.Saturday, date.DayOfWeek);

        date = date.NextDay;
        Assert.Equal(DayOfWeek.Sunday, date.DayOfWeek);

        date = date.NextDay;
        Assert.Equal(DayOfWeek.Monday, date.DayOfWeek);
    }

    [Theory]
    [InlineData(1, Month.Spring, 1, true)]
    [InlineData(0, Month.Spring, 1, false)]
    [InlineData(Calendar.DaysPerMonth + 1, Month.Spring, 1, false)]
    [InlineData(1, Month.Spring, 0, false)]
    public void Date_TryCreate_ShouldNotCreateInvalidDates(
        Int32 day, Month month, Int32 year,
        Boolean expectedValidity)
    {
        Boolean isValid = Date.TryCreate(day, month, year, out Date _);
        Assert.Equal(expectedValidity, isValid);
    }

    [Theory]
    [InlineData(1, Month.Spring, 1, 1, Month.Spring, 1, 0)]
    [InlineData(1, Month.Spring, 1, 2, Month.Spring, 1, 1)]
    [InlineData(1, Month.Spring, 1, 1, Month.Summer, 1, Calendar.DaysPerMonth)]
    [InlineData(1, Month.Spring, 1, 1, Month.Spring, 2, Calendar.DaysPerYear)]
    [InlineData(2, Month.Spring, 1, 1, Month.Spring, 1, -1)]
    [InlineData(1, Month.Summer, 1, 1, Month.Spring, 1, -Calendar.DaysPerMonth)]
    [InlineData(1, Month.Spring, 2, 1, Month.Spring, 1, -Calendar.DaysPerYear)]
    public void Date_Subtraction_ShouldReturnCorrectPeriod(
        Int32 day1, Month month1, Int32 year1,
        Int32 day2, Month month2, Int32 year2,
        Int32 expectedTotalDays)
    {
        Date date1 = new(day1, month1, year1);
        Date date2 = new(day2, month2, year2);

        Period period = date2 - date1;
        Assert.Equal(expectedTotalDays, period.TotalDays);
    }
}
