// <copyright file="NamedPositionArgument.cs" company="VoxelGame">
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
using OpenTK.Mathematics;

namespace VoxelGame.Client.Console.Parsers;

/// <summary>
///     A position argument resolved from a named entry in the <see cref="NamedPositionRegistry" />.
/// </summary>
/// <param name="name">The name of the position without the '@' prefix.</param>
public sealed class NamedPositionArgument(String name) : Argument
{
    /// <inheritdoc />
    public override Object? Resolve(Context context)
    {
        Vector3d? position = context.Invoker.Positions.Resolve(name, context);

        if (position is {} value) return new Position(value);

        context.Output.WriteError($"Unknown named position: '@{name}'");

        return null;
    }
}
