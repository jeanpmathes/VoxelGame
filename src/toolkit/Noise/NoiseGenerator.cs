// <copyright file="NoiseGenerator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using VoxelGame.Toolkit.Collections;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Toolkit.Noise;

/// <summary>
/// Wraps around the noise generation library linked in the native code.
/// Use the <see cref="NoiseBuilder"/> to create a new noise generator.
/// </summary>
public sealed class NoiseGenerator : IDisposable
{
    private readonly IntPtr self;

    /// <summary>
    /// Create a new noise generator.
    /// </summary>
    /// <param name="definition">The definition of the noise generator.</param>
    public NoiseGenerator(NoiseDefinition definition)
    {
        self = Native.CreateNoise(definition);
    }

    /// <summary>
    /// Get the noise value at the given position.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Single GetNoise(Vector2d position)
    {
        EnsureNotDisposed();

        return Native.GetNoise2D(self, position);
    }

    /// <summary>
    /// Get the noise value at the given position.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Single GetNoise(Vector3d position)
    {
        EnsureNotDisposed();

        return Native.GetNoise3D(self, position);
    }

    /// <summary>
    /// Get the noise value for a grid of points, starting at the given position.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Array2D<Single> GetNoiseGrid(Vector2i position, Int32 size)
    {
        EnsureNotDisposed();

        Array2D<Single> result = new(size, transpose: true);
        Native.GetNoiseGrid2D(self, position, new Vector2i(size), result.AsSpan());

        return result;
    }

    /// <summary>
    /// Get the noise value for a grid of points, starting at the given position.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Array3D<Single> GetNoiseGrid(Vector3i position, Int32 size)
    {
        EnsureNotDisposed();

        Array3D<Single> result = new(size, transpose: true);
        Native.GetNoiseGrid3D(self, position, new Vector3i(size), result.AsSpan());

        return result;
    }

    [Conditional("DEBUG")]
    private void EnsureNotDisposed()
    {
        if (!disposed) return;

        throw new ObjectDisposedException(nameof(NoiseGenerator));
    }

    #region IDisposable Support

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing) Native.DeleteNoise(self);
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
    ~NoiseGenerator()
    {
        Dispose(disposing: false);
    }

    #endregion
}
