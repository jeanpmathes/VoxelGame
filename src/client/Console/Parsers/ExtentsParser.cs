// <copyright file="ExtentsParser.cs" company="VoxelGame">
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
using System.Globalization;
using OpenTK.Mathematics;

namespace VoxelGame.Client.Console.Parsers;

/// <summary>
///     Parses an <see cref="Extents" /> argument from three numeric arguments.
/// </summary>
public sealed class ExtentsParser : Parser
{
    /// <inheritdoc />
    public override Type ParsedType => typeof(Extents);

    /// <inheritdoc />
    public override String TypeRepresentation => "Extents (x y z)";

    /// <inheritdoc />
    public override Argument? Parse(IReadOnlyList<String> args, Int32 offset, out Int32 consumed)
    {
        consumed = 0;

        if (offset + 3 > args.Count) return null;

        if (!Double.TryParse(args[offset], NumberStyles.Float, CultureInfo.InvariantCulture, out Double x)) return null;
        if (!Double.TryParse(args[offset + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out Double y)) return null;
        if (!Double.TryParse(args[offset + 2], NumberStyles.Float, CultureInfo.InvariantCulture, out Double z)) return null;

        consumed = 3;

        return new SimpleArgument(new Extents(new Vector3d(x, y, z)));
    }
}
