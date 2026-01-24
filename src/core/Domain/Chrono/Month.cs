// <copyright file="Month.cs" company="VoxelGame">
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
///     The four months of the year, named after the seasons.
/// </summary>
public enum Month
{
    /// <summary>
    ///     The first month of the year.
    /// </summary>
    Spring,

    /// <summary>
    ///     The second month of the year.
    /// </summary>
    Summer,

    /// <summary>
    ///     The third month of the year.
    /// </summary>
    Autumn,

    /// <summary>
    ///     The fourth month of the year.
    /// </summary>
    Winter
}

/// <summary>
///     Further utilities for months.
/// </summary>
public static class Months
{
    /// <summary>
    ///     Gets all months.
    /// </summary>
    public static IEnumerable<Month> All { get; } = Enum.GetValues<Month>();

    /// <summary>
    ///     Returns the next month.
    /// </summary>
    /// <param name="month">The current month.</param>
    /// <returns>The next month.</returns>
    public static Month Next(this Month month)
    {
        return month switch
        {
            Month.Spring => Month.Summer,
            Month.Summer => Month.Autumn,
            Month.Autumn => Month.Winter,
            Month.Winter => Month.Spring,
            _ => throw Exceptions.UnsupportedEnumValue(month)
        };
    }

    /// <summary>
    ///     Returns the previous month.
    /// </summary>
    /// <param name="month">The current month.</param>
    /// <returns>The previous month.</returns>
    public static Month Previous(this Month month)
    {
        return month switch
        {
            Month.Spring => Month.Winter,
            Month.Summer => Month.Spring,
            Month.Autumn => Month.Summer,
            Month.Winter => Month.Autumn,
            _ => throw Exceptions.UnsupportedEnumValue(month)
        };
    }

    /// <summary>
    ///     Gets the name of the month.
    /// </summary>
    public static String ToLongString(this Month month)
    {
        return month switch
        {
            Month.Spring => Language.MonthSpring,
            Month.Summer => Language.MonthSummer,
            Month.Autumn => Language.MonthAutumn,
            Month.Winter => Language.MonthWinter,
            _ => throw Exceptions.UnsupportedEnumValue(month)
        };
    }

    /// <summary>
    ///     Gets the short name of the month.
    /// </summary>
    public static String ToShortString(this Month month)
    {
        return month switch
        {
            Month.Spring => Language.MonthSpringShort,
            Month.Summer => Language.MonthSummerShort,
            Month.Autumn => Language.MonthAutumnShort,
            Month.Winter => Language.MonthWinterShort,
            _ => throw Exceptions.UnsupportedEnumValue(month)
        };
    }

    /// <summary>
    ///     Gets the month as a representative number, starting from 1.
    /// </summary>
    public static Int32 ToNumber(this Month month)
    {
        return month switch
        {
            Month.Spring => 1,
            Month.Summer => 2,
            Month.Autumn => 3,
            Month.Winter => 4,
            _ => throw Exceptions.UnsupportedEnumValue(month)
        };
    }

    /// <summary>
    ///     Inverse of <see cref="Months.ToNumber" />.
    /// </summary>
    public static Month FromNumber(Int32 number)
    {
        return number switch
        {
            1 => Month.Spring,
            2 => Month.Summer,
            3 => Month.Autumn,
            4 => Month.Winter,
            _ => throw Exceptions.UnsupportedValue(number)
        };
    }
}
