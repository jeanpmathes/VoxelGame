// <copyright file="MockWorldGenerator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Mathematics;
using VoxelGame.Core.Generation.Worlds;
using VoxelGame.Core.Logic.Chunks;
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

    public void EmitViews(DirectoryInfo path)
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
