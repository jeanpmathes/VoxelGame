// <copyright file="Time.cs" company="VoxelGame">
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
///     A time of day according to <see cref="Calendar" />.
///     This struct does not contain a specific date, only the time of day in hours, minutes, and seconds.
///     It should be used as a starting point for most user-entered times, as well as for repeated events.
/// </summary>
[JsonConverter(typeof(TimeJsonConverter))]
public struct Time : IEquatable<Time>, IComparable<Time>, IValue
{
    /// <summary>
    ///     Updates since start of the day are used.
    ///     The value will be smaller than <see cref="Calendar.UpdatesPerDay" />.
    /// </summary>
    private Int64 totalUpdates;

    /// <summary>
    ///     Create a new time.
    /// </summary>
    /// <param name="hours">The hour of the day. Must be in [0, 23].</param>
    /// <param name="minutes">The minute of the hour. Must be in [0, 59].</param>
    /// <param name="seconds">The second of the minute. Must be in [0, 59].</param>
    public Time(Int32 hours, Int32 minutes, Int32 seconds = 0)
    {
        Debug.Assert(hours is >= 0 and < (Int32) Calendar.HoursPerDay);
        Debug.Assert(minutes is >= 0 and < (Int32) Calendar.MinutesPerHour);
        Debug.Assert(seconds is >= 0 and < (Int32) Calendar.SecondsPerMinute);

        Int64 hourUpdates = hours * Calendar.UpdatesPerHour;
        Int64 minuteUpdates = minutes * Calendar.UpdatesPerMinute;
        Int64 secondUpdates = seconds * Calendar.UpdatesPerSecond;

        totalUpdates = hourUpdates + minuteUpdates + secondUpdates;
    }

    /// <summary>
    ///     Create a new time.
    /// </summary>
    /// <param name="totalUpdates">
    ///     The number of total Updates since the start of the day. Must be smaller than
    ///     <see cref="Calendar.UpdatesPerDay" />.
    /// </param>
    public Time(Int64 totalUpdates)
    {
        Debug.Assert(totalUpdates is >= 0 and < Calendar.UpdatesPerDay);

        this.totalUpdates = totalUpdates;
    }

    /// <summary>
    ///     Get the hour of this time, will not be larger than <see cref="Calendar.HoursPerDay" />.
    /// </summary>
    public Int32 Hours => (Int32) (TotalUpdates / Calendar.UpdatesPerHour);

    /// <summary>
    ///     Get the minute of this time, will not be larger than <see cref="Calendar.MinutesPerHour" />.
    /// </summary>
    public Int32 Minutes => (Int32) (TotalUpdates % Calendar.UpdatesPerHour / Calendar.UpdatesPerMinute);

    /// <summary>
    ///     Get the second of this time, will not be larger than <see cref="Calendar.SecondsPerMinute" />.
    /// </summary>
    public Int32 Seconds => (Int32) (TotalUpdates % Calendar.UpdatesPerMinute / Calendar.UpdatesPerSecond);

    /// <summary>
    ///     Get the total number of seconds since the start of the day.
    /// </summary>
    public Double TotalSeconds => TotalUpdates / (Double) Calendar.UpdatesPerSecond;

    /// <summary>
    ///     Get the time of day as a value between 0.0 and 1.0, relative to sunrise and sunset.
    ///     A value of 0.0 represents sunrise (06:00:00), a value of 0.5 represents sunset (18:00:00).
    /// </summary>
    public Double TimeOfDay => (TotalUpdates - Sunrise.TotalUpdates + Calendar.UpdatesPerDay) % Calendar.UpdatesPerDay / (Double) Calendar.UpdatesPerDay;

    /// <summary>
    ///     Get the total number of updates since the start of the day.
    /// </summary>
    public Int64 TotalUpdates => totalUpdates;

    /// <summary>
    ///     Whether this time is during daytime (between sunrise and sunset).
    /// </summary>
    public Boolean IsDaytime => this >= Sunrise && this < Sunset;

    /// <summary>
    ///     Whether this time is during nighttime (between sunset and sunrise).
    /// </summary>
    public Boolean IsNighttime => !IsDaytime;

    /// <summary>
    ///     Get the start of the day (00:00:00).
    /// </summary>
    public static readonly Time StartOfDay = new(hours: 0, minutes: 0);

