// <copyright file="DayOfWeek.cs" company="VoxelGame">
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
using System.Collections.Generic;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Domain.Chrono;

/// <summary>
///     The days of the week.
/// </summary>
public enum DayOfWeek
{
    /// <summary>
    ///     The first day of the week.
    /// </summary>
    Monday,

    /// <summary>
    ///     The second day of the week.
    /// </summary>
    Tuesday,

    /// <summary>
    ///     The third day of the week.
    /// </summary>
    Wednesday,

    /// <summary>
    ///     The fourth day of the week.
    /// </summary>
    Thursday,

    /// <summary>
    ///     The fifth day of the week.
    /// </summary>
    Friday,

    /// <summary>
    ///     The sixth day of the week.
    /// </summary>
    Saturday,

    /// <summary>
    ///     The seventh day of the week.
    /// </summary>
    Sunday
}

/// <summary>
///     Extensions for <see cref="DayOfWeek" />.
/// </summary>
public static class DaysOfWeek
{
    /// <summary>
    ///     Get all days of the week.
    /// </summary>
    public static IEnumerable<DayOfWeek> All { get; } = Enum.GetValues<DayOfWeek>();

    /// <summary>
    ///     Gets the number representation of the day of the week, starting from 1.
    /// </summary>
    public static Int32 ToNumber(this DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => 1,
            DayOfWeek.Tuesday => 2,
            DayOfWeek.Wednesday => 3,
            DayOfWeek.Thursday => 4,
            DayOfWeek.Friday => 5,
            DayOfWeek.Saturday => 6,
            DayOfWeek.Sunday => 7,
            _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, message: null)
        };
    }

    /// <summary>
    ///     Returns the next day of the week.
    /// </summary>
    /// <param name="dayOfWeek">The current day of the week.</param>
    /// <returns>The next day of the week.</returns>
    public static DayOfWeek Next(this DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => DayOfWeek.Tuesday,
            DayOfWeek.Tuesday => DayOfWeek.Wednesday,
            DayOfWeek.Wednesday => DayOfWeek.Thursday,
            DayOfWeek.Thursday => DayOfWeek.Friday,
            DayOfWeek.Friday => DayOfWeek.Saturday,
            DayOfWeek.Saturday => DayOfWeek.Sunday,
            DayOfWeek.Sunday => DayOfWeek.Monday,
            _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, message: null)
        };
    }

    /// <summary>
    ///     Returns the previous day of the week.
    /// </summary>
    /// <param name="dayOfWeek">The current day of the week.</param>
    /// <returns>The previous day of the week.</returns>
    public static DayOfWeek Previous(this DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => DayOfWeek.Sunday,
            DayOfWeek.Tuesday => DayOfWeek.Monday,
            DayOfWeek.Wednesday => DayOfWeek.Tuesday,
            DayOfWeek.Thursday => DayOfWeek.Wednesday,
            DayOfWeek.Friday => DayOfWeek.Thursday,
            DayOfWeek.Saturday => DayOfWeek.Friday,
            DayOfWeek.Sunday => DayOfWeek.Saturday,
            _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, message: null)
        };
    }

    /// <summary>
    ///     Gets the string representation of the day of the week.
    /// </summary>
    public static String ToLongString(this DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => Language.DayMonday,
            DayOfWeek.Tuesday => Language.DayTuesday,
            DayOfWeek.Wednesday => Language.DayWednesday,
            DayOfWeek.Thursday => Language.DayThursday,
            DayOfWeek.Friday => Language.DayFriday,
            DayOfWeek.Saturday => Language.DaySaturday,
            DayOfWeek.Sunday => Language.DaySunday,
            _ => throw Exceptions.UnsupportedEnumValue(dayOfWeek)
        };
    }

    /// <summary>
    ///     Gets the short string representation of the day of the week.
    /// </summary>
    public static String ToShortString(this DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => Language.DayMondayShort,
            DayOfWeek.Tuesday => Language.DayTuesdayShort,
            DayOfWeek.Wednesday => Language.DayWednesdayShort,
            DayOfWeek.Thursday => Language.DayThursdayShort,
            DayOfWeek.Friday => Language.DayFridayShort,
            DayOfWeek.Saturday => Language.DaySaturdayShort,
            DayOfWeek.Sunday => Language.DaySundayShort,
            _ => throw Exceptions.UnsupportedEnumValue(dayOfWeek)
        };
    }
}
