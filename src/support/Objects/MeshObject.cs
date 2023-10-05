// <copyright file="MeshObject.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Visuals;

namespace VoxelGame.Support.Objects;

/// <summary>
///     A mesh object with an indexed sequence of vertices.
/// </summary>
public class MeshObject : SpatialObject
{
    private bool enabled = true;

    /// <summary>
    ///     Create a new native indexed mesh object.
    /// </summary>
    public MeshObject(IntPtr nativePointer, Space space) : base(nativePointer, space) {}

    /// <summary>
    ///     Set or get the enabled state of this object. If disabled, the object will not be rendered.
    /// </summary>
    public bool IsEnabled
    {
        get => enabled;
        set
        {
            if (value == enabled) return;

            Native.SetMeshObjectEnabledState(this, value);
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
        Native.SetMeshObjectVertices(this, vertices);
    }

    /// <summary>
    ///     Set the bounds that define this mesh.
    ///     Only valid if the material uses a custom intersection shader.
    /// </summary>
    /// <param name="bounds">The bounds.</param>
    public void SetBounds(Span<SpatialBounds> bounds)
    {
        Native.SetMeshObjectBounds(this, bounds);
    }

    /// <summary>
    ///     Frees the native object.
    /// </summary>
    public void Return()
    {
        Deregister();
        Native.ReturnMeshObject(this);
    }
}
