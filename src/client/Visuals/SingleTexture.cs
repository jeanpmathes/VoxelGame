// <copyright file="SingleTexture.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Graphics.Objects;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Visuals;

/// <summary>
/// A texture resource. Wraps a <see cref="Texture"/>.
/// </summary>
public sealed class SingleTexture(RID identifier, Texture texture) : IResource
{
    /// <summary>
    /// Get the wrapped texture.
    /// </summary>
    public Texture Texture => texture;

    /// <inheritdoc />
    public RID Identifier { get; } = identifier;

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.Texture;

    #region DISPOSING

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing) texture.Free();
        else Throw.ForMissedDispose(this);

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

    #endregion DISPOSING
}
