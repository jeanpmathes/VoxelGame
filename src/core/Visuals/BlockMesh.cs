// <copyright file="BlockMesh.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     A mesh for a complex block, capable of defining more complex shapes than just a cube.
/// </summary>
public class BlockMesh
{
    private readonly uint[] indices;
    private readonly int[] textureIndices;
    private readonly uint vertexCount;
    private readonly float[] vertices;

    /// <summary>
    ///     Create a new block mesh.
    /// </summary>
    /// <param name="vertexCount">The vertex count.</param>
    /// <param name="vertices">The vertices, eight floats per vertex. Every octet contains position, normal and UV coordinates.</param>
    /// <param name="textureIndices">The texture indices.</param>
    /// <param name="indices">The indices defining the triangles.</param>
    public BlockMesh(uint vertexCount, float[] vertices, int[] textureIndices, uint[] indices)
    {
        this.vertexCount = vertexCount;
        this.vertices = vertices;
        this.textureIndices = textureIndices;
        this.indices = indices;
    }

    /// <summary>
    ///     Get the mesh as mesh data for complex meshing.
    /// </summary>
    /// <param name="tint">An optional tint.</param>
    /// <param name="isAnimated">Whether the model is animated.</param>
    /// <returns>The mesh data.</returns>
    public IComplex.MeshData GetMeshData(TintColor? tint = null, bool isAnimated = false)
    {
        return new IComplex.MeshData(vertexCount, vertices, textureIndices, indices)
        {
            Tint = tint ?? TintColor.None,
            IsAnimated = isAnimated
        };
    }
}
