// <copyright file="PositionParser.cs" company="VoxelGame">
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

namespace VoxelGame.Client.Console.Parsers;

/// <summary>
///     Parses a <see cref="Position" /> argument from either three coordinate expressions or a named position.
///     Coordinate expressions (<see cref="CoordinatesExpression" />) represent 3D positions using absolute coordinates
///     or coordinates relative to the caller's actor position using <c>~</c> (e.g. <c>10 20 30</c> or <c>~ ~5 ~-3</c>).
///     Named positions represent predefined or stored positions prefixed with <c>@</c> (e.g. <c>@spawn</c> or <c>@origin</c>).
/// </summary>
public sealed class PositionParser : Parser
{
    /// <inheritdoc />
    public override Type ParsedType => typeof(Position);

    /// <inheritdoc />
    public override String TypeRepresentation => "Position (x y z | @name)";

    /// <inheritdoc />
    public override Argument? Parse(IReadOnlyList<String> args, Int32 offset, out Int32 consumed)
    {
        consumed = 0;

        if (offset >= args.Count) return null;

        String first = args[offset];

        if (first.StartsWith('@'))
        {
            if (first.Length <= 1) return null;

            consumed = 1;

            return new NamedPositionArgument(first[1..]);
        }

        if (CoordinatesExpression.TryParse(args, offset, out CoordinatesExpression? coordinates))
        {
            consumed = 3;

            return new CoordinatePositionArgument(coordinates);
        }

        return null;
    }
}
