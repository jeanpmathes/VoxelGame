// <copyright file="Mesh.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Support.Core;
using VoxelGame.Support.Data;

namespace VoxelGame.Support.Objects;

/// <summary>
///     A mesh, positioned in 3D space and target of raytracing.
/// </summary>
public class Mesh : Drawable
{
    /// <summary>
    ///     Wrap a native mesh and drawable pointer.
    /// </summary>
    public Mesh(IntPtr nativePointer, Space space) : base(nativePointer, space) {}

    /// <summary>
    ///     Set the vertices that define this mesh.
    ///     Only valid if the material uses the default intersection shader.
    /// </summary>
    /// <param name="vertices">The vertices.</param>
    public void SetVertices(Span<SpatialVertex> vertices)
    {
        Native.SetMeshVertices(this, vertices);
    }

    /// <summary>
    ///     Set the bounds that define this mesh.
    ///     Only valid if the material uses a custom intersection shader.
    /// </summary>
    /// <param name="bounds">The bounds.</param>
    public void SetBounds(Span<SpatialBounds> bounds)
    {
        Native.SetMeshBounds(this, bounds);
    }
}
