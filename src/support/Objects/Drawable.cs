// <copyright file="IDrawable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Support.Core;

namespace VoxelGame.Support.Objects;

/// <summary>
///     The common abstract base class of objects drawn in 3D space.
/// </summary>
public abstract class Drawable : Spatial
{
    private bool enabled = true;

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

            Native.SetDrawableEnabledState(this, value);
            enabled = value;
        }
    }

    /// <summary>
    ///     Frees the native object.
    /// </summary>
    public void Return() // todo: try using IDisposable
    {
        Deregister();
        Native.ReturnDrawable(this);
    }
}
