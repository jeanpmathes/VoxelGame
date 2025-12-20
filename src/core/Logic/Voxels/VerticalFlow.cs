// <copyright file="VerticalFlow.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
///     The vertical flow direction of a fluid.
/// </summary>
public enum VerticalFlow
{
    /// <summary>
    ///     Flows up.
    /// </summary>
    Upwards,

    /// <summary>
    ///     Does not flow.
    /// </summary>
    Static,

    /// <summary>
    ///     Flows down.
    /// </summary>
    Downwards
}

/// <summary>
///     Extension methods for <see cref="VerticalFlow" />.
/// </summary>
public static class VerticalFlowExtensions
{
    /// <summary>
    ///     Get the flow as a direction vector.
    /// </summary>
    public static Vector3i Direction(this VerticalFlow flow)
    {
        return flow switch
        {
            VerticalFlow.Upwards => (0, 1, 0),
            VerticalFlow.Static => (0, 0, 0),
            VerticalFlow.Downwards => (0, -1, 0),
            _ => (0, 0, 0)
        };
    }

    /// <summary>
    ///     Get the flow encoded as a bit for shaders.
    ///     When encoded, static is ignored.
    /// </summary>
    public static Int32 GetBit(this VerticalFlow flow)
    {
        return flow switch
        {
            VerticalFlow.Upwards => 1,
            VerticalFlow.Static => 0,
            VerticalFlow.Downwards => 0,
            _ => 0
        };
    }

    /// <summary>
    ///     Get the opposite flow.
    /// </summary>
    public static VerticalFlow Opposite(this VerticalFlow flow)
    {
        return flow switch
        {
            VerticalFlow.Upwards => VerticalFlow.Downwards,
            VerticalFlow.Static => VerticalFlow.Static,
            VerticalFlow.Downwards => VerticalFlow.Upwards,
            _ => VerticalFlow.Static
        };
    }

    /// <summary>
    ///     Get the <see cref="Side" /> trough which the flows exists a block.
    /// </summary>
    public static Side ExitSide(this VerticalFlow flow)
    {
        return flow switch
        {
            VerticalFlow.Upwards => Side.Top,
            VerticalFlow.Static => Side.All,
            VerticalFlow.Downwards => Side.Bottom,
            _ => Side.All
        };
    }

    /// <summary>
    ///     Get the <see cref="Side" /> trough which the flows enters a block.
    /// </summary>
    public static Side EntrySide(this VerticalFlow flow)
    {
        return flow.ExitSide().Opposite();
    }
}
