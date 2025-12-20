// <copyright file="Convention.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Voxels.Conventions;

/// <summary>
///     Abstract base class for conventions.
/// </summary>
/// <param name="contentID">The content ID of the convention.</param>
/// <param name="builder">The block builder used to build the blocks in the convention.</param>
/// <typeparam name="TConvention">The concrete type of the convention.</typeparam>
public abstract class Convention<TConvention>(CID contentID, BlockBuilder builder) : IConvention where TConvention : IConvention
{
    /// <inheritdoc />
    public CID ID { get; } = contentID;

    /// <inheritdoc />
    public RID Identifier { get; } = contentID.GetResourceID<TConvention>();

    /// <inheritdoc />
    public IEnumerable<IContent> Content => builder.Registry.RetrieveContent();

    #region DISPOSABLE

    /// <summary>
    ///     Override this method to dispose of object used by the convention.
    ///     Note that most of the time, you will not need to override this method.
    /// </summary>
    /// <param name="disposing">Whether the method is called from the Dispose method or from the finalizer.</param>
    protected virtual void Dispose(Boolean disposing)
    {
        // Nothing to dispose by default.
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
    ~Convention()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
