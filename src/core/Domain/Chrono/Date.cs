// <copyright file="Date.cs" company="VoxelGame">
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
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using VoxelGame.Core.Serialization;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Domain.Chrono;

/// <summary>
///     A date according to <see cref="Calendar" />.
///     This struct does not contain a specific time of day, only the day, month, and year.
///     It should be used as a starting point for most user-entered dates, as well as for repeated events.
/// </summary>
[JsonConverter(typeof(DateJsonConverter))]
public struct Date : IEquatable<Date>, IComparable<Date>, IValue
{
    /// <summary>
    ///     The data is encoded to use minimal space.
    ///     The day is stored in the first 5 bits and can represent values from 0 to 28 (exclusive).
    ///     The month is stored in the next 2 bits and can represent values from 0 to 4 (exclusive).
    ///     All remaining 25 bits are used to store the year.
    /// </summary>
    private UInt32 data;

    private const Int32 BitsForDay = 5;
    private const Int32 ShiftForDay = 0;

    private const Int32 BitsForMonth = 2;
    private const Int32 ShiftForMonth = BitsForDay + ShiftForDay;

    private const Int32 BitsForYear = 32 - BitsForDay - BitsForMonth;
    private const Int32 ShiftForYear = BitsForMonth + ShiftForMonth;

    private static readonly UInt32 dayMask = BitTools.GetMask(BitsForDay) << ShiftForDay;
    private static readonly UInt32 monthMask = BitTools.GetMask(BitsForMonth) << ShiftForMonth;
    private static readonly UInt32 yearMask = BitTools.GetMask(BitsForYear) << ShiftForYear;

    /// <summary>
    ///     Create a new date.
    /// </summary>
    /// <param name="day">The day of the month. Must be in [1, 28].</param>
    /// <param name="month">The month of the year.</param>
    /// <param name="year">The year. Must be positive.</param>
    public Date(Int32 day, Month month, Int32 year)
    {
        Debug.Assert(day is >= 1 and <= Calendar.DaysPerMonth);
        Debug.Assert(year > 0);

        var storedDay = (UInt32) (day - 1);
        var storedMonth = (UInt32) (month.ToNumber() - 1);
        var storedYear = (UInt32) (year - 1);

        data = storedDay << ShiftForDay | storedMonth << ShiftForMonth | storedYear << ShiftForYear;
    }

    /// <summary>
    ///     Try to create a new date. Similar to the constructor, but will perform explicit validation and return false if the
    ///     date is invalid.
    /// </summary>
    public static Boolean TryCreate(Int32 day, Month month, Int32 year, out Date date)
    {
        if (day is < 1 or > Calendar.DaysPerMonth || year < 1 || month.ToNumber() is < 1 or > Calendar.MonthsPerYear)
        {
            date = default;

            return false;
        }

        date = new Date(day, month, year);

        return true;
    }

    /// <summary>
    ///     Create a date from the total number of days since the start of the calendar.
    /// </summary>
    /// <param name="totalDays">The total number of days since the start of the calendar. Must be non-negative.</param>
    /// <returns>>The created date.</returns>
    public static Date FromTotalDaysSinceStart(Int32 totalDays)
    {
        Debug.Assert(totalDays >= 0);

        Int32 year = totalDays / Calendar.DaysPerYear;
        totalDays -= year * Calendar.DaysPerYear;

        Int32 month = totalDays / Calendar.DaysPerMonth;
        totalDays -= month * Calendar.DaysPerMonth;

        Int32 day = totalDays;

        return new Date(day + 1, Months.FromNumber(month + 1), year + 1);
    }

    /// <summary>
    ///     The first day of the calendar (year 1, month 1, day 1).
    /// </summary>
    public static readonly Date StartOfCalendar = new(day: 1, Months.FromNumber(1), year: 1);

    /// <summary>
    ///     Get the day of this date, as day of the month.
    ///     Is always greater than or equal to 1.
    /// </summary>
    public Int32 Day => (Int32) ((data & dayMask) >> ShiftForDay) + 1;

    /// <summary>
    ///     Get the month of this date.
    /// </summary>
    public Month Month => Months.FromNumber((Int32) ((data & monthMask) >> ShiftForMonth) + 1);

    /// <summary>
    ///     Get the year of this date.
    ///     Is always greater than or equal to 1.
    /// </summary>
    public Int32 Year => (Int32) ((data & yearMask) >> ShiftForYear) + 1;

    /// <summary>
    ///     Get the day of the year.
    /// </summary>
    public Int32 DayOfYear => Day + (Month.ToNumber() - 1) * Calendar.DaysPerMonth;

    /// <summary>
    ///     Get the day of the week.
    /// </summary>
    public DayOfWeek DayOfWeek => (DayOfWeek) ((DayOfYear - 1) % Calendar.DaysPerWeek);

    /// <summary>
    ///     Get the total number of passed days since the start of the calendar (year 1, month 1, day 1).
    ///     Will be zero for the first day of the calendar.
    /// </summary>
    public Int32 TotalDaysSinceStart => (Year - 1) * Calendar.DaysPerYear +
                                        (Month.ToNumber() - 1) * Calendar.DaysPerMonth +
                                        (Day - 1);

