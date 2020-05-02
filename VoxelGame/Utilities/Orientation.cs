// <copyright file="Orientation.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
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
    }
}
