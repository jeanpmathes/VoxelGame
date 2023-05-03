// <copyright file="MeshObject.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Support.Definition;

namespace VoxelGame.Support.Objects;

/// <summary>
///     A mesh object with an indexed sequence of vertices.
/// </summary>
public class MeshObject : SpatialObject
{
    /// <summary>
    ///     Create a new native indexed mesh object.
    /// </summary>
    public MeshObject(IntPtr nativePointer, Space space) : base(nativePointer, space) {}

    /// <summary>
    ///     Set the mesh of this object.
    /// </summary>
    /// <param name="vertices">The vertices.</param>
    /// <param name="indices">The indices.</param>
    public void SetMesh(SpatialVertex[] vertices, uint[] indices)
    {
        Native.SetMeshObjectData(this, vertices, indices);
    }
}
