// <copyright file="FluidMeshFaceHolder.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Buffers;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Collections;

/// <summary>
///     A specialized class used to compact varying height faces while meshing.
/// </summary>
public class VaryingHeightMeshFaceHolder : MeshFaceHolder
{
    /// <summary>
    ///     The skip value that indicates no skip.
    /// </summary>
    public const int NoSkip = -1;

    /// <summary>
    ///     The direction value that indicates a normal direction, meaning a downwards flow.
    ///     The bottom of the face touches the ground, the top is as high as the size.
    /// </summary>
    public const bool DefaultDirection = true;

    private readonly MeshFace?[][] lastFaces;
    private readonly Vector3 inset;

    private int count;

    /// <summary>
    ///     Create a new <see cref="VaryingHeightMeshFaceHolder" /> for a given block side.
    /// </summary>
    /// <param name="side">The side the faces held belong too.</param>
    /// <param name="insetScale">How much to move the faces inwards.</param>
    public VaryingHeightMeshFaceHolder(BlockSide side, float insetScale) : base(side)
    {
        // Initialize layers.
        lastFaces = ArrayPool<MeshFace[]>.Shared.Rent(Section.Size);

        // Initialize rows.
        for (var i = 0; i < Section.Size; i++)
        {
            lastFaces[i] = ArrayPool<MeshFace>.Shared.Rent(Section.Size);

            for (var j = 0; j < Section.Size; j++) lastFaces[i][j] = null;
        }

        inset = side.Direction().ToVector3() * insetScale * -1.0f;
    }

