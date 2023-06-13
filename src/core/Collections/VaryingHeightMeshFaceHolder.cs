// <copyright file="FluidMeshFaceHolder.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Buffers;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Collections;

/// <summary>
///     A specialized class used to compact varying height block faces and fluid faces while meshing.
/// </summary>
public class VaryingHeightMeshFaceHolder : MeshFaceHolder // todo: do all the same refactoring here as in BlockMeshFaceHolder
{
    private static readonly uint[] indices =
    {
        0, 2, 1,
        0, 3, 2,
        0, 1, 2,
        0, 2, 3
    };

    private readonly MeshFace?[][] lastFaces;

    private int count;

    /// <summary>
    ///     Create a new <see cref="VaryingHeightMeshFaceHolder" /> for a given block side.
    /// </summary>
    /// <param name="side">The side the faces held belong too.</param>
    public VaryingHeightMeshFaceHolder(BlockSide side) : base(side)
    {
        // Initialize layers.
        lastFaces = ArrayPool<MeshFace[]>.Shared.Rent(Section.Size);

        // Initialize rows.
        for (var i = 0; i < Section.Size; i++)
        {
            lastFaces[i] = ArrayPool<MeshFace>.Shared.Rent(Section.Size);

            for (var j = 0; j < Section.Size; j++) lastFaces[i][j] = null;
        }
    }

    /// <summary>
    ///     Add a face to the holder.
    /// </summary>
    /// <param name="pos">The position of the face, in section block coordinates.</param>
    /// <param name="vertexData">The encoded data to use for this face.</param>
    /// <param name="vertices">The encoded data for each vertex.</param>
    /// <param name="isSingleSided">True if this face is single sided, false if double sided.</param>
    /// <param name="isFull">True if this face is full, filling a complete block side.</param>
    public void AddFace(Vector3i pos, int vertexData, (int vertA, int vertB, int vertC, int vertD) vertices,
        bool isSingleSided, bool isFull)
    {
        ExtractIndices(pos, out int layer, out int row, out int position);

        // Build current face.
        MeshFace currentFace = MeshFace.Get(
            vertices.vertA,
            vertices.vertB,
            vertices.vertC,
            vertices.vertD,
            vertexData,
            position,
            isSingleSided);

        // Front and Back faces cannot be extended (along the y axis) when the fluid is not all full level.
        bool levelPermitsExtending = !(side is BlockSide.Front or BlockSide.Back && !isFull);

        // Check if an already existing face can be extended.
        if (levelPermitsExtending && (lastFaces[layer][row]?.IsExtendable(currentFace) ?? false))
        {
            currentFace.Return();
            currentFace = lastFaces[layer][row]!;

            ExtendFace(currentFace, vertices);
        }
        else
        {
            currentFace.previous = lastFaces[layer][row];
            lastFaces[layer][row] = currentFace;

            count++;
        }

        if (row == 0) return;

        MeshFace? combinationRowFace = lastFaces[layer][row - 1];
        MeshFace? lastCombinationRowFace = null;

        // Left and right faces cannot be combined (along the y axis) when the fluid is not all full level.
        if (side is BlockSide.Left or BlockSide.Right && !isFull) return;

        // Check if the current face can be combined with a face in the previous row.
        while (combinationRowFace != null)
        {
            if (combinationRowFace.IsCombinable(currentFace))
            {
                CombineFace(currentFace, combinationRowFace);
                RemoveFace(combinationRowFace, lastCombinationRowFace, layer, row - 1);

                count--;

                break;
            }

            lastCombinationRowFace = combinationRowFace;
            combinationRowFace = combinationRowFace.previous;
        }

        void RemoveFace(MeshFace toRemove, MeshFace? last, int l, int r)
        {
            if (last == null) lastFaces[l][r] = toRemove.previous;
            else last.previous = toRemove.previous;

            toRemove.Return();
        }
    }

    private void CombineFace(MeshFace face, MeshFace combinationFace)
    {
        switch (side)
        {
            case BlockSide.Front:
            case BlockSide.Bottom:
            case BlockSide.Top:
                face.vertexA = combinationFace.vertexA;
                face.vertexB = combinationFace.vertexB;

                break;

            case BlockSide.Back:
                face.vertexC = combinationFace.vertexC;
                face.vertexD = combinationFace.vertexD;

                break;

            case BlockSide.Left:
            case BlockSide.Right:
                face.vertexA = combinationFace.vertexA;
                face.vertexD = combinationFace.vertexD;

                break;

            default: throw new InvalidCastException();
        }

        face.height = combinationFace.height + 1;
    }

