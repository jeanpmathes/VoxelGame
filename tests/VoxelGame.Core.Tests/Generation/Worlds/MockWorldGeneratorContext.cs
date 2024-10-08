// <copyright file="MockWorldGeneratorContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Generation.Worlds;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Serialization;

namespace VoxelGame.Core.Tests.Generation.Worlds;

public class MockWorldGeneratorContext : IWorldGeneratorContext
{
    private readonly Dictionary<String, Object> blobs = new();

    public (Int32 upper, Int32 lower) Seed => (0, 0);
    public Timer? Timer => null;

    public T? ReadBlob<T>(String name) where T : class, IEntity, new()
    {
        return blobs.GetValueOrDefault(name) as T;
    }

    public void WriteBlob<T>(String name, T entity) where T : class, IEntity, new()
    {
        blobs[name] = entity;
    }
}
