// <copyright file="BaseGenerationContext.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds;

/// <summary>
///     Base class for generation contexts.
/// </summary>
/// <param name="generator">The world generator.</param>
public abstract class BaseGenerationContext(IWorldGenerator generator) : IGenerationContext
{
    /// <inheritdoc />
    public IWorldGenerator Generator => generator;

    /// <inheritdoc />
    public abstract IEnumerable<Content> GenerateColumn(Int32 x, Int32 z, (Int32 start, Int32 end) heightRange);

    /// <inheritdoc />
    public virtual void GenerateStructures(Section section)
    {
        // No structures to generate.
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <summary>
    ///     Overridable dispose method.
    /// </summary>
    /// <param name="disposing">Whether managed resources should be disposed.</param>
    protected virtual void Dispose(Boolean disposing)
    {
        if (disposed)
            return;

        if (!disposing)
            ExceptionTools.ThrowForMissedDispose(this);

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
    ~BaseGenerationContext()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
