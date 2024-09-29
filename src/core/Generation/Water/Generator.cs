// <copyright file="Generator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Elements;

namespace VoxelGame.Core.Generation.Water;

/// <summary>
///     Generates a world made out of water.
/// </summary>
public sealed class Generator : IWorldGenerator
{
    private readonly Content core = new(Blocks.Instance.Core);
    private readonly Content empty = Content.Default;
    private readonly Content water = new(fluid: Fluids.Instance.SeaWater);

    private readonly Int32 waterLevel;

    /// <summary>
    ///     Create a new water generator.
    /// </summary>
    /// <param name="waterLevel">The water level (inclusive) below which the world is filled with water.</param>
    public Generator(Int32 waterLevel = 0)
    {
        this.waterLevel = waterLevel;
    }

    /// <inheritdoc />
    public IMap Map { get; } = new Map();

    /// <inheritdoc />
    public IGenerationContext CreateGenerationContext(ChunkPosition hint)
    {
        return new GenerationContext(this);
    }

    /// <inheritdoc />
    public IDecorationContext CreateDecorationContext()
    {
        return new DecorationContext(this);
    }

    /// <inheritdoc />
    public void EmitViews(DirectoryInfo path)
    {
        // No views to emit.
    }

    /// <inheritdoc />
    public IEnumerable<Vector3i>? SearchNamedGeneratedElements(Vector3i start, String name, UInt32 maxDistance)
    {
        #pragma warning disable S1168 // A null-return indicates that the name is not valid, which is different from not finding anything.
        return null;
        #pragma warning restore S1168
    }

    /// <inheritdoc cref="IGenerationContext.GenerateColumn" />
    public IEnumerable<Content> GenerateColumn(Int32 x, Int32 z, (Int32 start, Int32 end) heightRange)
    {
        for (Int32 y = heightRange.start; y < heightRange.end; y++) yield return GenerateContent((x, y, z));
    }

    private Content GenerateContent(Vector3i position)
    {
        if (position.Y == -World.BlockLimit) return core;

        return position.Y <= waterLevel ? water : empty;
    }

    #region IDisposable Support

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            // Nothing to dispose.
        }
        else
        {
            throw new InvalidOperationException("Tried to dispose a noise generator from the finalizer.");
        }

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~Generator()
    {
        Dispose(disposing: false);
    }

    #endregion
}
