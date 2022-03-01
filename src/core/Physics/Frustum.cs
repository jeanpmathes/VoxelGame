// <copyright file="Frustum.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Physics
{
    /// <summary>
    ///     A camera view frustum.
    /// </summary>
#pragma warning disable CA1815 // Override equals and operator equals on value types
    public readonly struct Frustum
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        private readonly Plane[] planes;

        /// <summary>
        ///     Create a new frustum.
        /// </summary>
        /// <param name="fovY">The field-of-view value, on the y axis.</param>
        /// <param name="ratio">The screen ratio.</param>
        /// <param name="clip">The distances to the near and far clipping planes.</param>
        /// <param name="position">The position of the camera.</param>
        /// <param name="direction">The view direction.</param>
        /// <param name="up">The up direction.</param>
        /// <param name="right">The right direction.</param>
        public Frustum(float fovY, float ratio, (float near, float far) clip,
            Vector3 position, Vector3 direction, Vector3 up, Vector3 right)
        {
            direction = direction.Normalized();
            up = up.Normalized();
            right = right.Normalized();

            var hNear = (float) (2f * Math.Tan(fovY / 2f) * clip.near);
            float wNear = hNear * ratio;

            Vector3 nc = position + direction * clip.near;
            Vector3 fc = position + direction * clip.far;

            Vector3 nl = Vector3.Cross((nc - right * wNear / 2f - position).Normalized(), up);
            Vector3 nr = Vector3.Cross(up, (nc + right * wNear / 2f - position).Normalized());

            Vector3 nb = Vector3.Cross(right, (nc - up * hNear / 2f - position).Normalized());
            Vector3 nt = Vector3.Cross((nc + up * hNear / 2f - position).Normalized(), right);

            planes = new[]
            {
                new Plane(direction, nc), // Near.
                new Plane(-direction, fc), // Far.
                new Plane(nl, position), // Left.
                new Plane(nr, position), // Right.
                new Plane(nb, position), // Bottom.
                new Plane(nt, position) // Top.
            };
        }

        /// <summary>
        ///     Check whether a <see cref="BoundingBox" /> is inside this <see cref="Frustum" />.
        /// </summary>
        /// <returns>true if the <see cref="BoundingBox" /> is inside; false if not.</returns>
        public bool BoxInFrustum(BoundingBox box)
        {
            for (var i = 0; i < 6; i++)
            {
                float px = planes[i].Normal.X < 0 ? box.Min.X : box.Max.X;
                float py = planes[i].Normal.Y < 0 ? box.Min.Y : box.Max.Y;
                float pz = planes[i].Normal.Z < 0 ? box.Min.Z : box.Max.Z;

                if (planes[i].Distance(new Vector3(px, py, pz)) < 0) return false;
            }

            return true;
        }
    }
}