    /// <summary>
    ///     Get sunrise (06:00:00).
    /// </summary>
    public static readonly Time Sunrise = new(hours: 6, minutes: 0);

    /// <summary>
    ///     Get noon (12:00:00).
    /// </summary>
    public static readonly Time Noon = new(hours: 12, minutes: 0);

    /// <summary>
    ///     Get sunset (18:00:00).
    /// </summary>
    public static readonly Time Sunset = new(hours: 18, minutes: 0);

    /// <summary>
    ///     Get the end of the day (23:59:59).
    /// </summary>
    public static readonly Time EndOfDay = new(hours: 23, minutes: 59, seconds: 59);

    /// <summary>
    ///     Get the duration between two times.
    /// </summary>
    public static Duration operator -(Time left, Time right)
    {
        return Duration.FromUpdates(left.TotalUpdates - right.TotalUpdates);
    }

    /// <summary>
    ///     Get the time after adding the given duration to this time.
    ///     This wraps around at midnight.
    /// </summary>
    public static Time operator +(Time time, Duration duration)
    {
        return new Time(MathTools.Mod(time.TotalUpdates + duration.TotalUpdates, Calendar.UpdatesPerDay));
    }

    /// <summary>
    ///     Get the time after subtracting the given duration from this time.
    ///     This wraps around at midnight.
    /// </summary>
    public static Time operator -(Time time, Duration duration)
    {
        return time + duration.Negated();
    }

    /// <inheritdoc cref="operator -(Time, Time)" />
    public static Duration Subtract(Time left, Time right)
    {
        return left - right;
    }

    /// <inheritdoc cref="operator -(Time, Duration)" />
    public static Time Subtract(Time time, Duration duration)
    {
        return time - duration;
    }

    /// <inheritdoc cref="operator +(Time, Duration)" />
    public static Time Add(Time time, Duration duration)
    {
        return time + duration;
    }

    /// <summary>
    ///     Get the duration from this time until the other time.
    /// </summary>
    public Duration Until(Time other)
    {
        return other - this;
    }

    /// <summary>
    ///     Check whether this time is between the given start and end time.
    /// </summary>
    public Boolean IsBetween(Time start, Time end)
    {
        if (start <= end)
        {
            return this >= start && this <= end;
        }

        return this >= start || this <= end;
    }

    /// <inheritdoc />
    public void Serialize(Serializer serializer)
    {
        serializer.Serialize(ref totalUpdates);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return ToString("S");
    }

    /// <summary>
    ///     Get a string representation of this time, using the given format.
    /// </summary>
    /// <param name="format">Can be S for short or L for long.</param>
    public String ToString(String format)
    {
        return format switch
        {
            "S" => $"{Hours:00}:{Minutes:00}",
            "L" => $"{Hours:00}:{Minutes:00}:{Seconds:00}",
            _ => throw Exceptions.UnsupportedValue(format)
        };
    }

    #region EQUALITY

    /// <inheritdoc />
    public Boolean Equals(Time other)
    {
        return TotalUpdates == other.TotalUpdates;
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is Time other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return TotalUpdates.GetHashCode();
    }

    /// <summary>
    ///     The equality operator.
    /// </summary>
    public static Boolean operator ==(Time left, Time right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     The inequality operator.
    /// </summary>
    public static Boolean operator !=(Time left, Time right)
    {
        return !left.Equals(right);
    }

    #endregion EQUALITY

    #region COMPARISON

    /// <inheritdoc />
    public Int32 CompareTo(Time other)
    {
        return TotalUpdates.CompareTo(other.TotalUpdates);
    }

    /// <summary>
    ///     The less-than operator.
    /// </summary>
    public static Boolean operator <(Time left, Time right)
    {
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    ///     The greater-than operator.
    /// </summary>
    public static Boolean operator >(Time left, Time right)
    {
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    ///     The less-than-or-equal operator.
    /// </summary>
    public static Boolean operator <=(Time left, Time right)
    {
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    ///     The greater-than-or-equal operator.
    /// </summary>
    public static Boolean operator >=(Time left, Time right)
    {
        return left.CompareTo(right) >= 0;
    }

    #endregion COMPARISON
}

/// <summary>
///     JSON converter for <see cref="Time" />.
/// </summary>
public class TimeJsonConverter : JsonConverter<Time>
{
    /// <inheritdoc />
    public override Time Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new Time(reader.GetInt64());
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Time value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.TotalUpdates);
    }
}
