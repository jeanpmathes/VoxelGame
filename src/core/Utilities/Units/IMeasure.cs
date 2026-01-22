// <copyright file="IUnit.cs" company="VoxelGame">
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
using VoxelGame.Annotations.Definitions;

namespace VoxelGame.Core.Utilities.Units;

/// <summary>
///     Base interface for all measures.
/// </summary>
public interface IMeasure
{
    /// <summary>
    ///     The unit of the measures.
    /// </summary>
    static abstract Unit Unit { get; }

    /// <summary>
    ///     All allowed prefixes for the measure.
    /// </summary>
    static abstract AllowedPrefixes Prefixes { get; }

    /// <summary>
    ///     The value of the measures.
    /// </summary>
    Double Value { get; }

    /// <summary>
    ///     Get the string representation of the measure.
    /// </summary>
    /// <param name="measure">The measure to convert to string.</param>
    /// <param name="format">The format provider to use.</param>
    /// <typeparam name="T">The type of the measure.</typeparam>
    /// <returns>The string representation of the measure.</returns>
    static String ToString<T>(T measure, IFormatProvider? format) where T : IMeasure
    {
        Prefix prefix = Prefix.FindBest(measure.Value, T.Prefixes);

        return String.Create(format, $"{measure.Value / prefix.Factor:F2} {prefix.Symbol}{T.Unit.Symbol}");
    }
}
