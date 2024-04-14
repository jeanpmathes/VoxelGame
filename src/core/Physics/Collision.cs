// <copyright file="Collision.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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
    public static Boolean IsIntersecting(Box3d box, Ray ray)
    {
        if (box.Contains(ray.Origin, boundaryInclusive: true) || box.Contains(ray.EndPoint, boundaryInclusive: true)) return true;

        var dirfrac = new Vector3d
        {
            X = 1.0 / ray.Direction.X,
            Y = 1.0 / ray.Direction.Y,
            Z = 1.0 / ray.Direction.Z
        };

        Double t1 = (box.Min.X - ray.Origin.X) * dirfrac.X;
        Double t2 = (box.Max.X - ray.Origin.X) * dirfrac.X;

        Double t3 = (box.Min.Y - ray.Origin.Y) * dirfrac.Y;
        Double t4 = (box.Max.Y - ray.Origin.Y) * dirfrac.Y;

        Double t5 = (box.Min.Z - ray.Origin.Z) * dirfrac.Z;
        Double t6 = (box.Max.Z - ray.Origin.Z) * dirfrac.Z;

        Double tMin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
        Double tMax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

        if (tMax < 0.0) return false;

        return tMin <= tMax;
    }

    /// <summary>
    ///     Check if a box intersects another box.
    /// </summary>
    public static Boolean IsIntersecting(Box3d box, Box3d other)
    {
        Boolean intersectsX = box.Min.X <= other.Max.X && box.Max.X >= other.Min.X;
        Boolean intersectsY = box.Min.Y <= other.Max.Y && box.Max.Y >= other.Min.Y;
        Boolean intersectsZ = box.Min.Z <= other.Max.Z && box.Max.Z >= other.Min.Z;

        return intersectsX && intersectsY && intersectsZ;
    }

    /// <summary>
    ///     Check if a given box intersects another box. This also sets the collision planes.
    /// </summary>
    public static Boolean IsIntersecting(Box3d box, Box3d other, ref Boolean x, ref Boolean y, ref Boolean z)
    {
        Boolean containedInX = box.Min.X <= other.Max.X && box.Max.X >= other.Min.X;
        Boolean containedInY = box.Min.Y <= other.Max.Y && box.Max.Y >= other.Min.Y;
        Boolean containedInZ = box.Min.Z <= other.Max.Z && box.Max.Z >= other.Min.Z;

        if (!containedInX || !containedInY || !containedInZ) return false;

        Double inverseOverlap;

        // Check on which plane the collision happened.

        Double xOverlap = box.Max.X - other.Min.X;
        inverseOverlap = other.Max.X - box.Min.X;
        xOverlap = xOverlap < inverseOverlap ? xOverlap : inverseOverlap;

        Double yOverlap = box.Max.Y - other.Min.Y;
        inverseOverlap = other.Max.Y - box.Min.Y;
        yOverlap = yOverlap < inverseOverlap ? yOverlap : inverseOverlap;

        Double zOverlap = box.Max.Z - other.Min.Z;
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
}
