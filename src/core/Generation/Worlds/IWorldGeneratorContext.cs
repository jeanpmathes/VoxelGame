// <copyright file="IWorldGeneratorContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Serialization;

namespace VoxelGame.Core.Generation.Worlds;

/// <summary>
///     Interface for a context in which a <see cref="IWorldGenerator" /> can be created.
/// </summary>
public interface IWorldGeneratorContext
{
    /// <summary>
    ///     The seed to use for generation.
    /// </summary>
    public (Int32 upper, Int32 lower) Seed { get; }

    /// <summary>
    ///     An optional timer for profiling.
    /// </summary>
    public Timer? Timer { get; }

    /// <inheritdoc cref="WorldData.ReadBlob{T}" />
    public T? ReadBlob<T>(String name) where T : class, IEntity, new();

    /// <inheritdoc cref="WorldData.WriteBlob{T}" />
    public void WriteBlob<T>(String name, T entity) where T : class, IEntity, new();
}
