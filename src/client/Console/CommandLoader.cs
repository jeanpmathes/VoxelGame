// <copyright file="CommandLoader.cs" company="VoxelGame">
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
using System.Collections.Generic;
using System.Globalization;
using VoxelGame.Client.Console.Commands;
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

        invoker.AddParser(Parser.BuildParser(_ => true, s => s));

        invoker.AddParser(
            Parser.BuildParser(
                s => Int32.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _),
                s => Int32.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture)));

        invoker.AddParser(
            Parser.BuildParser(
                s => UInt32.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _),
                s => UInt32.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture)));

        invoker.AddParser(
            Parser.BuildParser(
                s => Double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _),
                s => Double.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture)));

        invoker.AddParser(Parser.BuildParser(
            s => Enum.IsDefined(typeof(Orientation), s),
            Enum.Parse<Orientation>));

        invoker.AddParser(Parser.BuildParser(s => Boolean.TryParse(s, out _), Boolean.Parse));

        invoker.SearchCommands(context);
        invoker.AddCommand(new Help(invoker));

        return [invoker];
    }
}
