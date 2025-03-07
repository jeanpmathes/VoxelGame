﻿// <copyright file="Axis.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     An axis.
/// </summary>
public enum Axis
{
    /// <summary>
    ///     The x-axis.
    /// </summary>
    X = 0b00,

    /// <summary>
    ///     The y-axis.
    /// </summary>
    Y = 0b01,

    /// <summary>
    ///     The z-axis.
    /// </summary>
    Z = 0b10
}

/// <summary>
///     Extension methods for <see cref="Axis" />.
/// </summary>
public static class AxisExtensions
{
    /// <summary>
    ///     Rotate an axis around the y-axis.
    /// </summary>
    public static Axis Rotate(this Axis axis)
    {
        return axis switch
        {
            Axis.X => Axis.Z,
            Axis.Y => Axis.Y,
            Axis.Z => Axis.X,
            _ => throw Exceptions.UnsupportedEnumValue(axis)
        };
    }

    /// <summary>
    ///     Get the axis as a <see cref="Vector3" />.
    /// </summary>
    public static Vector3d Vector3(this Axis axis, Single onAxis, Single other)
    {
        return axis switch
        {
            Axis.X => new Vector3d(onAxis, other, other),
            Axis.Y => new Vector3d(other, onAxis, other),
            Axis.Z => new Vector3d(other, other, onAxis),
            _ => throw Exceptions.UnsupportedEnumValue(axis)
        };
    }
}
