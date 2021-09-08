// <copyright file="Frustum.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Physics
{
#pragma warning disable CA1815 // Override equals and operator equals on value types

    public struct Frustum
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        private readonly Plane[] planes;

        public Frustum(float fovy, float ratio, float near, float far, Vector3 pos, Vector3 dir, Vector3 up,
            Vector3 right)
        {
            dir = dir.Normalized();
            up = up.Normalized();
            right = right.Normalized();

            var hnear = (float) (2f * Math.Tan(fovy / 2f) * near);
            float wnear = hnear * ratio;

            Vector3 nc = pos + dir * near;
            Vector3 fc = pos + dir * far;

            Vector3 nl = Vector3.Cross((nc - right * wnear / 2f - pos).Normalized(), up);
            Vector3 nr = Vector3.Cross(up, (nc + right * wnear / 2f - pos).Normalized());

            Vector3 nb = Vector3.Cross(right, (nc - up * hnear / 2f - pos).Normalized());
            Vector3 nt = Vector3.Cross((nc + up * hnear / 2f - pos).Normalized(), right);

            planes = new[]
            {
                new Plane(dir, nc), // Near.
                new Plane(-dir, fc), // Far.
                new Plane(nl, pos), // Left.
                new Plane(nr, pos), // Right.
                new Plane(nb, pos), // Bottom.
                new Plane(nt, pos) // Top.
            };
        }

        /// <summary>
        ///     Check whether a <see cref="BoundingBox" /> is inside this <see cref="Frustum" />.
        /// </summary>
        /// <returns>true if the <see cref="BoundingBox" /> is inside; false if not.</returns>
        public bool BoxInFrustum(BoundingBox box)
        {
            float px, py, pz;

            for (var i = 0; i < 6; i++)
            {
                if (planes[i].Normal.X < 0) px = box.Min.X;
                else px = box.Max.X;

                if (planes[i].Normal.Y < 0) py = box.Min.Y;
                else py = box.Max.Y;

                if (planes[i].Normal.Z < 0) pz = box.Min.Z;
                else pz = box.Max.Z;

                if (planes[i].Distance(new Vector3(px, py, pz)) < 0) return false;
            }

            return true;
        }
    }
}