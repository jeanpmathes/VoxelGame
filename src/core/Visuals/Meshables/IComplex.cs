// <copyright file="IComplex.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;

namespace VoxelGame.Core.Visuals.Meshables;

/// <summary>
///     A meshable for complex meshes that have few constraints.
/// </summary>
public interface IComplex : IBlockMeshable
{
    void IBlockMeshable.CreateMesh(Vector3i position, BlockMeshInfo info, MeshingContext context)
    {
        (int x, int y, int z) = position;

        MeshData mesh = GetMeshData(info);
        float[] vertices = mesh.GetVertices();
        int[] textureIndices = mesh.GetTextureIndices();
        uint[] indices = mesh.GetIndices();

        (PooledList<float> complexVertexPositions, PooledList<int> complexVertexData, PooledList<uint> complexIndices) =
            context.GetComplexMeshLists();

        complexIndices.AddRange(indices);

        for (var i = 0; i < mesh.VertexCount; i++)
        {
            complexVertexPositions.Add(vertices[i * 8 + 0] + x);
            complexVertexPositions.Add(vertices[i * 8 + 1] + y);
            complexVertexPositions.Add(vertices[i * 8 + 2] + z);

            // int: nnnn nooo oopp ppp- ---- --uu uuuv vvvv (nop: normal; uv: texture coords)
            int upperData =
                ((vertices[i * 8 + 5] < 0f
                    ? 0b1_0000 | (int) (vertices[i * 8 + 5] * -15f)
                    : (int) (vertices[i * 8 + 5] * 15f)) << 27) |
                ((vertices[i * 8 + 6] < 0f
                    ? 0b1_0000 | (int) (vertices[i * 8 + 6] * -15f)
                    : (int) (vertices[i * 8 + 6] * 15f)) << 22) |
                ((vertices[i * 8 + 7] < 0f
                    ? 0b1_0000 | (int) (vertices[i * 8 + 7] * -15f)
                    : (int) (vertices[i * 8 + 7] * 15f)) << 17) |
                ((int) (vertices[i * 8 + 3] * 16f) << 5) |
                (int) (vertices[i * 8 + 4] * 16f);

            complexVertexData.Add(upperData);

            // int: tttt tttt t--- ---a ---i iiii iiii iiii(t: tint; a: animated; i: texture index)
            int lowerData = (mesh.Tint.GetBits(context.BlockTint) << 23) | mesh.GetAnimationBit(i, shift: 16) |
                            textureIndices[i];

            complexVertexData.Add(lowerData);
        }

        for (int i = complexIndices.Count - indices.Length; i < complexIndices.Count; i++)
            complexIndices[i] += context.ComplexVertexCount;

        context.ComplexVertexCount += mesh.VertexCount;
    }

    /// <summary>
    ///     Provides the mesh data for meshing.
    /// </summary>
    protected MeshData GetMeshData(BlockMeshInfo info);

    /// <summary>
    ///     Create the mesh data for a complex mesh.
    /// </summary>
    protected static MeshData CreateData(uint vertexCount, float[] vertices, int[] textureIndices, uint[] indices)
    {
        return new MeshData(vertexCount, vertices, textureIndices, indices);
    }

    /// <summary>
    ///     The data that blocks have to provide for complex meshing.
    /// </summary>
    public readonly struct MeshData : IEquatable<MeshData>
    {
        private readonly float[] vertices;
        private readonly int[] textureIndices;
        private readonly uint[] indices;

        /// <summary>
        ///     Create the mesh data.
        /// </summary>
        public MeshData(uint vertexCount, float[] vertices, int[] textureIndices, uint[] indices)
        {
            this.vertices = vertices;
            this.textureIndices = textureIndices;
            this.indices = indices;

            VertexCount = vertexCount;

            Tint = TintColor.None;
            IsAnimated = false;
        }

        /// <summary>
        ///     Get the vertex count of the mesh.
        /// </summary>
        public uint VertexCount { get; }

        /// <summary>
        ///     The block tint.
        /// </summary>
        public TintColor Tint { get; init; }

        /// <summary>
        ///     Whether the block is animated.
        /// </summary>
        public bool IsAnimated { get; init; }

        /// <summary>
        ///     Get the animation bit.
        /// </summary>
        public int GetAnimationBit(int texture, int shift)
        {
            return IsAnimated && textureIndices[texture] != 0 ? 1 << shift : 0;
        }

        /// <summary>
        ///     Get the vertex array.
        /// </summary>
        public float[] GetVertices()
        {
            return vertices;
        }

        /// <summary>
        ///     Get the texture index array.
        /// </summary>
        public int[] GetTextureIndices()
        {
            return textureIndices;
        }

        /// <summary>
        ///     Get the index array.
        /// </summary>
        public uint[] GetIndices()
        {
            return indices;
        }

        /// <inheritdoc />
        public bool Equals(MeshData other)
        {
            return (VertexCount, Tint, IsAnimated, vertices, textureIndices, indices) == (other.VertexCount, other.Tint,
                other.IsAnimated, other.vertices, other.textureIndices, other.indices);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is MeshData other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(vertices, textureIndices, indices, VertexCount, Tint, IsAnimated);
        }

        /// <summary>
        ///     The equality operator.
        /// </summary>
        public static bool operator ==(MeshData left, MeshData right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     The inequality operator.
        /// </summary>
        public static bool operator !=(MeshData left, MeshData right)
        {
            return !left.Equals(right);
        }
    }
}
