// <copyright file="CoordinatesExpression.cs" company="VoxelGame">
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
using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

namespace VoxelGame.Client.Console.Parsers;

/// <summary>
///     Combines three coordinate expressions for X, Y, and Z axes.
/// </summary>
/// <param name="X">The expression for the X axis.</param>
/// <param name="Y">The expression for the Y axis.</param>
/// <param name="Z">The expression for the Z axis.</param>
public sealed record CoordinatesExpression(CoordinateExpression X, CoordinateExpression Y, CoordinateExpression Z)
{
    /// <summary>
    ///     Resolve the full 3D coordinates using the caller's actor position.
    /// </summary>
    /// <param name="callerPosition">The caller's actor position.</param>
    /// <returns>The resolved 3D vector.</returns>
    public Vector3d Resolve(Vector3d callerPosition)
    {
        return new Vector3d(
            X.Resolve(callerPosition.X),
            Y.Resolve(callerPosition.Y),
            Z.Resolve(callerPosition.Z));
    }

    /// <summary>
    ///     Try to parse three coordinate expressions from an argument list starting at a given offset.
    /// </summary>
    /// <param name="args">The list of arguments.</param>
    /// <param name="offset">The starting offset in the argument list.</param>
    /// <param name="expression">The parsed coordinates expression if successful, otherwise <see langword="null" />.</param>
    /// <returns><see langword="true" /> if parsing was successful; otherwise <see langword="false" />.</returns>
    public static Boolean TryParse(IReadOnlyList<String> args, Int32 offset, [NotNullWhen(true)] out CoordinatesExpression? expression)
    {
        expression = null;

        if (offset + 3 > args.Count) return false;

        if (!CoordinateExpression.TryParse(args[offset], out CoordinateExpression? x)) return false;
        if (!CoordinateExpression.TryParse(args[offset + 1], out CoordinateExpression? y)) return false;
        if (!CoordinateExpression.TryParse(args[offset + 2], out CoordinateExpression? z)) return false;

        expression = new CoordinatesExpression(x, y, z);

        return true;
    }
}
