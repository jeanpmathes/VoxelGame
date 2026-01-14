// <copyright file="DateAndTime.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Domain.Chrono;

/// <summary>
///     This struct combines a <see cref="Date" /> and a <see cref="Time" />.
/// </summary>
public readonly struct DateAndTime : IEquatable<DateAndTime>, IComparable<DateAndTime>
{
    /// <summary>
    ///     The date.
    /// </summary>
    public Date Date { get; init; }

    /// <summary>
    ///     The time.
    /// </summary>
    public Time Time { get; init; }

    /// <summary>
    ///     Create a new date and time.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="time">The time.</param>
    public DateAndTime(Date date, Time time)
    {
        Date = date;
        Time = time;
    }

    /// <summary>
    ///     Create a new date and time from the given total updates since the start of the calendar.
    /// </summary>
    /// <param name="updates">The total updates since the start of the calendar, must be non-negative.</param>
    /// <returns>The created date and time.</returns>
    public static DateAndTime FromUpdates(Int64 updates)
    {
        Debug.Assert(updates >= 0);

        Int64 days = updates / Calendar.UpdatesPerDay;
        updates -= days * Calendar.UpdatesPerDay;

        Date date = Date.FromTotalDaysSinceStart((Int32) days);
        Time time = new(updates);

        return new DateAndTime(date, time);
    }

    /// <summary>
    ///     Get a <see cref="DateAndTime" /> at the start of the given <see cref="Date" />.
    /// </summary>
    public static DateAndTime AtStartOfDay(Date date)
    {
        return new DateAndTime(date, Time.StartOfDay);
    }

    /// <summary>
    ///     Get a <see cref="DateAndTime" /> at the end of the given <see cref="Date" />.
    /// </summary>
    public static DateAndTime AtEndOfDay(Date date)
    {
        return new DateAndTime(date, Time.EndOfDay);
    }

    /// <summary>
    ///     Get the <see cref="DateAndTime" /> at the start of this day.
    /// </summary>
    public DateAndTime StartOfDay => AtStartOfDay(Date);

    /// <summary>
    ///     Get the <see cref="DateAndTime" /> at the end of this day.
    /// </summary>
    public DateAndTime EndOfDay => AtEndOfDay(Date);

    private Int64 TotalUpdates => Date.TotalDaysSinceStart * Calendar.UpdatesPerDay + Time.TotalUpdates;

    /// <summary>
    ///     Get the duration until another <see cref="DateAndTime" />.
    /// </summary>
    /// <param name="other">The other date and time.</param>
    /// <returns>The duration until the other date and time.</returns>
    public Duration Until(DateAndTime other)
    {
        return other - this;
    }

    /// <summary>
    ///     Subtract two <see cref="DateAndTime" /> instances.
    /// </summary>
    public static Duration operator -(DateAndTime left, DateAndTime right)
    {
        return Duration.FromUpdates(left.TotalUpdates - right.TotalUpdates);
    }

    /// <inheritdoc cref="operator -(DateAndTime, DateAndTime)" />
    public static Duration Subtract(DateAndTime left, DateAndTime right)
    {
        return left - right;
    }

    /// <summary>
    ///     Add a <see cref="Duration" /> to a <see cref="DateAndTime" />.
    /// </summary>
    public static DateAndTime operator +(DateAndTime dateAndTime, Duration duration)
    {
        return FromUpdates(dateAndTime.TotalUpdates + duration.TotalUpdates);
    }

    /// <inheritdoc cref="operator +(DateAndTime, Duration)" />
    public static DateAndTime Add(DateAndTime dateAndTime, Duration duration)
    {
        return dateAndTime + duration;
    }

    /// <summary>
    ///     Subtract a <see cref="Duration" /> from a <see cref="DateAndTime" />.
    /// </summary>
    public static DateAndTime operator -(DateAndTime dateAndTime, Duration duration)
    {
        return FromUpdates(dateAndTime.TotalUpdates - duration.TotalUpdates);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return $"{Date} {Time}";
    }

    #region EQUALITY

    /// <inheritdoc />
    public Boolean Equals(DateAndTime other)
    {
        return Date.Equals(other.Date) && Time.Equals(other.Time);
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is DateAndTime other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return HashCode.Combine(Date, Time);
    }

    /// <summary>
    ///     The equality operator.
    /// </summary>
    public static Boolean operator ==(DateAndTime left, DateAndTime right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     The inequality operator.
    /// </summary>
    public static Boolean operator !=(DateAndTime left, DateAndTime right)
    {
        return !left.Equals(right);
    }

    #endregion EQUALITY

    #region COMPARISON

    /// <inheritdoc />
    public Int32 CompareTo(DateAndTime other)
    {
        return TotalUpdates.CompareTo(other.TotalUpdates);
    }

    /// <summary>
    ///     The less-than operator.
    /// </summary>
    public static Boolean operator <(DateAndTime left, DateAndTime right)
    {
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    ///     The greater-than operator.
    /// </summary>
    public static Boolean operator >(DateAndTime left, DateAndTime right)
    {
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    ///     The less-than-or-equal operator.
    /// </summary>
    public static Boolean operator <=(DateAndTime left, DateAndTime right)
    {
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    ///     The greater-than-or-equal operator.
    /// </summary>
    public static Boolean operator >=(DateAndTime left, DateAndTime right)
    {
        return left.CompareTo(right) >= 0;
    }

    #endregion COMPARISON
}
