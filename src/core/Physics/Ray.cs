// <copyright file="Ray.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Physics
{
    /// <summary>
    ///     A ray trough 3D space.
    /// </summary>
    public readonly struct Ray : IEquatable<Ray>
    {
        /// <summary>
        ///     The origin of the ray.
        /// </summary>
        public Vector3 Origin { get; }

        /// <summary>
        ///     The direction of the ray.
        /// </summary>
        public Vector3 Direction { get; }

        /// <summary>
        ///     The length of the ray.
        /// </summary>
        public float Length { get; }

        /// <summary>
        ///     Create a new ray trough 3D space.
        /// </summary>
        public Ray(Vector3 origin, Vector3 direction, float length)
        {
            Origin = origin;
            Direction = direction.Normalized();
            Length = length;
        }

        /// <summary>
        ///     The end point of the ray.
        /// </summary>
        public Vector3 EndPoint => Origin + Direction * Length;

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Origin.GetHashCode(), Direction.GetHashCode());
        }

        /// <summary>
        ///     Compare two rays for equality.
        /// </summary>
        public static bool operator ==(Ray left, Ray right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Compare two rays for inequality.
        /// </summary>
        public static bool operator !=(Ray left, Ray right)
        {
            return !(left == right);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is Ray ray) return Equals(ray);

            return false;
        }

        /// <inheritdoc />
        public bool Equals(Ray other)
        {
            return Origin.Equals(other.Origin) && Direction.Equals(other.Direction);
        }
    }
}