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
///     A duration is a length of time, measured in game updates.
///     Compare with <see cref="Period" />.
/// </summary>
public readonly struct Duration : IEquatable<Duration>, IComparable<Duration>
{
    /// <summary>
    ///     Create a duration from a number of updates.
    /// </summary>
    /// <param name="updates">The number of updates in this duration. May be negative.</param>
    private Duration(Int64 updates)
    {
        TotalUpdates = updates;
    }

    /// <summary>
    ///     Create a duration from a number of updates.
    /// </summary>
    /// <param name="updates">The number of updates in this duration. May be negative.</param>
    public static Duration FromUpdates(Int64 updates)
    {
        return new Duration(updates);
    }

    /// <summary>
    ///     Create a duration from a number of seconds.
    /// </summary>
    /// <param name="seconds">The number of seconds in this duration. May be negative.</param>
    public static Duration FromSeconds(Int32 seconds)
    {
        return new Duration(seconds * Calendar.UpdatesPerSecond);
    }

    /// <summary>
    ///     Create a duration from a number of minutes.
    /// </summary>
    /// <param name="minutes">The number of minutes in this duration. May be negative.</param>
    public static Duration FromMinutes(Int32 minutes)
    {
        return new Duration(minutes * Calendar.UpdatesPerMinute);
    }

    /// <summary>
    ///     Create a duration from a number of hours.
    /// </summary>
    /// <param name="hours">The number of hours in this duration. May be negative.</param>
    public static Duration FromHours(Int32 hours)
    {
        return new Duration(hours * Calendar.UpdatesPerHour);
    }

    /// <summary>
    ///     Create a duration from a number of days.
    /// </summary>
    /// <param name="days">The number of days in this duration. May be negative.</param>
    public static Duration FromDays(Int32 days)
    {
        return new Duration(days * Calendar.UpdatesPerDay);
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
        return new Duration(MathTools.RoundedToInt(totalSeconds * Calendar.UpdatesPerSecond));
    }

    /// <summary>
    ///     Create a duration from a total number of minutes.
    /// </summary>
    /// <param name="totalMinutes">The total number of minutes in this duration. May be negative.</param>
    public static Duration FromTotalMinutes(Double totalMinutes)
    {
        return new Duration(MathTools.RoundedToInt(totalMinutes * Calendar.UpdatesPerMinute));
    }

    /// <summary>
    ///     Create a duration from a total number of hours.
    /// </summary>
    /// <param name="totalHours">The total number of hours in this duration. May be negative.</param>
    public static Duration FromTotalHours(Double totalHours)
    {
        return new Duration(MathTools.RoundedToInt(totalHours * Calendar.UpdatesPerHour));
    }

    /// <summary>
    ///     Create a duration from a total number of days.
    /// </summary>
    /// <param name="totalDays">The total number of days in this duration. May be negative.</param>
    public static Duration FromTotalDays(Double totalDays)
    {
        return new Duration(MathTools.RoundedToInt(totalDays * Calendar.UpdatesPerDay));
    }

    /// <summary>
    ///     Create a duration from a total number of weeks.
    /// </summary>
    /// <param name="totalWeeks">The total number of weeks in this duration. May be negative.</param>
    public static Duration FromTotalWeeks(Double totalWeeks)
    {
        return new Duration(MathTools.RoundedToInt(totalWeeks * Calendar.DaysPerWeek * Calendar.UpdatesPerDay));
    }

    /// <summary>
    ///     Create a duration from a total number of months.
    /// </summary>
    /// <param name="totalMonths">The total number of months in this duration. May be negative.</param>
    public static Duration FromTotalMonths(Double totalMonths)
    {
        return new Duration(MathTools.RoundedToInt(totalMonths * Calendar.DaysPerMonth * Calendar.UpdatesPerDay));
    }

    /// <summary>
    ///     Create a duration from a total number of years.
    /// </summary>
    /// <param name="totalYears">The total number of years in this duration. May be negative.</param>
    public static Duration FromTotalYears(Double totalYears)
    {
        return new Duration(MathTools.RoundedToInt(totalYears * Calendar.DaysPerYear * Calendar.UpdatesPerDay));
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
    ///     Get a duration of one update.
    /// </summary>
    public static Duration Update => FromUpdates(1);

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
    public Boolean IsZero => TotalUpdates == 0;

    /// <summary>
    ///     Whether this duration is negative.
    /// </summary>
    public Boolean IsNegative => TotalUpdates < 0;

    /// <summary>
    ///     The total number of updates in this duration.
    /// </summary>
    public Int64 TotalUpdates { get; }

    /// <summary>
    ///     The second component of this duration, not including full minutes.
    /// </summary>
    public Int64 Seconds => TotalUpdates % Calendar.UpdatesPerMinute / Calendar.UpdatesPerSecond;

    /// <summary>
    ///     The minute component of this duration, not including full hours.
    /// </summary>
    public Int64 Minutes => TotalUpdates % Calendar.UpdatesPerHour / Calendar.UpdatesPerMinute;

    /// <summary>
    ///     The hour component of this duration, not including full days.
    /// </summary>
    public Int64 Hours => TotalUpdates % Calendar.UpdatesPerDay / Calendar.UpdatesPerHour;

    /// <summary>
    ///     The day component of this duration, not including full weeks.
    /// </summary>
    public Int64 Days => TotalUpdates % (Calendar.UpdatesPerDay * Calendar.DaysPerWeek) / Calendar.UpdatesPerDay;

    /// <summary>
    ///     The week component of this duration, not including full months.
    /// </summary>
    public Int64 Weeks => TotalUpdates % (Calendar.UpdatesPerDay * Calendar.DaysPerMonth) / (Calendar.UpdatesPerDay * Calendar.DaysPerWeek);

    /// <summary>
    ///     The month component of this duration, not including full years.
    /// </summary>
    public Int64 Months => TotalUpdates % (Calendar.UpdatesPerDay * Calendar.DaysPerYear) / (Calendar.UpdatesPerDay * Calendar.DaysPerMonth);

    /// <summary>
    ///     The year component of this duration.
    /// </summary>
    public Int64 Years => TotalUpdates / (Calendar.UpdatesPerDay * Calendar.DaysPerYear);

    /// <summary>
    ///     The total number of seconds in this duration.
    /// </summary>
    public Double TotalSeconds => TotalUpdates / (Double) Calendar.UpdatesPerSecond;

    /// <summary>
    ///     The total number of minutes in this duration.
    /// </summary>
    public Double TotalMinutes => TotalUpdates / (Double) Calendar.UpdatesPerMinute;

    /// <summary>
    ///     The total number of hours in this duration.
    /// </summary>
    public Double TotalHours => TotalUpdates / (Double) Calendar.UpdatesPerHour;

    /// <summary>
    ///     The total number of days in this duration.
    /// </summary>
    public Double TotalDays => TotalUpdates / (Double) Calendar.UpdatesPerDay;

    /// <summary>
    ///     The total number of weeks in this duration.
    /// </summary>
    public Double TotalWeeks => TotalUpdates / (Double) (Calendar.UpdatesPerDay * Calendar.DaysPerWeek);

    /// <summary>
    ///     The total number of months in this duration.
    /// </summary>
    public Double TotalMonths => TotalUpdates / (Double) (Calendar.UpdatesPerDay * Calendar.DaysPerMonth);

    /// <summary>
    ///     The total number of years in this duration.
    /// </summary>
    public Double TotalYears => TotalUpdates / (Double) (Calendar.UpdatesPerDay * Calendar.DaysPerYear);

    /// <summary>
    ///     Get the negated duration.
    /// </summary>
    public Duration Negated()
    {
        return new Duration(-TotalUpdates);
    }

    /// <summary>
    ///     Get the absolute duration.
    /// </summary>
    public Duration Absolute()
    {
        return TotalUpdates < 0 ? Negated() : this;
    }

    /// <summary>
    ///     Add two durations.
    /// </summary>
    public static Duration operator +(Duration a, Duration b)
    {
        return new Duration(a.TotalUpdates + b.TotalUpdates);
    }

    /// <summary>
    ///     Subtract two durations.
    /// </summary>
    public static Duration operator -(Duration a, Duration b)
    {
        return new Duration(a.TotalUpdates - b.TotalUpdates);
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
        return new Duration(duration.TotalUpdates * factor);
    }

    /// <summary>
    ///     Multiply a duration by a factor.
    /// </summary>
    public static Duration operator *(Int32 factor, Duration duration)
    {
        return new Duration(duration.TotalUpdates * factor);
    }

    /// <inheritdoc cref="operator *(Duration, Int32)" />
    public static Duration Multiply(Duration duration, Int32 factor)
    {
        return new Duration(duration.TotalUpdates * factor);
    }

    /// <inheritdoc cref="operator *(Int32, Duration)" />
    public static Duration Multiply(Int32 factor, Duration duration)
    {
        return new Duration(duration.TotalUpdates * factor);
    }

    /// <summary>
    ///     Divide a duration by a factor.
    /// </summary>
    public static Duration operator /(Duration duration, Int32 factor)
    {
        return new Duration(duration.TotalUpdates / factor);
    }

    /// <inheritdoc cref="operator /(Duration, Int32)" />
    public static Duration Divide(Duration duration, Int32 factor)
    {
        return new Duration(duration.TotalUpdates / factor);
    }

    /// <summary>
    ///     Multiply a duration by a floating point factor.
    /// </summary>
    public static Duration operator *(Duration duration, Double factor)
    {
        return new Duration(MathTools.RoundedToInt(duration.TotalUpdates * factor));
    }

    /// <summary>
    ///     Multiply a duration by a floating point factor.
    /// </summary>
    public static Duration operator *(Double factor, Duration duration)
    {
        return new Duration(MathTools.RoundedToInt(duration.TotalUpdates * factor));
    }

    /// <summary>
    ///     Divide a duration by a floating point factor.
    /// </summary>
    public static Duration operator /(Duration duration, Double factor)
    {
        return new Duration(MathTools.RoundedToInt(duration.TotalUpdates / factor));
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
    ///         <item><c>F</c> - Full duration, excluding updates not fitting into seconds.</item>
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
        return a.TotalUpdates == b.TotalUpdates;
    }

    /// <summary>
    ///     Check if two durations are not equal.
    /// </summary>
    public static Boolean operator !=(Duration a, Duration b)
    {
        return a.TotalUpdates != b.TotalUpdates;
    }

    /// <inheritdoc />
    public Boolean Equals(Duration other)
    {
        return TotalUpdates == other.TotalUpdates;
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is Duration duration && Equals(duration);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return TotalUpdates.GetHashCode();
    }

    #endregion EQUALITY

    #region COMPARISON

    /// <inheritdoc />
    public Int32 CompareTo(Duration other)
    {
        return TotalUpdates.CompareTo(other.TotalUpdates);
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
