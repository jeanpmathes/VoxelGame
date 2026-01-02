// <copyright file="Generators.cs" company="VoxelGame">
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

using System.Collections.Generic;
using VoxelGame.Core.Generation.Worlds;
using VoxelGame.Core.Generation.Worlds.Standard;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Generation;

/// <summary>
///     Resource catalog with all world generators.
/// </summary>
public class Generators : ResourceCatalog
{
    /// <summary>
    ///     Create a new instance of the generators catalog.
    /// </summary>
    public Generators() : base([
        .. GetGeneratorEntries<Generator>(),
        .. GetGeneratorEntries<Worlds.Water.Generator>()
    ]) {}

    private static IEnumerable<ICatalogEntry> GetGeneratorEntries<T>() where T : IWorldGenerator
    {
        return
        [
            T.CreateResourceCatalog(),
            new GeneratorLinker<T>()
        ];
    }

    private sealed class GeneratorLinker<T> : IResourceLinker where T : IWorldGenerator
    {
        public void Link(IResourceContext context)
        {
            T.LinkResources(context);
        }
    }
}
