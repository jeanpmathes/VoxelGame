// <copyright file="MeshFaceHolder.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Collections;

/// <summary>
///     Holds and combines mesh faces, that then can be used to create a final mesh.
/// </summary>
public class MeshFaceHolder
{
    private protected readonly BlockSide side;

    /// <summary>
    ///     Create a new <see cref="MeshFaceHolder" />
    /// </summary>
    /// <param name="side">The block side to target.</param>
    protected MeshFaceHolder(BlockSide side)
    {
        this.side = side;

        LengthAxis = DetermineLengthAxis();
        HeightAxis = DetermineHeightAxis();
        SideDependentOffset = DetermineSideDependentOffset();
    }

    /// <summary>
    ///     The axis along which length extension (extend) happens.
    /// </summary>
    protected Vector3i LengthAxis { get; }

    /// <summary>
    ///     The axis along which height extension (combine) happens.
    /// </summary>
    protected Vector3i HeightAxis { get; }

    /// <summary>
    ///     An offset that is applied to the position of the face, depending on the side.
    ///     This offset closes gaps when the axis directions are negative.
    /// </summary>
    protected Vector3i SideDependentOffset { get; }

    private Vector3i DetermineLengthAxis()
    {
        return side switch
        {
            BlockSide.Front => (0, 1, 0),
            BlockSide.Back => (0, 1, 0),
            BlockSide.Left => (0, 0, 1),
            BlockSide.Right => (0, 0, 1),
            BlockSide.Bottom => (0, 0, 1),
            BlockSide.Top => (0, 0, 1),
            _ => throw new InvalidOperationException()
        };
    }

    private Vector3i DetermineHeightAxis()
    {
        return side switch
        {
            BlockSide.Front => (-1, 0, 0),
            BlockSide.Back => (-1, 0, 0),
            BlockSide.Left => (0, -1, 0),
            BlockSide.Right => (0, -1, 0),
            BlockSide.Bottom => (-1, 0, 0),
            BlockSide.Top => (-1, 0, 0),
            _ => throw new InvalidOperationException()
        };
    }

    private Vector3i DetermineSideDependentOffset()
    {
        return side switch
        {
            BlockSide.Front => (1, 0, 1),
            BlockSide.Back => (1, 0, 0),
            BlockSide.Left => (0, 1, 0),
            BlockSide.Right => (1, 1, 0),
            BlockSide.Bottom => (1, 0, 0),
            BlockSide.Top => (1, 1, 0),
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
    protected void ExtractIndices(Vector3i pos, out int layer, out int row, out int position)
    {
        switch (side)
        {
            case BlockSide.Front:
            case BlockSide.Back:
                layer = pos.Z;
                row = pos.X;
                position = pos.Y;

                break;

            case BlockSide.Left:
            case BlockSide.Right:
                layer = pos.X;
                row = pos.Y;
                position = pos.Z;

                break;

            case BlockSide.Bottom:
            case BlockSide.Top:
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
    protected Vector3i RestorePosition(int layer, int row, int position)
    {
        return side switch
        {
            BlockSide.Front => new Vector3i(row, position, layer),
            BlockSide.Back => new Vector3i(row, position, layer),
            BlockSide.Left => new Vector3i(layer, row, position),
            BlockSide.Right => new Vector3i(layer, row, position),
            BlockSide.Bottom => new Vector3i(row, layer, position),
            BlockSide.Top => new Vector3i(row, layer, position),
            _ => throw new InvalidOperationException()
        };
    }

    /// <summary>
    ///     Get the positions of the full face.
    /// </summary>
    protected (Vector3, Vector3, Vector3, Vector3) GetPositions(int layer, int row, (int position, uint length, uint height) face)
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
            _ => throw new InvalidOperationException()
        };
    }
}
