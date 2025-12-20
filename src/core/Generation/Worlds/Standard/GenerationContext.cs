// <copyright file="GenerationContext.cs" company="VoxelGame">
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
using System.Diagnostics;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Logic.Voxels;

namespace VoxelGame.Core.Generation.Worlds.Standard;

/// <summary>
///     Implementation of <see cref="IGenerationContext" />.
/// </summary>
public sealed class GenerationContext(Generator generator, ChunkPosition hint) : BaseGenerationContext(generator)
{
    private ColumnSampleStore? columns = generator.GetColumns(hint);

    /// <inheritdoc />
    public override IEnumerable<Content> GenerateColumn(Int32 x, Int32 z, (Int32 start, Int32 end) heightRange)
    {
        ChunkPosition chunk = ChunkPosition.From((x, 0, z));
        columns ??= ColumnSampleStore.Sample(chunk.X, chunk.Z, generator);

        Debug.Assert(columns.Contains(chunk));

        return generator.GenerateColumn(x, z, heightRange, columns);
    }

    /// <inheritdoc />
    public override void GenerateStructures(Section section)
    {
        generator.GenerateStructures(section, columns);
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        if (!disposed)
        {
            if (disposing && columns != null)
                generator.AddColumns(columns);

            disposed = true;
        }

        base.Dispose(disposing);
    }

    #endregion DISPOSABLE
}
