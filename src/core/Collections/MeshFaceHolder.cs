// <copyright file="MeshFaceHolder.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Buffers;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Collections;

/// <summary>
///     Holds and combines mesh faces, that then can be used to create a final mesh.
/// </summary>
public class MeshFaceHolder
{
    /// <summary>
    ///     The direction value that indicates a normal direction, meaning a downwards flow.
    ///     The bottom of the face touches the ground, the top is as high as the size.
    /// </summary>
    public const Boolean DefaultDirection = true;

    private readonly Side side;

    private readonly MeshFace?[][] lastFaces;
    private readonly Vector3 inset;

    private Int32 count;

    /// <summary>
    ///     Create a new <see cref="MeshFaceHolder" /> for a given block side.
    /// </summary>
    /// <param name="side">The side the faces held belong too.</param>
    /// <param name="insetScale">How much to move the faces inwards.</param>
    public MeshFaceHolder(Side side, Single insetScale)
    {
        this.side = side;

        LengthAxis = DetermineLengthAxis();
        HeightAxis = DetermineHeightAxis();
        SideDependentOffset = DetermineSideDependentOffset();

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
    ///     The axis along which length extension (extend) happens.
    /// </summary>
    private Vector3i LengthAxis { get; }

    /// <summary>
    ///     The axis along which height extension (combine) happens.
    /// </summary>
    private Vector3i HeightAxis { get; }

    /// <summary>
    ///     An offset that is applied to the position of the face, depending on the side.
    ///     This offset closes gaps when the axis directions are negative.
    /// </summary>
    private Vector3i SideDependentOffset { get; }

    private Vector3i DetermineLengthAxis()
    {
        return side switch
        {
            Side.Front => (0, 1, 0),
            Side.Back => (0, 1, 0),
            Side.Left => (0, 0, 1),
            Side.Right => (0, 0, 1),
            Side.Bottom => (0, 0, 1),
            Side.Top => (0, 0, 1),
            _ => throw new InvalidOperationException()
        };
    }

    private Vector3i DetermineHeightAxis()
    {
        return side switch
        {
            Side.Front => (-1, 0, 0),
            Side.Back => (-1, 0, 0),
            Side.Left => (0, -1, 0),
            Side.Right => (0, -1, 0),
            Side.Bottom => (-1, 0, 0),
            Side.Top => (-1, 0, 0),
            _ => throw new InvalidOperationException()
        };
    }

    private Vector3i DetermineSideDependentOffset()
    {
        return side switch
        {
            Side.Front => (1, 0, 1),
            Side.Back => (1, 0, 0),
            Side.Left => (0, 1, 0),
            Side.Right => (1, 1, 0),
            Side.Bottom => (1, 0, 0),
            Side.Top => (1, 1, 0),
            _ => throw new InvalidOperationException()
        };
    }

    /// <summary>
    ///     Extract the layer and row indices from the given block position.
    /// </summary>
    /// <param name="pos">The block position.</param>
    /// <param name="layer">The extracted layer index.</param>
    /// <param name="row">The extracted row index.</param>
    /// <param name="position">The extracted position in the row.</param>
    private void ExtractIndices(Vector3i pos, out Int32 layer, out Int32 row, out Int32 position)
    {
        switch (side)
        {
            case Side.Front:
            case Side.Back:
                layer = pos.Z;
                row = pos.X;
                position = pos.Y;

                break;

            case Side.Left:
            case Side.Right:
                layer = pos.X;
                row = pos.Y;
                position = pos.Z;

                break;

            case Side.Bottom:
            case Side.Top:
                layer = pos.Y;
                row = pos.X;
                position = pos.Z;

                break;

            default:
                throw new InvalidOperationException();
        }
    }

    /// <summary>
    ///     Restore the position from the given indices.
    /// </summary>
    private Vector3i RestorePosition(Int32 layer, Int32 row, Int32 position)
    {
        return side switch
        {
            Side.Front => new Vector3i(row, position, layer),
            Side.Back => new Vector3i(row, position, layer),
            Side.Left => new Vector3i(layer, row, position),
            Side.Right => new Vector3i(layer, row, position),
            Side.Bottom => new Vector3i(row, layer, position),
            Side.Top => new Vector3i(row, layer, position),
            _ => throw new InvalidOperationException()
        };
    }

    /// <summary>
    ///     Add a new face to the holder.
    /// </summary>
    /// <param name="pos">The position of the face, relative to the section origin.</param>
    /// <param name="data">The binary encoded data of the quad.</param>
    /// <param name="isRotated">True if the face is rotated.</param>
    /// <param name="isSingleSided">True if the face is single sided, false if double sided.</param>
    public void AddFace(Vector3i pos, (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data, Boolean isRotated, Boolean isSingleSided)
    {
        AddFace(pos, (IHeightVariable.MaximumHeight, IHeightVariable.NoHeight), data, isSingleSided, isFull: true, isRotated, DefaultDirection);
    }

    /// <summary>
    ///     Add a face to the holder.
    /// </summary>
    /// <param name="pos">The position of the face, in section block coordinates.</param>
    /// <param name="size">The size of the face, <see cref="IHeightVariable" /> units.</param>
    /// <param name="skip">
    ///     The portion of the face that is skipped, in size units. This means <c>0</c> is one step and
    ///     <c>-1</c> is no skip.
    /// </param>
    /// <param name="direction">The direction of the face. True for upwards (default), false for downwards.</param>
    /// <param name="data">The binary encoded data of the face.</param>
    /// <param name="isSingleSided">
    ///     True if this face is single sided, false if double sided.
    ///     If true, the second face will have a flipped normal.
    /// </param>
    /// <param name="isFull">True if this face is full, filling a complete block side.</param>
    public void AddFace(Vector3i pos, Int32 size, Int32 skip, Boolean direction, (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data,
        Boolean isSingleSided, Boolean isFull)
    {
        AddFace(pos, (size, skip), data, isSingleSided, isFull, isRotated: false, direction);
    }

    private void AddFace(Vector3i pos, (Int32 size, Int32 skip) dimensions, (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data,
        Boolean isSingleSided, Boolean isFull, Boolean isRotated, Boolean direction)
    {
        ExtractIndices(pos, out Int32 layer, out Int32 row, out Int32 position);

        // Build current face.
        MeshFace currentFace = MeshFace.Get(
            dimensions.size,
            dimensions.skip,
            direction,
            data,
            position,
            isSingleSided,
            isRotated);

        // Front and Back faces cannot be extended (along the y axis) when the face is not all full level.
        Boolean levelPermitsExtending = side is not (Side.Front or Side.Back) || isFull;

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
        if (side is Side.Left or Side.Right && !isFull) return;

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

        void RemoveFace(MeshFace toRemove, MeshFace? last, Int32 l, Int32 r)
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
    /// <param name="meshing">The meshing object to which the mesh is added.</param>
    public void GenerateMesh(IMeshing meshing)
    {
        if (count == 0) return;

        meshing.Grow(IMeshing.Primitive.Quad, count);

        for (var l = 0; l < Section.Size; l++)
        for (var r = 0; r < Section.Size; r++)
        {
            MeshFace? currentFace = lastFaces[l][r];

            while (currentFace != null)
            {
                if (side is not Side.Left and not Side.Right)
                    currentFace.isRotated = !currentFace.isRotated;

                Meshing.SetTextureRepetition(ref currentFace.data,
                    currentFace.isRotated,
                    currentFace.height,
                    currentFace.length);

                (Vector3, Vector3, Vector3, Vector3) positions = GetPositions(l, r, currentFace);
                ApplyVaryingHeight(ref positions, currentFace);

                PushQuads(meshing, positions, currentFace);

                MeshFace? next = currentFace.previous;
                currentFace.Return();
                currentFace = next;
            }
        }
    }

    private (Vector3, Vector3, Vector3, Vector3) GetPositions(Int32 layer, Int32 row, MeshFace face)
    {
        return GetPositions(layer, row, (face.position, face.length, face.height));
    }

    /// <summary>
    ///     Get the positions of the full face.
    /// </summary>
    private (Vector3, Vector3, Vector3, Vector3) GetPositions(Int32 layer, Int32 row, (Int32 position, UInt32 length, UInt32 height) face)
    {
        Vector3 position = RestorePosition(layer, row, face.position) + SideDependentOffset;

        // Both height and length are given in additional distance to the normal height and length of a quad, so we add 1.
        Vector3 length = LengthAxis.ToVector3() * (face.length + 1);
        Vector3 height = HeightAxis.ToVector3() * (face.height + 1);

        Vector3 v00 = position;
        Vector3 v01 = position + height;
        Vector3 v10 = position + length;
        Vector3 v11 = position + length + height;

        return side switch
        {
            Side.Front => (v01, v11, v10, v00),
            Side.Back => (v00, v10, v11, v01),
            Side.Left => (v01, v00, v10, v11),
            Side.Right => (v11, v10, v00, v01),
            Side.Bottom => (v01, v11, v10, v00),
            Side.Top => (v11, v01, v00, v10),
            _ => throw new InvalidOperationException()
        };
    }

    private void ApplyVaryingHeight(ref (Vector3 a, Vector3 b, Vector3 c, Vector3 d) positions, MeshFace face)
    {
        if (side is Side.Top or Side.Bottom) ApplyVaryingHeightToVerticalSide(ref positions, face);
        else ApplyVaryingHeightToLateralSide(ref positions, face);
    }

    private void ApplyVaryingHeightToLateralSide(ref (Vector3 a, Vector3 b, Vector3 c, Vector3 d) positions, MeshFace face)
    {
        Vector3 bottomOffset;
        Vector3 topOffset;

        Single gap = IHeightVariable.GetGap(face.size);
        Single skip = IHeightVariable.GetSize(face.skip);

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
        Single gap = IHeightVariable.GetGap(face.size);
        Vector3 offset = inset;

        if (face.direction && side == Side.Top) offset += (0, -gap, 0);
        if (!face.direction && side == Side.Bottom) offset += (0, gap, 0);

        positions.a += offset;
        positions.b += offset;
        positions.c += offset;
        positions.d += offset;
    }

    private static void PushQuads(IMeshing meshing, (Vector3 a, Vector3 b, Vector3 c, Vector3 d) positions, MeshFace face)
    {
        meshing.PushQuad(positions, face.data);

        if (face.isSingleSided) return;

        positions = (positions.d, positions.c, positions.b, positions.a);

        Meshing.MirrorUVs(ref face.data);
        Meshing.SetFlag(ref face.data, Meshing.QuadFlag.IsNormalInverted, value: true);

        meshing.PushQuad(positions, face.data);
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
        public (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data;

        /// <summary>
        ///     The direction of the face, either up (true) or down (false).
        ///     A face that goes up starts at the bottom of the block and goes up according to the size.
        ///     A face that goes down starts at the top of the block and goes down according to the size.
        /// </summary>
        public Boolean direction;

        /// <summary>
        ///     The size of the face, in the units used by <see cref="IHeightVariable" />.
        ///     Is referred to as the height of the face outside of this class.
        /// </summary>
        public Int32 size;

        /// <summary>
        ///     The portion of the face that is skipped, in size units.
        ///     This can be the height of a neighboring block.
        /// </summary>
        public Int32 skip;

        public UInt32 height;
        public UInt32 length;
        public Int32 position;

        public Boolean isSingleSided;
        public Boolean isRotated;

        public MeshFace? previous;

        #pragma warning disable S1067
        public Boolean IsExtendable(MeshFace extension)
        {
            return position + length + 1 == extension.position &&
                   height == extension.height &&
                   size == extension.size &&
                   skip == extension.skip &&
                   direction == extension.direction &&
                   data == extension.data &&
                   isSingleSided == extension.isSingleSided &&
                   isRotated == extension.isRotated;
        }

        public Boolean IsCombinable(MeshFace addition)
        {
            return position == addition.position &&
                   length == addition.length &&
                   size == addition.size &&
                   skip == addition.skip &&
                   direction == addition.direction &&
                   data == addition.data &&
                   isSingleSided == addition.isSingleSided &&
                   isRotated == addition.isRotated;
        }
        #pragma warning restore S1067

        #region POOLING

        public static MeshFace Get(Int32 size, Int32 skip, Boolean direction, (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data,
            Int32 position, Boolean isSingleSided, Boolean isRotated)
        {
            MeshFace instance = SimpleObjectPool<MeshFace>.Shared.Get();

            instance.previous = null;

            instance.size = size;
            instance.skip = skip;
            instance.direction = direction;
            instance.data = data;

            instance.position = position;
            instance.length = 0;
            instance.height = 0;

            instance.isSingleSided = isSingleSided;
            instance.isRotated = isRotated;

            return instance;
        }

        public void Return()
        {
            SimpleObjectPool<MeshFace>.Shared.Return(this);
        }

        #endregion POOLING
    }

#pragma warning restore CA1812
}
