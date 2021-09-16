﻿// <copyright file="Orientation.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics.CodeAnalysis;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Utilities
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
            if (Math.Abs(vector.Z) > Math.Abs(vector.X)) return vector.Z > 0 ? Orientation.South : Orientation.North;

            return vector.X > 0 ? Orientation.East : Orientation.West;
        }

        public static Vector3 ToVector3(this Orientation orientation)
        {
            return orientation.ToVector3i().ToVector3();
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static Vector3i ToVector3i(this Orientation orientation)
        {
            return orientation switch
            {
                Orientation.North => -Vector3i.UnitZ,
                Orientation.East => Vector3i.UnitX,
                Orientation.South => Vector3i.UnitZ,
                Orientation.West => -Vector3i.UnitX,
                _ => -Vector3i.UnitZ
            };
        }

        public static BlockSide ToBlockSide(this Orientation orientation)
        {
            return orientation switch
            {
                Orientation.North => BlockSide.Back,
                Orientation.East => BlockSide.Right,
                Orientation.South => BlockSide.Front,
                Orientation.West => BlockSide.Left,
                _ => BlockSide.Back
            };
        }

        public static Orientation Opposite(this Orientation orientation)
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

        public static Vector3i Offset(this Orientation orientation, Vector3i vector)
        {
            return vector + orientation.ToVector3i();
        }

        public static uint ToFlag(this Orientation orientation)
        {
            return orientation switch
            {
                Orientation.North => 0b1000,
                Orientation.East => 0b0100,
                Orientation.South => 0b0010,
                Orientation.West => 0b0001,
                _ => 0b1000
            };
        }

        /// <summary>
        ///     Check if this orientation is on the x axis.
        /// </summary>
        public static bool IsX(this Orientation orientation)
        {
            return orientation.Axis() == Utilities.Axis.X;
        }

        /// <summary>
        ///     Check if this orientation is on the z axis.
        /// </summary>
        public static bool IsZ(this Orientation orientation)
        {
            return orientation.Axis() == Utilities.Axis.Z;
        }

        public static Axis Axis(this Orientation orientation)
        {
            return orientation switch
            {
                Orientation.North => Utilities.Axis.Z,
                Orientation.East => Utilities.Axis.X,
                Orientation.South => Utilities.Axis.Z,
                Orientation.West => Utilities.Axis.X,
                _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, message: null)
            };
        }
    }
}