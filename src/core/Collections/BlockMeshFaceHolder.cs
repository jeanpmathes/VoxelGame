// <copyright file="CompactedMeshFaceHolder.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Buffers;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Collections;

/// <summary>
///     A specialized class used to compact block faces when meshing.
/// </summary>
public class BlockMeshFaceHolder : MeshFaceHolder
{
    private readonly MeshFace?[][] lastFaces;

    private int count;

    /// <summary>
    ///     Create a new instance of the <see cref="BlockMeshFaceHolder" /> class for a given block side.
    /// </summary>
    /// <param name="side">The block side that the faces will correspond too.</param>
    public BlockMeshFaceHolder(BlockSide side) : base(side)
    {
        Debug.Assert(side != BlockSide.All);

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
    ///     Add a new face to the holder.
    /// </summary>
    /// <param name="pos">The position of the face, relative to the section origin.</param>
    /// <param name="data">The binary encoded data of the quad.</param>
    /// <param name="isRotated">True if the face is rotated.</param>
    public void AddFace(Vector3i pos, (uint a, uint b, uint c, uint d) data, bool isRotated)
    {
        ExtractIndices(pos, out int layer, out int row, out int position);

        // Build current face.
        MeshFace currentFace = MeshFace.Get(
            data,
            isRotated,
            position);

        // Check if an already existing face can be extended.
        if (lastFaces[layer][row]?.IsExtendable(currentFace) ?? false)
        {
            currentFace.Return();
            currentFace = lastFaces[layer][row]!;

            ExtendFace(currentFace);
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

        // Check if the current face can be combined with a face in the previous row.
        while (combinationRowFace != null)
        {
            if (combinationRowFace.IsCombinable(currentFace))
            {
                CombineFace(currentFace, combinationRowFace);

                if (lastCombinationRowFace == null)
                {
                    lastFaces[layer][row - 1] = combinationRowFace.previous;
                    combinationRowFace.Return();
                }
                else
                {
                    lastCombinationRowFace.previous = combinationRowFace.previous;
                    combinationRowFace.Return();
                }

                count--;

                break;
            }

            lastCombinationRowFace = combinationRowFace;
            combinationRowFace = combinationRowFace.previous;
        }
    }

    private static void ExtendFace(MeshFace face)
    {
        face.length++;
    }

    private static void CombineFace(MeshFace newFace, MeshFace combinationFace)
    {
        newFace.height = combinationFace.height + 1;
    }

    /// <summary>
    ///     Generate the mesh using all faces held by this holder.
    /// </summary>
    /// <param name="vertices">The list of vertices to add to.</param>
    /// <param name="indices">The list of indices to add to.</param>
    public void GenerateMesh(PooledList<SpatialVertex> vertices, PooledList<uint> indices)
    {
        if (count == 0) return;

        vertices.Capacity += count * 4;
        indices.Capacity += count * 6;

        for (var l = 0; l < Section.Size; l++)
        for (var r = 0; r < Section.Size; r++)
        {
            MeshFace? currentFace = lastFaces[l][r];

            while (currentFace != null)
            {
                if (side is BlockSide.Left or BlockSide.Right)
                    currentFace.isRotated = !currentFace.isRotated;

                Meshing.SetTextureRepetition(ref currentFace.data,
                    currentFace.isRotated,
                    currentFace.height,
                    currentFace.length);

                (Vector3, Vector3, Vector3, Vector3) positions = GetPositions(l, r, currentFace);

                Meshing.PushFaces(vertices, indices, positions, currentFace.data);

                MeshFace? next = currentFace.previous;
                currentFace.Return();
                currentFace = next;
            }
        }
    }

    private (Vector3, Vector3, Vector3, Vector3) GetPositions(int layer, int row, MeshFace face)
    {
        Vector3 position = RestorePosition(layer, row, face.position) + SideDependentOffset;

        // Both height and lenght are given in additional distance to the normal height and lenght of a quad, so we add 1.
        Vector3 lenght = LengthAxis.ToVector3() * (face.length + 1);
        Vector3 height = HeightAxis.ToVector3() * (face.height + 1);

        Vector3 v00 = position;
        Vector3 v01 = position + height;
        Vector3 v10 = position + lenght;
        Vector3 v11 = position + lenght + height;

        return side switch
        {
            BlockSide.Front => (v01, v11, v10, v00),
            BlockSide.Back => (v00, v10, v11, v01),
            BlockSide.Left => (v01, v00, v10, v11),
            BlockSide.Right => (v11, v10, v00, v01),
            BlockSide.Bottom => (v01, v11, v10, v00),
            BlockSide.Top => (v11, v01, v00, v10),
            BlockSide.All or _ => throw new InvalidOperationException()
        };
    }

    /// <summary>
    ///     Return all pooled resources used.
    /// </summary>
    public void ReturnToPool()
    {
        for (var i = 0; i < Section.Size; i++) ArrayPool<MeshFace>.Shared.Return(lastFaces[i]!);

        ArrayPool<MeshFace[]>.Shared.Return(lastFaces!);
    }

#pragma warning disable CA1812

    private sealed class MeshFace
    {
        public (uint a, uint b, uint c, uint d) data;
        public uint height;

        public bool isRotated;
        public uint length;

        public int position;
        public MeshFace? previous;

        public bool IsExtendable(MeshFace extension)
        {
            return position + length + 1 == extension.position &&
                   height == extension.height &&
                   isRotated == extension.isRotated;
        }

        public bool IsCombinable(MeshFace addition)
        {
            return position == addition.position &&
                   length == addition.length &&
                   isRotated == addition.isRotated &&
                   data == addition.data;
        }

        #region POOLING

        public static MeshFace Get((uint, uint, uint, uint) data, bool isRotated, int position)
        {
            MeshFace instance = ObjectPool<MeshFace>.Shared.Get();

            instance.previous = null;

            instance.data = data;
            instance.isRotated = isRotated;

            instance.position = position;
            instance.length = 0;
            instance.height = 0;

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
