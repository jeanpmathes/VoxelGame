// <copyright file="Axis.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Utilities
{
    public enum Axis
    {
        X = 0b00,
        Y = 0b01,
        Z = 0b10
    }

    public static class AxisExtensions
    {
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