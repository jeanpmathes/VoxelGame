// <copyright file="IDrawable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Utilities;
using VoxelGame.Support.Core;

namespace VoxelGame.Support.Objects;

/// <summary>
///     The common abstract base class of objects drawn in 3D space.
/// </summary>
public class Drawable : Spatial, IDisposable
{
    private bool enabled;

    /// <summary>
    ///     Create a new drawable object.
    /// </summary>
    /// <param name="nativePointer">The native pointer.</param>
    /// <param name="space">The space.</param>
    protected Drawable(IntPtr nativePointer, Space space) : base(nativePointer, space) {}

    /// <summary>
    ///     Set or get the enabled state of this object. If disabled, the object will not be rendered.
    /// </summary>
    public bool IsEnabled
    {
        get => enabled;
        set
        {
            if (value == enabled) return;

            Throw.IfDisposed(disposed);

            Native.SetDrawableEnabledState(this, value);
            enabled = value;
        }
    }

    #region IDisposable Support

    private bool disposed;

    /// <summary>
    /// Override to implement custom dispose logic.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            Deregister();
            Native.ReturnDrawable(this);
        }
        else
        {
            Throw.ForMissedDispose(nameof(Drawable));
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
    ///     The finalizer.
    /// </summary>
    ~Drawable()
    {
        Dispose(disposing: false);
    }

    #endregion IDisposable Support
}
