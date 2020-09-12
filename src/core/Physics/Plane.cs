// <copyright file="Plane.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Physics
{
#pragma warning disable CA1815 // Override equals and operator equals on value types

    public struct Plane
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        public Vector3 Normal { get; }
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

        public float Distance(Vector3 point)
        {
            return Vector3.Dot(point, Normal) + d;
        }
    }
}