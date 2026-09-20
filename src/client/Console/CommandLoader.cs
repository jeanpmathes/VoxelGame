// <copyright file="CommandLoader.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Client.Actors.Components;
using VoxelGame.Client.Console.Commands;
using VoxelGame.Client.Console.Parsers;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Client.Console;

/// <summary>
///     Loads all commands, creating a <see cref="CommandInvoker" />.
/// </summary>
public sealed class CommandLoader : IResourceLoader
{
    String? ICatalogEntry.Instance => null;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        CommandInvoker invoker = new();

        invoker.AddParser(new StringParser());
        invoker.AddParser(new Int32Parser());
        invoker.AddParser(new UInt32Parser());
        invoker.AddParser(new DoubleParser());
        invoker.AddParser(new BooleanParser());
        invoker.AddParser(new EnumParser<Orientation>());
        invoker.AddParser(new PositionParser());
        invoker.AddParser(new ExtentsParser());

        invoker.Positions.Register("origin", _ => Vector3d.Zero);
        invoker.Positions.Register("spawn", ctx => ctx.Player.World.SpawnPosition);
        invoker.Positions.Register("min-corner", ctx => -ctx.Player.World.Extents);
        invoker.Positions.Register("max-corner", ctx => ctx.Player.World.Extents);
        invoker.Positions.Register("self", ctx => ctx.Player.Body.Transform.Position);
        invoker.Positions.Register("prev-self", ctx => ctx.Player.GetComponent<PreviousPosition>()?.Value ?? ctx.Player.World.SpawnPosition);

        invoker.SearchCommands(context);
        invoker.AddCommand(new Help(invoker));

        return [invoker];
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose of.
    }
}
