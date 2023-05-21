﻿// <copyright file="MeshFaceHolder.cs" company="VoxelGame">
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
    }

    /// <summary>
    ///     The axis along which length extension happens.
    /// </summary>
    protected Vector3i LengthAxis { get; }

    /// <summary>
    ///     The axis along which height extension happens.
    /// </summary>
    protected Vector3i HeightAxis { get; }

    private (int, int, int) DetermineLengthAxis()
    {
        return side switch
        {
            BlockSide.Front or BlockSide.Back => (0, 1, 0),
            BlockSide.Left or BlockSide.Right => (0, 0, 1),
            BlockSide.Bottom or BlockSide.Top => (0, 0, 1),
            BlockSide.All or _ => throw new InvalidOperationException()
        };
    }

    private (int, int, int) DetermineHeightAxis()
    {
        return side switch
        {
            BlockSide.Front or BlockSide.Back => (1, 0, 0),
            BlockSide.Left or BlockSide.Right => (0, 1, 0),
            BlockSide.Bottom or BlockSide.Top => (1, 0, 0),
            BlockSide.All or _ => throw new InvalidOperationException()
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
            BlockSide.All or _ => throw new InvalidOperationException()
        };
    }
}
