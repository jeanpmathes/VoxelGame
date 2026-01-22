// <copyright file="MockWorldGenerator.cs" company="VoxelGame">
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
using System.IO;
using OpenTK.Mathematics;
using VoxelGame.Core.Generation.Worlds;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Tests.Generation;

public sealed class MockWorldGenerator : IWorldGenerator
{
    public IMap Map => throw new NotSupportedException();

    public static ICatalogEntry CreateResourceCatalog()
    {
        throw new NotSupportedException();
    }

    public static void LinkResources(IResourceContext context)
    {
        throw new NotSupportedException();
    }

    public static IWorldGenerator Create(IWorldGeneratorContext context)
    {
        throw new NotSupportedException();
    }

    public IGenerationContext CreateGenerationContext(ChunkPosition hint)
    {
        throw new NotSupportedException();
    }

    public IDecorationContext CreateDecorationContext(ChunkPosition hint, Int32 extents = 0)
    {
        throw new NotSupportedException();
    }

    public Operation EmitWorldInfo(DirectoryInfo path)
    {
        throw new NotSupportedException();
    }

    public IEnumerable<Vector3i> SearchNamedGeneratedElements(Vector3i start, String name, UInt32 maxDistance)
    {
        throw new NotSupportedException();
    }

    public void Dispose()
    {
        // Nothing to dispose.
    }
}
