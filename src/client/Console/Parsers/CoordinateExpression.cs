// <copyright file="CoordinateExpression.cs" company="VoxelGame">
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace VoxelGame.Client.Console.Parsers;

/// <summary>
///     Represents a single coordinate component, which can be an absolute value or relative to the caller's actor coordinate.
/// </summary>
public abstract record CoordinateExpression
{
    /// <summary>
    ///     Resolve this coordinate expression given the caller's actor coordinate along this axis.
    /// </summary>
    /// <param name="callerCoordinate">The caller's actor coordinate along this axis.</param>
    /// <returns>The resolved coordinate value.</returns>
    public abstract Double Resolve(Double callerCoordinate);

    /// <summary>
    ///     Try to parse a coordinate expression from a single word.
    /// </summary>
    /// <param name="word">The word to parse.</param>
    /// <param name="expression">The parsed coordinate expression if successful, otherwise <see langword="null" />.</param>
    /// <returns><see langword="true" /> if parsing was successful; otherwise <see langword="false" />.</returns>
    public static Boolean TryParse(String word, [NotNullWhen(true)] out CoordinateExpression? expression)
    {
        expression = null;

        if (String.IsNullOrEmpty(word)) return false;

        if (word.StartsWith('~'))
        {
            if (word.Length == 1)
            {
                expression = new Relative(0.0);

                return true;
            }

            String remainder = word[1..];

            if (Double.TryParse(remainder, NumberStyles.Float, CultureInfo.InvariantCulture, out Double offset))
            {
                expression = new Relative(offset);

                return true;
            }

            return false;
        }

        if (Double.TryParse(word, NumberStyles.Float, CultureInfo.InvariantCulture, out Double value))
        {
            expression = new Absolute(value);

            return true;
        }

        return false;
    }

    /// <summary>
    ///     An absolute coordinate value.
    /// </summary>
    /// <param name="Value">The absolute coordinate value.</param>
    public sealed record Absolute(Double Value) : CoordinateExpression
    {
        /// <inheritdoc />
        public override Double Resolve(Double callerCoordinate)
        {
            return Value;
        }
    }

    /// <summary>
    ///     A coordinate value relative to the caller's actor coordinate.
    /// </summary>
    /// <param name="Offset">The offset to add to the caller's actor coordinate.</param>
    public sealed record Relative(Double Offset) : CoordinateExpression
    {
        /// <inheritdoc />
        public override Double Resolve(Double callerCoordinate)
        {
            return callerCoordinate + Offset;
        }
    }
}
