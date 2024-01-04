// <copyright file="Effect.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Support.Core;
using VoxelGame.Support.Data;

namespace VoxelGame.Support.Objects;

/// <summary>
///     An effect is a object positioned in 3D space that is rendered with a raster pipeline.
/// </summary>
public class Effect : Drawable
{
    /// <summary>
    ///     Wrap a native mesh and drawable pointer.
    /// </summary>
    public Effect(IntPtr nativePointer, Space space) : base(nativePointer, space) {}

    /// <summary>
    ///     Set the new vertices for this effect.
    /// </summary>
    /// <param name="vertices">The new vertices.</param>
    public void SetNewVertices(Span<EffectVertex> vertices)
    {
        Native.SetEffectVertices(this, vertices);
    }
}
