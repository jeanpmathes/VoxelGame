// <copyright file="WorldGeneratorContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Serialization;

namespace VoxelGame.Core.Generation;

/// <summary>
///     Implementation of <see cref="IWorldGeneratorContext" />.
/// </summary>
public class WorldGeneratorContext(World world, Timer? timer) : IWorldGeneratorContext
{
    /// <inheritdoc />
    public (Int32 upper, Int32 lower) Seed => world.Seed;

    /// <inheritdoc />
    public Timer? Timer { get; } = timer;

    /// <inheritdoc />
    public T? ReadBlob<T>(String name) where T : class, IEntity, new()
    {
        return world.Data.ReadBlob<T>(name);
    }

    /// <inheritdoc />
    public void WriteBlob<T>(String name, T entity) where T : class, IEntity, new()
    {
        world.Data.WriteBlob(name, entity);
    }
}
