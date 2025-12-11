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
    (Int32 upper, Int32 lower) Seed { get; }

    /// <summary>
    ///     An optional timer for profiling.
    /// </summary>
    Timer? Timer { get; }

    /// <inheritdoc cref="WorldData.ReadBlobAsync{T}" />
    T? ReadBlob<T>(String name) where T : class, IEntity, new();

    /// <inheritdoc cref="WorldData.WriteBlobAsync{T}" />
    void WriteBlob<T>(String name, T entity) where T : class, IEntity, new();
}
