// <copyright file="Period.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Domain.Chrono;

/// <summary>
///     A period is a length of time, measured in calendar units.
///     See <see cref="Duration" /> for a time span measured in absolute time units (ticks).
/// </summary>
public readonly struct Period : IEquatable<Period>, IComparable<Period>
{
    private const MidpointRounding RoundingMode = MidpointRounding.ToEven;

    private Period(Int32 totalDays)
    {
        TotalDays = totalDays;
    }

    /// <summary>
    ///     Create a <see cref="Period" /> from a total number of days.
    /// </summary>
    /// <param name="totalDays">The total number of days, may be negative.</param>
    /// <returns>The corresponding period.</returns>
    public static Period FromDays(Int32 totalDays)
    {
        return new Period(totalDays);
    }

    /// <summary>
    ///     Create a <see cref="Period" /> from a number of weeks.
    /// </summary>
    /// <param name="totalWeeks">The total number of weeks, may be negative.</param>
    /// <returns>The corresponding period.</returns>
    public static Period FromWeeks(Int32 totalWeeks)
    {
        return FromDays(totalWeeks * Calendar.DaysPerWeek);
    }

    /// <summary>
    ///     Create a <see cref="Period" /> from a number of months.
    /// </summary>
    /// <param name="totalMonths">The total number of months, may be negative.</param>
    /// <returns>The corresponding period.</returns>
    public static Period FromMonths(Int32 totalMonths)
    {
        return FromDays(totalMonths * Calendar.DaysPerMonth);
    }

    /// <summary>
    ///     Create a <see cref="Period" /> from a number of years.
    /// </summary>
    /// <param name="totalYears">The total number of years, may be negative.</param>
    /// <returns>The corresponding period.</returns>
    public static Period FromYears(Int32 totalYears)
    {
        return FromDays(totalYears * Calendar.DaysPerYear);
    }

    /// <summary>
    ///     Create a <see cref="Period" /> from a total number of weeks.
    /// </summary>
    /// <param name="totalWeeks">The total number of weeks, may be negative.</param>
    /// <returns>The corresponding period.</returns>
    public static Period FromTotalWeeks(Double totalWeeks)
    {
        return FromDays((Int32) Math.Round(totalWeeks * Calendar.DaysPerWeek, RoundingMode));
    }

    /// <summary>
    ///     Create a <see cref="Period" /> from a total number of months.
    /// </summary>
    /// <param name="totalMonths">The total number of months, may be negative.</param>
    /// <returns>The corresponding period.</returns>
    public static Period FromTotalMonths(Double totalMonths)
    {
        return FromDays((Int32) Math.Round(totalMonths * Calendar.DaysPerMonth, RoundingMode));
    }

    /// <summary>
    ///     Create a <see cref="Period" /> from a total number of years.
    /// </summary>
    /// <param name="totalYears">The total number of years, may be negative.</param>
    /// <returns>The corresponding period.</returns>
    public static Period FromTotalYears(Double totalYears)
    {
        return FromDays((Int32) Math.Round(totalYears * Calendar.DaysPerYear, RoundingMode));
    }

    /// <summary>
    ///     Get the total number of days in this period.
    /// </summary>
    public Int32 TotalDays { get; }

    /// <summary>
    ///     Get the number of days in this period, excluding full weeks.
    /// </summary>
    public Int32 Days => TotalDays % Calendar.DaysPerWeek;

    /// <summary>
    ///     Get the number of weeks in this period, excluding full months.
    /// </summary>
    public Int32 Weeks => TotalDays % Calendar.DaysPerMonth / Calendar.DaysPerWeek;

    /// <summary>
    ///     Get the number of months in this period, excluding full years.
    /// </summary>
    public Int32 Months => TotalDays % Calendar.DaysPerYear / Calendar.DaysPerMonth;

    /// <summary>
    ///     Get the number of full years in this period.
    /// </summary>
    public Int32 Years => TotalDays / Calendar.DaysPerYear;

    /// <summary>
    ///     Get the total number of weeks in this period.
    /// </summary>
    public Double TotalWeeks => TotalDays / (Double) Calendar.DaysPerWeek;

    /// <summary>
    ///     Get the total number of months in this period.
    /// </summary>
    public Double TotalMonths => TotalDays / (Double) Calendar.DaysPerMonth;

    /// <summary>
    ///     Get the total number of years in this period.
    /// </summary>
    public Double TotalYears => TotalDays / (Double) Calendar.DaysPerYear;

    /// <summary>
    ///     Check if this period is zero.
    /// </summary>
    public Boolean IsZero => TotalDays == 0;

    /// <summary>
    ///     Check if this period is positive.
    /// </summary>
    public Boolean IsPositive => TotalDays > 0;

    /// <summary>
    ///     Check if this period is negative.
    /// </summary>
    public Boolean IsNegative => TotalDays < 0;

    /// <summary>
    ///     Convert this period to a duration.
    /// </summary>
    public Duration ToDuration()
    {
        return Duration.FromDays(TotalDays);
    }

    /// <summary>
    ///     Get an absolute version of this period.
    /// </summary>
    /// <returns>The absolute period.</returns>
    public Period Absolute()
    {
        return new Period(Math.Abs(TotalDays));
    }

    /// <summary>
    ///     Get a negated version of this period.
    /// </summary>
    /// <returns>>The negated period.</returns>
    public Period Negated()
    {
        return new Period(-TotalDays);
    }

    /// <summary>
    ///     Get the negation of a period.
    /// </summary>
    public static Period operator -(Period period)
    {
        return period.Negated();
    }

    /// <summary>
    ///     Subtract two periods.
    /// </summary>
    public static Period operator -(Period left, Period right)
    {
        return FromDays(left.TotalDays - right.TotalDays);
    }

    /// <summary>
    ///     Add two periods together.
    /// </summary>
    public static Period operator +(Period left, Period right)
    {
        return FromDays(left.TotalDays + right.TotalDays);
    }

    /// <summary>
    ///     Multiply a period by a factor.
    /// </summary>
    public static Period operator *(Period duration, Int32 factor)
    {
        return new Period(duration.TotalDays * factor);
    }

    /// <summary>
    ///     Multiply a period by a factor.
    /// </summary>
    public static Period operator *(Int32 factor, Period duration)
    {
        return new Period(duration.TotalDays * factor);
    }

    /// <summary>
    ///     Divide a period by a factor.
    /// </summary>
    public static Period operator /(Period duration, Int32 factor)
    {
        return new Period(duration.TotalDays / factor);
    }

    /// <summary>
    ///     Multiply a period by a floating point factor.
    /// </summary>
    public static Period operator *(Period duration, Double factor)
    {
        return new Period((Int32) Math.Round(duration.TotalDays * factor, RoundingMode));
        // todo: check if MathTools has a utility for this
    }

    /// <summary>
    ///     Multiply a period by a floating point factor.
    /// </summary>
    public static Period operator *(Double factor, Period duration)
    {
        return new Period((Int32) Math.Round(duration.TotalDays * factor, RoundingMode));
    }

    /// <summary>
    ///     Divide a period by a floating point factor.
    /// </summary>
    public static Period operator /(Period duration, Double factor)
    {
        return new Period((Int32) Math.Round(duration.TotalDays / factor, RoundingMode));
    }

    /// <inheritdoc cref="operator -(Period, Period)" />
    public static Period Subtract(Period left, Period right)
    {
        return left - right;
    }

    /// <inheritdoc cref="operator +(Period, Period)" />
    public static Period Add(Period left, Period right)
    {
        return left + right;
    }

    /// <inheritdoc cref="operator *(Period, Int32)" />
    public static Period Multiply(Period duration, Int32 factor)
    {
        return duration * factor;
    }

    /// <inheritdoc cref="operator *(Period, Double)" />
    public static Period Multiply(Period duration, Double factor)
    {
        return duration * factor;
    }

    /// <inheritdoc cref="operator /(Period, Int32)" />
    public static Period Divide(Period duration, Int32 factor)
    {
        return duration / factor;
    }

    /// <inheritdoc cref="operator /(Period, Double)" />
    public static Period Divide(Period duration, Double factor)
    {
        return duration / factor;
    }

    /// <summary>
    ///     Get a period of one day.
    /// </summary>
    public static Period Day => FromDays(1);

    /// <summary>
    ///     Get a period of one month.
    /// </summary>
    public static Period Month => FromMonths(1);

    /// <summary>
    ///     Get a period of one week.
    /// </summary>
    public static Period Week => FromWeeks(1);

    /// <summary>
    ///     Get a period of one year.
    /// </summary>
    public static Period Year => FromYears(1);

    /// <inheritdoc />
    public override String ToString()
    {
        return ToString("F");
    }

    /// <summary>
    ///     Formats the period according to the given type.
    ///     <list type="bullet">
    ///         <item><c>F</c> - Full period, excluding remainder ticks.</item>
    ///         <item><c>S</c> - Simple period, simply the largest component.</item>
    ///     </list>
    /// </summary>
    public String ToString(String format)
    {
        if (String.IsNullOrEmpty(format) || format.Length != 1 || format != "S")
            return GetFullDurationString();

        return IsZero ? "0d" : GetSimpleDurationString();
    }

    private String GetFullDurationString()
    {
        Period absolute = Absolute();

        return $"{(IsNegative ? "-" : "")}{absolute.Years}.{absolute.Months:0}.{absolute.Weeks:0}.{absolute.Days:00}";
    }

    private String GetSimpleDurationString()
    {
        Boolean isNegative = IsNegative;
        Period absolute = Absolute();

        if (absolute.Years > 0)
            return GetSignedOutput("y", absolute.Years);

        if (absolute.Months > 0)
            return GetSignedOutput("mo", absolute.Months);

        if (absolute.Weeks > 0)
            return GetSignedOutput("w", absolute.Weeks);

        if (absolute.Days > 0)
            return GetSignedOutput("d", absolute.Days);

        return "~0d";

        String GetSignedOutput(String unit, Int64 value)
        {
            return isNegative ? $"-{value}{unit}" : $"{value}{unit}";
        }
    }

    #region EQUALITY

    /// <inheritdoc />
    public Boolean Equals(Period other)
    {
        return TotalDays == other.TotalDays;
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is Period other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return TotalDays.GetHashCode();
    }

    /// <summary>
    ///     The equality operator.
    /// </summary>
    public static Boolean operator ==(Period left, Period right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     The inequality operator.
    /// </summary>
    public static Boolean operator !=(Period left, Period right)
    {
        return !left.Equals(right);
    }

    #endregion EQUALITY

    #region COMPARISON

    /// <inheritdoc />
    public Int32 CompareTo(Period other)
    {
        return TotalDays.CompareTo(other.TotalDays);
    }

    /// <summary>
    ///     The less-than operator.
    /// </summary>
    public static Boolean operator <(Period left, Period right)
    {
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    ///     The greater-than operator.
    /// </summary>
    public static Boolean operator >(Period left, Period right)
    {
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    ///     The less-than-or-equal operator.
    /// </summary>
    public static Boolean operator <=(Period left, Period right)
    {
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    ///     The greater-than-or-equal operator.
    /// </summary>
    public static Boolean operator >=(Period left, Period right)
    {
        return left.CompareTo(right) >= 0;
    }

    #endregion COMPARISON
}
