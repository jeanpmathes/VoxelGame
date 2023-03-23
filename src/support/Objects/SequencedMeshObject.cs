// <copyright file="SequencedMeshObject.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Support.Definition;

namespace VoxelGame.Support.Objects;

/// <summary>
///     A mesh object with a sequence of vertices.
/// </summary>
public class SequencedMeshObject : SpatialObject
{
    /// <summary>
    ///     Create a new native sequenced mesh object.
    /// </summary>
    public SequencedMeshObject(IntPtr nativePointer, Space space) : base(nativePointer, space) {}

    /// <summary>
    ///     Set the mesh of this object.
    /// </summary>
    /// <param name="vertices">The vertices of the mesh.</param>
    public void SetMesh(SpatialVertex[] vertices)
    {
        Native.SetSequencedMeshObjectData(this, vertices, vertices.Length);
    }
}

