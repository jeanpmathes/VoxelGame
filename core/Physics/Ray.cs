// <copyright file="Ray.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using System;

namespace VoxelGame.Core.Physics
{
    public struct Ray : IEquatable<Ray>
    {
        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public float Length { get; }

        public Ray(Vector3 origin, Vector3 direction, float length)
        {
            Origin = origin;
            Direction = direction.Normalized();
            Length = length;
        }

        public Vector3 EndPoint
        {
            get
            {
                return Origin + (Direction * Length);
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Origin.GetHashCode(), Direction.GetHashCode());
        }

        public static bool operator ==(Ray left, Ray right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Ray left, Ray right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            if (obj is Ray ray)
            {
                return Equals(other: ray);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(Ray other)
        {
            return Origin.Equals(other.Origin) && Direction.Equals(other.Direction);
        }
    }
}