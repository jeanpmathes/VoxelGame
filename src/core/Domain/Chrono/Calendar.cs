// <copyright file="Calendar.cs" company="VoxelGame">
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
///     Defines the calendar system.
///     The basic unit of time is a simulation update step, referred to as an "update".
///     All units in this class, if not specified otherwise, are in-game units and not real-time units.
/// </summary>
public static class Calendar
{
    /// <summary>
    ///     Number of updates in a real-time second.
    /// </summary>
    public const Int64 UpdatesPerRealSecond = 60; // todo: in C++, pass this value to DxApp::Init

    // todo: add somewhere in planning to have a year 9999 check and end game there, have special event, add game over screen later and way to continue even later

    /// <summary>
    ///     Number of updates in an in-game second.
    ///     With a value of 1 and 60 updates per real-time second, a game day will last 24 real-time minutes.
    /// </summary>
    public const Int64 UpdatesPerSecond = 1;

    /// <summary>
    ///     Number of in-game seconds in an in-game minute.
    /// </summary>
    public const Int64 SecondsPerMinute = 60;

    /// <summary>
    ///     Number of updates in an in-game minute.
    /// </summary>
    public const Int64 UpdatesPerMinute = UpdatesPerSecond * SecondsPerMinute;

    /// <summary>
    ///     Number of in-game minutes in an in-game hour.
    /// </summary>
    public const Int64 MinutesPerHour = 60;

    /// <summary>
    ///     Number of updates in an in-game hour.
    /// </summary>
    public const Int64 UpdatesPerHour = UpdatesPerMinute * MinutesPerHour;

    /// <summary>
    ///     Number of in-game hours in an in-game day.
    /// </summary>
    public const Int64 HoursPerDay = 24;

    /// <summary>
    ///     Number of updates in an in-game day.
    /// </summary>
    public const Int64 UpdatesPerDay = UpdatesPerHour * HoursPerDay;

    /// <summary>
    ///     Number of in-game days in an in-game week.
    /// </summary>
    public const Int32 DaysPerWeek = 7;

    /// <summary>
    ///     Number of in-game weeks in an in-game month.
    /// </summary>
    public const Int32 WeeksPerMonth = 4;

    /// <summary>
    ///     Number of in-game days in an in-game month.
    /// </summary>
    public const Int32 DaysPerMonth = DaysPerWeek * WeeksPerMonth;

    /// <summary>
    ///     Number of in-game months in an in-game year.
    /// </summary>
    public const Int32 MonthsPerYear = 4;

    /// <summary>
    ///     Number of in-game days in an in-game year.
    /// </summary>
    public const Int32 DaysPerYear = DaysPerMonth * MonthsPerYear;
}
