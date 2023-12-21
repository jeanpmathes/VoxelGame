// <copyright file="Mesh.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Visuals;
using VoxelGame.Support.Core;

namespace VoxelGame.Support.Objects;

/// <summary>
///     A mesh, positioned in 3D space and target of raytracing.
/// </summary>
public class Mesh : Spatial
{
    private bool enabled = true;

    /// <summary>
    ///     Wrap a native mesh pointer.
    /// </summary>
    public Mesh(IntPtr nativePointer, Space space) : base(nativePointer, space) {}

    /// <summary>
    ///     Set or get the enabled state of this object. If disabled, the object will not be rendered.
    /// </summary>
    public bool IsEnabled
    {
        get => enabled;
        set
        {
            if (value == enabled) return;

            Native.SetMeshEnabledState(this, value);
            enabled = value;
        }
    }

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

    /// <summary>
    ///     Frees the native object.
    /// </summary>
    public void Return()
    {
        Deregister();
        Native.ReturnMesh(this);
    }
}
