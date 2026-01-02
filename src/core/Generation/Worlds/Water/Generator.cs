// <copyright file="Generator.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Water;

/// <summary>
///     Generates a world made out of water.
/// </summary>
public sealed class Generator : IWorldGenerator
{
    private readonly Content core = Content.CreateGenerated(Blocks.Instance.Core.CoreBlock);
    private readonly Content empty = Content.GenerationDefault;
    private readonly Content water = Content.CreateGenerated(fluid: Fluids.Instance.SeaWater);

    private readonly Int32 waterLevel;

    /// <summary>
    ///     Create a new water generator.
    /// </summary>
    /// <param name="waterLevel">The water level (inclusive) below which the world is filled with water.</param>
    private Generator(Int32 waterLevel = 0)
    {
        this.waterLevel = waterLevel;
    }

    /// <inheritdoc />
    public static ICatalogEntry CreateResourceCatalog()
    {
        return new Catalog();
    }

    /// <inheritdoc />
    public static void LinkResources(IResourceContext context)
    {
        // No resources to link.
    }

    /// <inheritdoc />
    public static IWorldGenerator Create(IWorldGeneratorContext context)
    {
        return new Generator();
    }

    /// <inheritdoc />
    public IMap Map { get; } = new Map();

    /// <inheritdoc />
    public IGenerationContext CreateGenerationContext(ChunkPosition hint)
    {
        return new GenerationContext(this);
    }

    /// <inheritdoc />
    public IDecorationContext CreateDecorationContext(ChunkPosition hint, Int32 extents = 0)
    {
        return new DecorationContext(this);
    }

    /// <inheritdoc />
    public Operation EmitWorldInfo(DirectoryInfo path)
    {
        return Operations.CreateDone();
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

    #region DISPOSABLE

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
            ExceptionTools.ThrowForMissedDispose(this);
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

    #endregion DISPOSABLE
}
