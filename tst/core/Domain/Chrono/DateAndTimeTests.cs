// <copyright file="DateAndTimeTests.cs" company="VoxelGame">
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

[TestSubject(typeof(DateAndTime))]
public class DateAndTimeTests
{
    [Fact]
    public void DateAndTime_Constructor_ShouldPreserveDateAndTime()
    {
        Date date = new(day: 1, Month.Spring, year: 1);
        Time time = new(hours: 12, minutes: 0);

        DateAndTime dateAndTime = new(date, time);

        Assert.Equal(date, dateAndTime.Date);
        Assert.Equal(time, dateAndTime.Time);
    }

    [Fact]
    public void DateAndTime_FromTicks_ShouldCreateStartOfCalendar()
    {
        DateAndTime dateAndTime = DateAndTime.FromTicks(0);

        Assert.Equal(Date.StartOfCalendar, dateAndTime.Date);
        Assert.Equal(Time.StartOfDay, dateAndTime.Time);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 0, 1)]
    [InlineData(5, 6, 30)]
    public void DateAndTime_FromTicks_ShouldDecomposeCorrectly(
        Int32 dayOffset,
        Int32 hour,
        Int32 minute)
    {
        Int64 ticks = dayOffset * Calendar.TicksPerDay +
                      hour * Calendar.TicksPerHour +
                      minute * Calendar.TicksPerMinute;

        DateAndTime dateAndTime = DateAndTime.FromTicks(ticks);

        Assert.Equal(dayOffset, dateAndTime.Date.TotalDaysSinceStart);
        Assert.Equal(hour, dateAndTime.Time.Hours);
        Assert.Equal(minute, dateAndTime.Time.Minutes);
        Assert.Equal(expected: 0, dateAndTime.Time.Seconds);
    }

    [Fact]
    public void DateAndTime_StartAndEndOfDay_ShouldMatchHelpers()
    {
        Date date = new(day: 5, Month.Summer, year: 3);
        DateAndTime dateAndTime = new(date, Time.Noon);

        Assert.Equal(Time.StartOfDay, dateAndTime.StartOfDay.Time);
        Assert.Equal(Time.EndOfDay, dateAndTime.EndOfDay.Time);
        Assert.Equal(date, dateAndTime.StartOfDay.Date);
        Assert.Equal(date, dateAndTime.EndOfDay.Date);
    }

    [Fact]
    public void DateAndTime_AddingDuration_ShouldAdvanceDateCorrectly()
    {
        const Int32 startDay = 5;
        const Int32 daysToAdd = 2;

        Date date = Date.FromTotalDaysSinceStart(startDay);
        Time time = new(hours: 6, minutes: 59);
        DateAndTime dateAndTime = new(date, time);

        Duration duration = Duration.FromTotalDays(daysToAdd) + Duration.FromTotalSeconds(60);

        DateAndTime result = dateAndTime + duration;

        Assert.Equal(startDay + daysToAdd, result.Date.TotalDaysSinceStart);

        Assert.Equal(expected: 7, result.Time.Hours);
        Assert.Equal(expected: 0, result.Time.Minutes);
    }

    [Fact]
    public void DateAndTime_AdditionAndSubtraction_ShouldBeInverseOperations()
    {
        DateAndTime dateAndTime1 = new(
            Date.FromTotalDaysSinceStart(1),
            new Time(hours: 6, minutes: 23));

        DateAndTime dateAndTime2 = new(
            Date.FromTotalDaysSinceStart(3),
            new Time(hours: 23, minutes: 48));

        Assert.Equal(dateAndTime1, dateAndTime2 - (dateAndTime2 - dateAndTime1));
        Assert.Equal(dateAndTime2, dateAndTime1 + (dateAndTime2 - dateAndTime1));
    }

    [Fact]
    public void DateAndTime_Until_ShouldReturnCorrectDuration()
    {
        DateAndTime start = new(
            Date.FromTotalDaysSinceStart(1),
            new Time(hours: 6, minutes: 0));

        DateAndTime end = new(
            Date.FromTotalDaysSinceStart(2),
            new Time(hours: 6, minutes: 0));

        Duration duration = start.Until(end);

        Assert.Equal(
            Calendar.TicksPerDay,
            duration.TotalTicks);
    }

    [Fact]
    public void DateAndTime_Comparison_ShouldRespectChronologicalOrder()
    {
        DateAndTime dateAndTime1 = new(
            Date.FromTotalDaysSinceStart(1),
            new Time(hours: 6, minutes: 0));

        DateAndTime dateAndTime2 = new(
            Date.FromTotalDaysSinceStart(1),
            new Time(hours: 12, minutes: 0));

        DateAndTime dateAndTime3 = new(
            Date.FromTotalDaysSinceStart(2),
            new Time(hours: 0, minutes: 0));

        Assert.True(dateAndTime1 < dateAndTime2);
        Assert.True(dateAndTime2 < dateAndTime3);
        Assert.True(dateAndTime3 > dateAndTime1);
        Assert.True(dateAndTime1 != dateAndTime2);

        Assert.True(dateAndTime1 == new DateAndTime(
            Date.FromTotalDaysSinceStart(1),
            new Time(hours: 6, minutes: 0)));
    }

    [Fact]
    public void DateAndTime_ToString_ShouldBeStable()
    {
        Date date = new(day: 3, Month.Winter, year: 12);
        Time time = new(hours: 9, minutes: 5, seconds: 7);

        DateAndTime dateAndTime = new(date, time);

        var text = dateAndTime.ToString();

        Assert.Contains(date.ToString(), text, StringComparison.Ordinal);
        Assert.Contains(time.ToString(), text, StringComparison.Ordinal);
    }
}