    private void ExtendFace(MeshFace face, (int vertA, int vertB, int vertC, int vertD) vertices)
    {
        switch (side)
        {
            case BlockSide.Front:
            case BlockSide.Back:
            case BlockSide.Bottom:
                face.vertexB = vertices.vertB;
                face.vertexC = vertices.vertC;

                break;

            case BlockSide.Left:
                face.vertexC = vertices.vertC;
                face.vertexD = vertices.vertD;

                break;

            case BlockSide.Right:
                face.vertexA = vertices.vertA;
                face.vertexB = vertices.vertB;

                break;

            case BlockSide.Top:
                face.vertexA = vertices.vertA;
                face.vertexD = vertices.vertD;

                break;

            default: throw new InvalidCastException();
        }

        face.length++;
    }

    /// <summary>
    ///     Generate the mesh with all held faces.
    /// </summary>
    /// <param name="vertexCount">The current vertex count, will be incremented for all added vertices.</param>
    /// <param name="meshData">The list that will be filled with mesh data.</param>
    /// <param name="meshIndices">The list that will be filled with indices.</param>
    public void GenerateMesh(ref uint vertexCount, PooledList<int> meshData, PooledList<uint> meshIndices)
    {
        if (count == 0) return;

        meshData.Capacity += count;

        for (var l = 0; l < Section.Size; l++)
        for (var r = 0; r < Section.Size; r++)
        {
            MeshFace? currentFace = lastFaces[l][r];

            while (currentFace != null)
            {
                int vertexTexRepetition = BuildVertexTextureRepetition(currentFace.height, currentFace.length);

                meshData.Add(vertexTexRepetition | currentFace.vertexA);
                meshData.Add(currentFace.vertexData);

                meshData.Add(vertexTexRepetition | currentFace.vertexB);
                meshData.Add(currentFace.vertexData);

                meshData.Add(vertexTexRepetition | currentFace.vertexC);
                meshData.Add(currentFace.vertexData);

                meshData.Add(vertexTexRepetition | currentFace.vertexD);
                meshData.Add(currentFace.vertexData);

                int newIndices = currentFace.isSingleSided ? 6 : 12;
                meshIndices.AddRange(indices, newIndices);

                for (var i = 0; i < newIndices; i++) meshIndices[meshIndices.Count - newIndices + i] += vertexCount;

                vertexCount += 4;

                MeshFace? next = currentFace.previous;
                currentFace.Return();
                currentFace = next;
            }
        }
    }

    private int BuildVertexTextureRepetition(int height, int length)
    {
        const int heightShift = 24;
        const int lengthShift = 20;

        return !(side is BlockSide.Left or BlockSide.Right)
            ? (height << heightShift) | (length << lengthShift)
            : (length << heightShift) | (height << lengthShift);
    }

    /// <summary>
    ///     Return all pooled resources.
    /// </summary>
    public void ReturnToPool()
    {
        for (var i = 0; i < Section.Size; i++) ArrayPool<MeshFace>.Shared.Return(lastFaces[i]!);

        ArrayPool<MeshFace[]>.Shared.Return(lastFaces!);
    }

#pragma warning disable CA1812

    private sealed class MeshFace
    {
        public int height;

        public bool isSingleSided;
        public int length;

        private int position;
        public MeshFace? previous;

        public int vertexA;
        public int vertexB;
        public int vertexC;
        public int vertexD;

        public int vertexData;

        public bool IsExtendable(MeshFace extension)
        {
            return position + length + 1 == extension.position &&
                   height == extension.height &&
                   vertexData == extension.vertexData &&
                   isSingleSided == extension.isSingleSided;
        }

        public bool IsCombinable(MeshFace addition)
        {
            return position == addition.position &&
                   length == addition.length &&
                   vertexData == addition.vertexData &&
                   isSingleSided == addition.isSingleSided;
        }

        #region POOLING

        public static MeshFace Get(int vert00, int vert01, int vert11, int vert10, int vertData, int position,
            bool isSingleSided)
        {
            MeshFace instance = ObjectPool<MeshFace>.Shared.Get();

            instance.previous = null;

            instance.vertexA = vert00;
            instance.vertexB = vert01;
            instance.vertexC = vert11;
            instance.vertexD = vert10;

            instance.vertexData = vertData;

            instance.position = position;
            instance.length = 0;
            instance.height = 0;

            instance.isSingleSided = isSingleSided;

            return instance;
        }

        public void Return()
        {
            ObjectPool<MeshFace>.Shared.Return(this);
        }

        #endregion POOLING
    }

#pragma warning restore CA1812
}