    /// <summary>
    ///     Get the previous day. Only valid if the date is not the first day of the calendar.
    /// </summary>
    public Date PreviousDay => this - Period.Day;

    /// <summary>
    ///     Get the next day.
    /// </summary>
    public Date NextDay => this + Period.Day;

    /// <summary>
    ///     Combine this date with a time to create a <see cref="DateAndTime" /> instance.
    /// </summary>
    /// <param name="time">The time of day.</param>
    /// <returns>>The combined date and time.</returns>
    public DateAndTime ToDateAndTime(Time time)
    {
        return new DateAndTime(this, time);
    }

    /// <summary>
    ///     Subtract two dates to get the period between them.
    /// </summary>
    public static Period operator -(Date left, Date right)
    {
        Int32 totalDaysLeft = left.TotalDaysSinceStart;
        Int32 totalDaysRight = right.TotalDaysSinceStart;

        Int32 difference = totalDaysLeft - totalDaysRight;

        return Period.FromDays(difference);
    }

    /// <inheritdoc cref="operator - (Date, Date)" />
    public static Period Subtract(Date left, Date right)
    {
        return left - right;
    }

    /// <summary>
    ///     Add a period to a date to get a new date.
    ///     Submitting a period that results in a date before the start of the calendar is not allowed.
    /// </summary>
    public static Date operator +(Date date, Period period)
    {
        Int32 totalDays = date.TotalDaysSinceStart;
        Int32 shift = period.TotalDays;

        Debug.Assert(shift >= 0 || totalDays + shift >= 0);

        totalDays += shift;

        return FromTotalDaysSinceStart(totalDays);
    }

    /// <inheritdoc cref="operator + (Date, Period)" />
    public static Date Add(Date date, Period period)
    {
        return date + period;
    }

    /// <summary>
    ///     Remove a period from a date to get a new date.
    ///     Submitting a period that results in a date before the start of the calendar is not allowed.
    /// </summary>
    public static Date operator -(Date date, Period period)
    {
        return date + period.Negated();
    }

    /// <inheritdoc />
    public void Serialize(Serializer serializer)
    {
        serializer.Serialize(ref data);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return ToString("YMD");
    }

    /// <summary>
    ///     Formats the date according to the given type.
    ///     <list type="bullet">
    ///         <item><c>D</c> - Day only.</item>
    ///         <item><c>M</c> - Month only.</item>
    ///         <item><c>Y</c> - Year only.</item>
    ///         <item><c>MD</c> - Month and day.</item>
    ///         <item><c>YM</c> - Year and month.</item>
    ///         <item><c>YMD</c> - Year, month, and day.</item>
    ///         <item><c>S</c> - Short written date.</item>
    ///         <item><c>L</c> - Long written date, containing the day of the week.</item>
    ///     </list>
    /// </summary>
    public String ToString(String format)
    {
        return format switch
        {
            "D" => $"{Day:00}",
            "M" => $"{Month.ToNumber():00}",
            "Y" => $"{Year:0000}",
            "MD" => $"{Month.ToNumber():00}/{Day:00}",
            "YM" => $"{Year:0000}/{Month.ToNumber():00}",
            "YMD" => $"{Year:0000}/{Month.ToNumber():00}/{Day:00}",
            "S" => $"{Day} {Month.ToShortString()} {Year}",
            "L" => $"{DayOfWeek.ToShortString()}, {Day} {Month.ToLongString()} {Year}",
            _ => throw Exceptions.UnsupportedValue(format)
        };
    }

    #region EQUALITY

    /// <inheritdoc />
    public Boolean Equals(Date other)
    {
        return data == other.data;
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is Date other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return (Int32) data;
    }

    /// <summary>
    ///     The equality operator.
    /// </summary>
    public static Boolean operator ==(Date left, Date right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     The inequality operator.
    /// </summary>
    public static Boolean operator !=(Date left, Date right)
    {
        return !left.Equals(right);
    }

    #endregion EQUALITY

    #region COMPARISON

    /// <inheritdoc />
    public Int32 CompareTo(Date other)
    {
        return data.CompareTo(other.data);
    }

    /// <summary>
    ///     The less-than operator.
    /// </summary>
    public static Boolean operator <(Date left, Date right)
    {
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    ///     The greater-than operator.
    /// </summary>
    public static Boolean operator >(Date left, Date right)
    {
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    ///     The less-than-or-equal operator.
    /// </summary>
    public static Boolean operator <=(Date left, Date right)
    {
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    ///     The greater-than-or-equal operator.
    /// </summary>
    public static Boolean operator >=(Date left, Date right)
    {
        return left.CompareTo(right) >= 0;
    }

    #endregion COMPARISON
}

/// <summary>
///     JSON converter for <see cref="Date" />.
/// </summary>
public class DateJsonConverter : JsonConverter<Date>
{
    /// <inheritdoc />
    public override Date Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Date.FromTotalDaysSinceStart(reader.GetInt32());
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Date value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.TotalDaysSinceStart);
    }
}