    /// <summary>
    ///     Add a face to the holder.
    /// </summary>
    /// <param name="pos">The position of the face, in section block coordinates.</param>
    /// <param name="size">The size of the face.</param>
    /// <param name="skip">The portion of the face that is skipped, in size units. This means <c>0</c> is one step and <c>-1</c> is no skip.</param>
    /// <param name="direction">The direction of the face. True for upwards (default), false for downwards.</param>
    /// <param name="data">The binary encoded data of the face.</param>
    /// <param name="isSingleSided">True if this face is single sided, false if double sided.</param>
    /// <param name="isFull">True if this face is full, filling a complete block side.</param>
    public void AddFace(Vector3i pos, int size, int skip, bool direction, (uint a, uint b, uint c, uint d) data,
        bool isSingleSided, bool isFull)
    {
        ExtractIndices(pos, out int layer, out int row, out int position);

        // Build current face.
        MeshFace currentFace = MeshFace.Get(
            size,
            skip,
            direction,
            data,
            position,
            isSingleSided);

        // Front and Back faces cannot be extended (along the y axis) when the face is not all full level.
        bool levelPermitsExtending = side is not (BlockSide.Front or BlockSide.Back) || isFull;

        // Check if an already existing face can be extended.
        if (levelPermitsExtending && (lastFaces[layer][row]?.IsExtendable(currentFace) ?? false))
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

        // Left and right faces cannot be combined (along the y axis) when the face is not all full level.
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

    private static void ExtendFace(MeshFace face)
    {
        face.length++;
    }

    private static void CombineFace(MeshFace face, MeshFace combinationFace)
    {
        face.height = combinationFace.height + 1;
    }

    /// <summary>
    ///     Generate the mesh with all held faces.
    /// </summary>
    /// <param name="vertices">The mesh list to which all new vertices will be added.</param>
    public void GenerateMesh(PooledList<SpatialVertex> vertices)
    {
        if (count == 0) return;

        vertices.EnsureCapacity(vertices.Count + count * 4);

        for (var l = 0; l < Section.Size; l++)
        for (var r = 0; r < Section.Size; r++)
        {
            MeshFace? currentFace = lastFaces[l][r];

            while (currentFace != null)
            {
                bool isRotated = side is not (BlockSide.Left or BlockSide.Right);

                Meshing.SetTextureRepetition(ref currentFace.data,
                    isRotated,
                    currentFace.height,
                    currentFace.length);

                (Vector3, Vector3, Vector3, Vector3) positions = GetPositions(l, r, currentFace);
                ApplyVaryingHeight(ref positions, currentFace);

                PushQuads(vertices, positions, currentFace);

                MeshFace? next = currentFace.previous;
                currentFace.Return();
                currentFace = next;
            }
        }
    }

    private (Vector3, Vector3, Vector3, Vector3) GetPositions(int layer, int row, MeshFace face)
    {
        return GetPositions(layer, row, (face.position, face.length, face.height));
    }

    private void ApplyVaryingHeight(ref (Vector3 a, Vector3 b, Vector3 c, Vector3 d) positions, MeshFace face)
    {
        if (side is BlockSide.Top or BlockSide.Bottom) ApplyVaryingHeightToVerticalSide(ref positions, face);
        else ApplyVaryingHeightToLateralSide(ref positions, face);
    }

    private void ApplyVaryingHeightToLateralSide(ref (Vector3 a, Vector3 b, Vector3 c, Vector3 d) positions, MeshFace face)
    {
        Vector3 bottomOffset;
        Vector3 topOffset;

        float gap = IHeightVariable.GetGap(face.size);
        float skip = IHeightVariable.GetSize(face.skip);

        if (face.direction)
        {
            bottomOffset = (0, skip, 0);
            topOffset = (0, -gap, 0);
        }
        else
        {
            bottomOffset = (0, gap, 0);
            topOffset = (0, -skip, 0);
        }

        positions.a += bottomOffset + inset;
        positions.b += topOffset + inset;
        positions.c += topOffset + inset;
        positions.d += bottomOffset + inset;
    }

    private void ApplyVaryingHeightToVerticalSide(ref (Vector3 a, Vector3 b, Vector3 c, Vector3 d) positions, MeshFace face)
    {
        float gap = IHeightVariable.GetGap(face.size);
        Vector3 offset = inset;

        if (face.direction && side == BlockSide.Top) offset += (0, -gap, 0);
        if (!face.direction && side == BlockSide.Bottom) offset += (0, gap, 0);

        positions.a += offset;
        positions.b += offset;
        positions.c += offset;
        positions.d += offset;
    }

    private static void PushQuads(PooledList<SpatialVertex> vertices, (Vector3 a, Vector3 b, Vector3 c, Vector3 d) positions, MeshFace face)
    {
        Meshing.PushQuad(vertices, positions, face.data);

        if (face.isSingleSided) return;

        positions = (positions.d, positions.c, positions.b, positions.a);
        Meshing.MirrorUVs(ref face.data);

        Meshing.PushQuad(vertices, positions, face.data);
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
        public (uint a, uint b, uint c, uint d) data;

        /// <summary>
        ///     The direction of the face, either up (true) or down (false).
        ///     A face that goes up starts at the bottom of the block and goes up according to the size.
        ///     A face that goes down starts at the top of the block and goes down according to the size.
        /// </summary>
        public bool direction;

        public uint height;

        public bool isSingleSided;
        public uint length;

        public int position;
        public MeshFace? previous;

        /// <summary>
        ///     The size of the face, in the units used by <see cref="IHeightVariable" />.
        ///     Is referred to as the height of the face outside of this class.
        /// </summary>
        public int size;

        /// <summary>
        ///     The portion of the face that is skipped, in size units.
        ///     This can be the height of a neighboring block.
        /// </summary>
        public int skip;

        #pragma warning disable S1067
        public bool IsExtendable(MeshFace extension)
        {
            return position + length + 1 == extension.position &&
                   height == extension.height &&
                   size == extension.size &&
                   skip == extension.skip &&
                   direction == extension.direction &&
                   data == extension.data &&
                   isSingleSided == extension.isSingleSided;
        }

        public bool IsCombinable(MeshFace addition)
        {
            return position == addition.position &&
                   length == addition.length &&
                   size == addition.size &&
                   skip == addition.skip &&
                   direction == addition.direction &&
                   data == addition.data &&
                   isSingleSided == addition.isSingleSided;
        }
        #pragma warning restore S1067

        #region POOLING

        public static MeshFace Get(int size, int skip, bool direction, (uint a, uint b, uint c, uint d) data,
            int position, bool isSingleSided)
        {
            MeshFace instance = ObjectPool<MeshFace>.Shared.Get();

            instance.previous = null;

            instance.size = size;
            instance.skip = skip;
            instance.direction = direction;
            instance.data = data;

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
