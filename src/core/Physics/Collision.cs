// <copyright file="Collision.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Physics;

/// <summary>
///     Contains utility methods for collision detection.
/// </summary>
public static class Collision
{
    /// <summary>
    ///     Check if a ray intersects a box.
    /// </summary>
    /// <param name="box">The box to check.</param>
    /// <param name="ray">The ray to cast against the box.</param>
    /// <returns>True if the ray intersects the box.</returns>
    public static bool IsIntersecting(Box3 box, Ray ray)
    {
        if (box.Contains(ray.Origin) || box.Contains(ray.EndPoint)) return true;

        var dirfrac = new Vector3
        {
            X = 1.0f / ray.Direction.X,
            Y = 1.0f / ray.Direction.Y,
            Z = 1.0f / ray.Direction.Z
        };

        float t1 = (box.Min.X - ray.Origin.X) * dirfrac.X;
        float t2 = (box.Max.X - ray.Origin.X) * dirfrac.X;

        float t3 = (box.Min.Y - ray.Origin.Y) * dirfrac.Y;
        float t4 = (box.Max.Y - ray.Origin.Y) * dirfrac.Y;

        float t5 = (box.Min.Z - ray.Origin.Z) * dirfrac.Z;
        float t6 = (box.Max.Z - ray.Origin.Z) * dirfrac.Z;

        float tMin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
        float tMax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

        if (tMax < 0f) return false;

        return tMin <= tMax;
    }

    /// <summary>
    ///     Check if a box intersects another box.
    /// </summary>
    public static bool IsIntersecting(Box3 box, Box3 other)
    {
        bool intersectsX = box.Min.X <= other.Max.X && box.Max.X >= other.Min.X;
        bool intersectsY = box.Min.Y <= other.Max.Y && box.Max.Y >= other.Min.Y;
        bool intersectsZ = box.Min.Z <= other.Max.Z && box.Max.Z >= other.Min.Z;

        return intersectsX && intersectsY && intersectsZ;
    }

    /// <summary>
    ///     Check if a given box intersects another box. This also sets the collision planes.
    /// </summary>
    public static bool IsIntersecting(Box3 box, Box3 other, ref bool x, ref bool y, ref bool z)
    {
        bool containedInX = box.Min.X <= other.Max.X && box.Max.X >= other.Min.X;
        bool containedInY = box.Min.Y <= other.Max.Y && box.Max.Y >= other.Min.Y;
        bool containedInZ = box.Min.Z <= other.Max.Z && box.Max.Z >= other.Min.Z;

        if (containedInX && containedInY && containedInZ)
        {
            float inverseOverlap;

            // Check on which plane the collision happened
            float xOverlap = box.Max.X - other.Min.X;
            inverseOverlap = other.Max.X - box.Min.X;
            xOverlap = xOverlap < inverseOverlap ? xOverlap : inverseOverlap;

            float yOverlap = box.Max.Y - other.Min.Y;
            inverseOverlap = other.Max.Y - box.Min.Y;
            yOverlap = yOverlap < inverseOverlap ? yOverlap : inverseOverlap;

            float zOverlap = box.Max.Z - other.Min.Z;
            inverseOverlap = other.Max.Z - box.Min.Z;
            zOverlap = zOverlap < inverseOverlap ? zOverlap : inverseOverlap;

            if (xOverlap < yOverlap)
            {
                if (xOverlap < zOverlap) x = true;
                else z = true;
            }
            else
            {
                if (yOverlap < zOverlap) y = true;
                else z = true;
            }

            return true;
        }

        return false;
    }
}
