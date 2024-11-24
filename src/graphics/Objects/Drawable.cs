// <copyright file="IDrawable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices.Marshalling;
using VoxelGame.Graphics.Core;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Graphics.Objects;

/// <summary>
///     The common abstract base class of objects drawn in 3D space.
/// </summary>
[NativeMarshalling(typeof(DrawableMarshaller))]
public class Drawable : Spatial, IDisposable
{
    private Boolean enabled;

    /// <summary>
    ///     Create a new drawable object.
    /// </summary>
    /// <param name="nativePointer">The native pointer.</param>
    /// <param name="space">The space.</param>
    protected Drawable(IntPtr nativePointer, Space space) : base(nativePointer, space) {}

    /// <summary>
    ///     Set or get the enabled state of this object. If disabled, the object will not be rendered.
    /// </summary>
    public Boolean IsEnabled
    {
        get => enabled;
        set
        {
            if (value == enabled) return;

            Throw.IfDisposed(disposed);

            NativeMethods.SetDrawableEnabledState(this, value);
            enabled = value;
        }
    }

    #region IDisposable Support

    private Boolean disposed;

    /// <summary>
    ///     Override to implement custom dispose logic.
    /// </summary>
    protected virtual void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            Deregister();
            NativeMethods.ReturnDrawable(this);
        }
        else
        {
            Throw.ForMissedDispose(this);
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

#pragma warning disable S3242
[CustomMarshaller(typeof(Drawable), MarshalMode.ManagedToUnmanagedIn, typeof(DrawableMarshaller))]
internal static class DrawableMarshaller
{
    internal static IntPtr ConvertToUnmanaged(Drawable managed)
    {
        return managed.Self;
    }

    internal static void Free(IntPtr unmanaged)
    {
        // Nothing to do here.
    }
}
#pragma warning restore S3242
