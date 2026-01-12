// <copyright file="Duration.cs" company="VoxelGame">
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
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Domain.Chrono;

/// <summary>
///     A duration is a length of time, measured in game ticks.
///     Compare with <see cref="Period" />.
/// </summary>
public readonly struct Duration : IEquatable<Duration>, IComparable<Duration>
{
    /// <summary>
    ///     Create a duration from a number of ticks.
    /// </summary>
    /// <param name="ticks">The number of ticks in this duration. May be negative.</param>
    private Duration(Int64 ticks)
    {
        TotalTicks = ticks;
    }

    /// <summary>
    ///     Create a duration from a number of ticks.
    /// </summary>
    /// <param name="ticks">The number of ticks in this duration. May be negative.</param>
    public static Duration FromTicks(Int64 ticks)
    {
        return new Duration(ticks);
    }

    /// <summary>
    ///     Create a duration from a number of seconds.
    /// </summary>
    /// <param name="seconds">The number of seconds in this duration. May be negative.</param>
    public static Duration FromSeconds(Int32 seconds)
    {
        return new Duration(seconds * Calendar.TicksPerSecond);
    }

    /// <summary>
    ///     Create a duration from a number of minutes.
    /// </summary>
    /// <param name="minutes">The number of minutes in this duration. May be negative.</param>
    public static Duration FromMinutes(Int32 minutes)
    {
        return new Duration(minutes * Calendar.TicksPerMinute);
    }

    /// <summary>
    ///     Create a duration from a number of hours.
    /// </summary>
    /// <param name="hours">The number of hours in this duration. May be negative.</param>
    public static Duration FromHours(Int32 hours)
    {
        return new Duration(hours * Calendar.TicksPerHour);
    }

    /// <summary>
    ///     Create a duration from a number of days.
    /// </summary>
    /// <param name="days">The number of days in this duration. May be negative.</param>
    public static Duration FromDays(Int32 days)
    {
        return new Duration(days * Calendar.TicksPerDay);
    }

    /// <summary>
    ///     Create a duration from a number of weeks.
    /// </summary>
    /// <param name="weeks">The number of weeks in this duration. May be negative.</param>
    public static Duration FromWeeks(Int32 weeks)
    {
        return FromDays(weeks * Calendar.DaysPerWeek);
    }

    /// <summary>
    ///     Create a duration from a number of months.
    /// </summary>
    /// <param name="months">The number of months in this duration. May be negative.</param>
    public static Duration FromMonths(Int32 months)
    {
        return FromDays(months * Calendar.DaysPerMonth);
    }

    /// <summary>
    ///     Create a duration from a number of years.
    /// </summary>
    /// <param name="years">The number of years in this duration. May be negative.</param>
    public static Duration FromYears(Int32 years)
    {
        return FromDays(years * Calendar.DaysPerYear);
    }

    /// <summary>
    ///     Create a duration from a total number of seconds.
    /// </summary>
    /// <param name="totalSeconds">The total number of seconds in this duration. May be negative.</param>
    public static Duration FromTotalSeconds(Double totalSeconds)
    {
        return new Duration(MathTools.RoundedToInt(totalSeconds * Calendar.TicksPerSecond));
    }

    /// <summary>
    ///     Create a duration from a total number of minutes.
    /// </summary>
    /// <param name="totalMinutes">The total number of minutes in this duration. May be negative.</param>
    public static Duration FromTotalMinutes(Double totalMinutes)
    {
        return new Duration(MathTools.RoundedToInt(totalMinutes * Calendar.TicksPerMinute));
    }

    /// <summary>
    ///     Create a duration from a total number of hours.
    /// </summary>
    /// <param name="totalHours">The total number of hours in this duration. May be negative.</param>
    public static Duration FromTotalHours(Double totalHours)
    {
        return new Duration(MathTools.RoundedToInt(totalHours * Calendar.TicksPerHour));
    }

    /// <summary>
    ///     Create a duration from a total number of days.
    /// </summary>
    /// <param name="totalDays">The total number of days in this duration. May be negative.</param>
    public static Duration FromTotalDays(Double totalDays)
    {
        return new Duration(MathTools.RoundedToInt(totalDays * Calendar.TicksPerDay));
    }

    /// <summary>
    ///     Create a duration from a total number of weeks.
    /// </summary>
    /// <param name="totalWeeks">The total number of weeks in this duration. May be negative.</param>
    public static Duration FromTotalWeeks(Double totalWeeks)
    {
        return new Duration(MathTools.RoundedToInt(totalWeeks * Calendar.DaysPerWeek * Calendar.TicksPerDay));
    }

    /// <summary>
    ///     Create a duration from a total number of months.
    /// </summary>
    /// <param name="totalMonths">The total number of months in this duration. May be negative.</param>
    public static Duration FromTotalMonths(Double totalMonths)
    {
        return new Duration(MathTools.RoundedToInt(totalMonths * Calendar.DaysPerMonth * Calendar.TicksPerDay));
    }

    /// <summary>
    ///     Create a duration from a total number of years.
    /// </summary>
    /// <param name="totalYears">The total number of years in this duration. May be negative.</param>
    public static Duration FromTotalYears(Double totalYears)
    {
        return new Duration(MathTools.RoundedToInt(totalYears * Calendar.DaysPerYear * Calendar.TicksPerDay));
    }

    /// <summary>
    ///     Create a duration from multiple components.
    /// </summary>
    public static Duration From(Int32 years = 0, Int32 months = 0, Int32 weeks = 0, Int32 days = 0, Int32 hours = 0, Int32 minutes = 0, Int32 seconds = 0)
    {
        return FromYears(years) +
               FromMonths(months) +
               FromWeeks(weeks) +
               FromDays(days) +
               FromHours(hours) +
               FromMinutes(minutes) +
               FromSeconds(seconds);
    }

    /// <summary>
    ///     Get a duration of zero time.
    /// </summary>
    public static Duration Zero => new(0);

    /// <summary>
    ///     Get a duration of one tick.
    /// </summary>
    public static Duration Tick => FromTicks(1);

    /// <summary>
    ///     Get a duration of one second.
    /// </summary>
    public static Duration Second => FromSeconds(1);

    /// <summary>
    ///     Get a duration of one minute.
    /// </summary>
    public static Duration Minute => FromMinutes(1);

    /// <summary>
    ///     Get a duration of one hour.
    /// </summary>
    public static Duration Hour => FromHours(1);

    /// <summary>
    ///     Get a duration of one day.
    /// </summary>
    public static Duration Day => FromDays(1);

    /// <summary>
    ///     Get a duration of one week.
    /// </summary>
    public static Duration Week => FromWeeks(1);

    /// <summary>
    ///     Get a duration of one month.
    /// </summary>
    public static Duration Month => FromMonths(1);

    /// <summary>
    ///     Get a duration of one year.
    /// </summary>
    public static Duration Year => FromYears(1);

    /// <summary>
    ///     Whether this duration is zero.
    /// </summary>
    public Boolean IsZero => TotalTicks == 0;

    /// <summary>
    ///     Whether this duration is negative.
    /// </summary>
    public Boolean IsNegative => TotalTicks < 0;

    /// <summary>
    ///     The total number of ticks in this duration.
    /// </summary>
    public Int64 TotalTicks { get; }

    /// <summary>
    ///     The second component of this duration, not including full minutes.
    /// </summary>
    public Int64 Seconds => TotalTicks % Calendar.TicksPerMinute / Calendar.TicksPerSecond;

    /// <summary>
    ///     The minute component of this duration, not including full hours.
    /// </summary>
    public Int64 Minutes => TotalTicks % Calendar.TicksPerHour / Calendar.TicksPerMinute;

    /// <summary>
    ///     The hour component of this duration, not including full days.
    /// </summary>
    public Int64 Hours => TotalTicks % Calendar.TicksPerDay / Calendar.TicksPerHour;

    /// <summary>
    ///     The day component of this duration, not including full weeks.
    /// </summary>
    public Int64 Days => TotalTicks % (Calendar.TicksPerDay * Calendar.DaysPerWeek) / Calendar.TicksPerDay;

    /// <summary>
    ///     The week component of this duration, not including full months.
    /// </summary>
    public Int64 Weeks => TotalTicks % (Calendar.TicksPerDay * Calendar.DaysPerMonth) / (Calendar.TicksPerDay * Calendar.DaysPerWeek);

    /// <summary>
    ///     The month component of this duration, not including full years.
    /// </summary>
    public Int64 Months => TotalTicks % (Calendar.TicksPerDay * Calendar.DaysPerYear) / (Calendar.TicksPerDay * Calendar.DaysPerMonth);

    /// <summary>
    ///     The year component of this duration.
    /// </summary>
    public Int64 Years => TotalTicks / (Calendar.TicksPerDay * Calendar.DaysPerYear);

    /// <summary>
    ///     The total number of seconds in this duration.
    /// </summary>
    public Double TotalSeconds => TotalTicks / (Double) Calendar.TicksPerSecond;

    /// <summary>
    ///     The total number of minutes in this duration.
    /// </summary>
    public Double TotalMinutes => TotalTicks / (Double) Calendar.TicksPerMinute;

    /// <summary>
    ///     The total number of hours in this duration.
    /// </summary>
    public Double TotalHours => TotalTicks / (Double) Calendar.TicksPerHour;

    /// <summary>
    ///     The total number of days in this duration.
    /// </summary>
    public Double TotalDays => TotalTicks / (Double) Calendar.TicksPerDay;

    /// <summary>
    ///     The total number of weeks in this duration.
    /// </summary>
    public Double TotalWeeks => TotalTicks / (Double) (Calendar.TicksPerDay * Calendar.DaysPerWeek);

    /// <summary>
    ///     The total number of months in this duration.
    /// </summary>
    public Double TotalMonths => TotalTicks / (Double) (Calendar.TicksPerDay * Calendar.DaysPerMonth);

    /// <summary>
    ///     The total number of years in this duration.
    /// </summary>
    public Double TotalYears => TotalTicks / (Double) (Calendar.TicksPerDay * Calendar.DaysPerYear);

    /// <summary>
    ///     Get the negated duration.
    /// </summary>
    public Duration Negated()
    {
        return new Duration(-TotalTicks);
    }

    /// <summary>
    ///     Get the absolute duration.
    /// </summary>
    public Duration Absolute()
    {
        return TotalTicks < 0 ? Negated() : this;
    }

    /// <summary>
    ///     Add two durations.
    /// </summary>
    public static Duration operator +(Duration a, Duration b)
    {
        return new Duration(a.TotalTicks + b.TotalTicks);
    }

    /// <summary>
    ///     Subtract two durations.
    /// </summary>
    public static Duration operator -(Duration a, Duration b)
    {
        return new Duration(a.TotalTicks - b.TotalTicks);
    }

    /// <summary>
    ///     Add a period to a duration.
    /// </summary>
    public static Duration operator +(Duration duration, Period period)
    {
        return duration + period.ToDuration();
    }

    /// <summary>
    ///     Add a period to a duration.
    /// </summary>
    public static Duration operator +(Period period, Duration duration)
    {
        return period.ToDuration() + duration;
    }

    /// <inheritdoc cref="operator +(Duration, Duration)" />
    public static Duration Add(Duration a, Duration b)
    {
        return a + b;
    }

    /// <inheritdoc cref="operator +(Duration, Period)" />
    public static Duration Add(Duration duration, Period period)
    {
        return duration + period;
    }

    /// <inheritdoc cref="operator +(Period, Duration)" />
    public static Duration Add(Period period, Duration duration)
    {
        return period + duration;
    }

    /// <inheritdoc cref="operator -(Duration, Duration)" />
    public static Duration Subtract(Duration a, Duration b)
    {
        return a - b;
    }

    /// <summary>
    ///     Negate a duration.
    /// </summary>
    public static Duration operator -(Duration duration)
    {
        return duration.Negated();
    }

    /// <inheritdoc cref="operator -(Duration)" />
    public static Duration Negate(Duration duration)
    {
        return duration.Negated();
    }

    /// <summary>
    ///     Multiply a duration by a factor.
    /// </summary>
    public static Duration operator *(Duration duration, Int32 factor)
    {
        return new Duration(duration.TotalTicks * factor);
    }

    /// <summary>
    ///     Multiply a duration by a factor.
    /// </summary>
    public static Duration operator *(Int32 factor, Duration duration)
    {
        return new Duration(duration.TotalTicks * factor);
    }

    /// <inheritdoc cref="operator *(Duration, Int32)" />
    public static Duration Multiply(Duration duration, Int32 factor)
    {
        return new Duration(duration.TotalTicks * factor);
    }

    /// <inheritdoc cref="operator *(Int32, Duration)" />
    public static Duration Multiply(Int32 factor, Duration duration)
    {
        return new Duration(duration.TotalTicks * factor);
    }

    /// <summary>
    ///     Divide a duration by a factor.
    /// </summary>
    public static Duration operator /(Duration duration, Int32 factor)
    {
        return new Duration(duration.TotalTicks / factor);
    }

    /// <inheritdoc cref="operator /(Duration, Int32)" />
    public static Duration Divide(Duration duration, Int32 factor)
    {
        return new Duration(duration.TotalTicks / factor);
    }

    /// <summary>
    ///     Multiply a duration by a floating point factor.
    /// </summary>
    public static Duration operator *(Duration duration, Double factor)
    {
        return new Duration(MathTools.RoundedToInt(duration.TotalTicks * factor));
    }

    /// <summary>
    ///     Multiply a duration by a floating point factor.
    /// </summary>
    public static Duration operator *(Double factor, Duration duration)
    {
        return new Duration(MathTools.RoundedToInt(duration.TotalTicks * factor));
    }

    /// <summary>
    ///     Divide a duration by a floating point factor.
    /// </summary>
    public static Duration operator /(Duration duration, Double factor)
    {
        return new Duration(MathTools.RoundedToInt(duration.TotalTicks / factor));
    }

    /// <summary>
    ///     Clamp a duration between a minimum and maximum duration.
    /// </summary>
    public static Duration Clamp(Duration value, Duration min, Duration max)
    {
        if (value < min) return min;
        if (value > max) return max;

        return value;
    }

    /// <summary>
    ///     Get the minimum of two durations.
    /// </summary>
    public static Duration Min(Duration a, Duration b)
    {
        return a < b ? a : b;
    }

    /// <summary>
    ///     Get the maximum of two durations.
    /// </summary>
    public static Duration Max(Duration a, Duration b)
    {
        return a > b ? a : b;
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return ToString("F");
    }

    /// <summary>
    ///     Formats the duration according to the given type.
    ///     <list type="bullet">
    ///         <item><c>F</c> - Full duration, excluding remainder ticks.</item>
    ///         <item><c>S</c> - Simple duration, simply the largest component.</item>
    ///     </list>
    /// </summary>
    public String ToString(String format)
    {
        if (String.IsNullOrEmpty(format) || format.Length != 1 || format != "S")
            return GetFullDurationString();

        return IsZero ? "0s" : GetSimpleDurationString();
    }

    private String GetFullDurationString()
    {
        Duration absolute = Absolute();

        return $"{(IsNegative ? "-" : "")}{absolute.Years}.{absolute.Months:0}.{absolute.Weeks:0}.{absolute.Days:00}.{absolute.Hours:00}:{absolute.Minutes:00}:{absolute.Seconds:00}";
    }

    private String GetSimpleDurationString()
    {
        Boolean isNegative = IsNegative;
        Duration absolute = Absolute();

        if (absolute.Years > 0)
            return GetSignedOutput("y", absolute.Years);

        if (absolute.Months > 0)
            return GetSignedOutput("mo", absolute.Months);

        if (absolute.Weeks > 0)
            return GetSignedOutput("w", absolute.Weeks);

        if (absolute.Days > 0)
            return GetSignedOutput("d", absolute.Days);

        if (absolute.Hours > 0)
            return GetSignedOutput("h", absolute.Hours);

        if (absolute.Minutes > 0)
            return GetSignedOutput("m", absolute.Minutes);

        if (absolute.Seconds > 0)
            return GetSignedOutput("s", absolute.Seconds);

        return "~0s";

        String GetSignedOutput(String unit, Int64 value)
        {
            return isNegative ? $"-{value}{unit}" : $"{value}{unit}";
        }
    }

    #region EQUALITY

    /// <summary>
    ///     Check if two durations are equal.
    /// </summary>
    public static Boolean operator ==(Duration a, Duration b)
    {
        return a.TotalTicks == b.TotalTicks;
    }

    /// <summary>
    ///     Check if two durations are not equal.
    /// </summary>
    public static Boolean operator !=(Duration a, Duration b)
    {
        return a.TotalTicks != b.TotalTicks;
    }

    /// <inheritdoc />
    public Boolean Equals(Duration other)
    {
        return TotalTicks == other.TotalTicks;
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is Duration duration && Equals(duration);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return TotalTicks.GetHashCode();
    }

    #endregion EQUALITY

    #region COMPARISON

    /// <inheritdoc />
    public Int32 CompareTo(Duration other)
    {
        return TotalTicks.CompareTo(other.TotalTicks);
    }

    /// <summary>
    ///     The less-than operator.
    /// </summary>
    public static Boolean operator <(Duration left, Duration right)
    {
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    ///     The greater-than operator.
    /// </summary>
    public static Boolean operator >(Duration left, Duration right)
    {
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    ///     The less-than-or-equal operator.
    /// </summary>
    public static Boolean operator <=(Duration left, Duration right)
    {
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    ///     The greater-than-or-equal operator.
    /// </summary>
    public static Boolean operator >=(Duration left, Duration right)
    {
        return left.CompareTo(right) >= 0;
    }

    #endregion COMPARISON
}
