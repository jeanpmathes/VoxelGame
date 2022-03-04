// <copyright file="Axis.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Utilities
{
    /// <summary>
    ///     An axis.
    /// </summary>
    public enum Axis
    {
        /// <summary>
        ///     The x axis.
        /// </summary>
        X = 0b00,

        /// <summary>
        ///     The y axis.
        /// </summary>
        Y = 0b01,

        /// <summary>
        ///     The z axis.
        /// </summary>
        Z = 0b10
    }

    /// <summary>
    ///     Extension methods for <see cref="Axis" />.
    /// </summary>
    public static class AxisExtensions
    {
        /// <summary>
        ///     Rotate an axis around the y axis.
        /// </summary>
        public static Axis Rotate(this Axis axis)
        {
            return axis switch
            {
                Axis.X => Axis.Z,
                Axis.Y => Axis.Y,
                Axis.Z => Axis.X,
                _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, message: null)
            };
        }

        /// <summary>
        ///     Get the axis as a <see cref="Vector3" />.
        /// </summary>
        public static Vector3 Vector3(this Axis axis, float onAxis, float other)
        {
            return axis switch
            {
                Axis.X => new Vector3(onAxis, other, other),
                Axis.Y => new Vector3(other, onAxis, other),
                Axis.Z => new Vector3(other, other, onAxis),
                _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, message: null)
            };
        }
    }
}
