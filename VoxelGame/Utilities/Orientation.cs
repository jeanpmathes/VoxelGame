// <copyright file="Orientation.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using System;

namespace VoxelGame.Utilities
{
    public enum Orientation
    {
        North = 0b00,
        East = 0b01,
        South = 0b10,
        West = 0b11
    }

    public static class OrientationExtensions
    {
        public static Orientation ToOrientation(this Vector3 vector)
        {
            if (Math.Abs(vector.Z) > Math.Abs(vector.X))
            {
                return (vector.Z > 0) ? Orientation.South : Orientation.North;
            }
            else
            {
                return (vector.X > 0) ? Orientation.East : Orientation.West;
            }
        }

        public static Vector3 ToVector(this Orientation orientation)
        {
            return orientation switch
            {
                Orientation.North => -Vector3.UnitZ,
                Orientation.East => Vector3.UnitX,
                Orientation.South => Vector3.UnitZ,
                Orientation.West => -Vector3.UnitX,
                _ => -Vector3.UnitZ
            };
        }

        public static Orientation Invert(this Orientation orientation)
        {
            return orientation switch
            {
                Orientation.North => Orientation.South,
                Orientation.East => Orientation.West,
                Orientation.South => Orientation.North,
                Orientation.West => Orientation.East,
                _ => Orientation.North
            };
        }

        public static Orientation Rotate(this Orientation orientation)
        {
            return orientation switch
            {
                Orientation.North => Orientation.East,
                Orientation.East => Orientation.South,
                Orientation.South => Orientation.West,
                Orientation.West => Orientation.North,
                _ => Orientation.North
            };
        }
    }
}