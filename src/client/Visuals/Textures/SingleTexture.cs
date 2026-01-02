// <copyright file="SingleTexture.cs" company="VoxelGame">
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
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Graphics.Objects;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Visuals.Textures;

/// <summary>
///     A texture resource. Wraps a <see cref="Texture" />.
/// </summary>
public sealed class SingleTexture(RID identifier, Texture texture) : IResource
{
    /// <summary>
    ///     Get the wrapped texture.
    /// </summary>
    public Texture Texture => texture;

    /// <inheritdoc />
    public RID Identifier { get; } = identifier;

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.Texture;

    #region DISPOSABLE

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing) texture.Free();
        else ExceptionTools.ThrowForMissedDispose(this);

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
    ~SingleTexture()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
