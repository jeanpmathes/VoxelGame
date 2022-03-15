// <copyright file="Plane.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Physics
{
    /// <summary>
    ///     A plane in 3D space.
    /// </summary>
    public readonly struct Plane : IEquatable<Plane>
    {
        /// <summary>
        ///     The normal of the plane.
        /// </summary>
        public Vector3 Normal { get; }

        /// <summary>
        ///     A point that is in the plane.
        /// </summary>
        public Vector3 Point { get; }

        private readonly float d;

        /// <summary>
        /// Creates a plane. The normal parameter has to be normalized.
        /// </summary>
        /// <param name="normal">The normalized normal vector.</param>
        /// <param name="point">A point in the plane.</param>
        public Plane(Vector3 normal, Vector3 point)
        {
            Normal = normal;
            Point = point;

            d = -Vector3.Dot(normal, point);
        }

        /// <summary>
        ///     Calculate the distance from a point to the plane.
        /// </summary>
        /// <param name="point">The point to calculate the distance to.</param>
        /// <returns>The distance to the point.</returns>
        public float Distance(Vector3 point)
        {
            return Vector3.Dot(point, Normal) + d;
        }

        /// <inheritdoc />
        public bool Equals(Plane other)
        {
            return Normal.Equals(other.Normal) && Point.Equals(other.Point);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Plane other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Normal, Point);
        }

        /// <summary>
        ///     Checks if two planes are equal.
        /// </summary>
        public static bool operator ==(Plane left, Plane right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Checks if two planes are not equal.
        /// </summary>
        public static bool operator !=(Plane left, Plane right)
        {
            return !left.Equals(right);
        }
    }
}
