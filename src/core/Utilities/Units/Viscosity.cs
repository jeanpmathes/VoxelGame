// <copyright file="Viscosity.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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

namespace VoxelGame.Core.Utilities.Units;

public readonly partial struct Viscosity
{
    private const Double UpdateTicksPerMilliPascalSecond = 15.0;

    /// <summary>
    ///     Gets the viscosity expressed as update ticks.
    /// </summary>
    public Double UpdateTicks
    {
        get => MilliPascalSeconds * UpdateTicksPerMilliPascalSecond;
        init => MilliPascalSeconds = value / UpdateTicksPerMilliPascalSecond;
    }

    /// <summary>
    ///     Gets the viscosity in milli Pascal seconds.
    /// </summary>
    public Double MilliPascalSeconds
    {
        get => PascalSeconds * 1000.0;
        init => PascalSeconds = value / 1000.0;
    }

    /// <summary>
    ///     Converts this viscosity to the update delay used for scheduling.
    /// </summary>
    public UInt32 ToUpdateDelay()
    {
        return (UInt32) Math.Max(val1: 1, Math.Round(UpdateTicks));
    }
}
