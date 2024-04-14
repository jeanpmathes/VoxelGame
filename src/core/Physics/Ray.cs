// <copyright file="Ray.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Physics;

/// <summary>
///     A ray trough 3D space.
/// </summary>
public readonly struct Ray : IEquatable<Ray>
{
    /// <summary>
    ///     The origin of the ray.
    /// </summary>
    public Vector3d Origin { get; }

    /// <summary>
    ///     The direction of the ray.
    /// </summary>
    public Vector3d Direction { get; }

    /// <summary>
    ///     The length of the ray.
    /// </summary>
    public Single Length { get; }

    /// <summary>
    ///     Create a new ray trough 3D space.
    /// </summary>
    public Ray(Vector3d origin, Vector3d direction, Single length)
    {
        Origin = origin;
        Direction = direction.Normalized();
        Length = length;
    }

    /// <summary>
    ///     Get a translated ray.
    /// </summary>
    public Ray Translated(Vector3d translation)
    {
        return new Ray(Origin + translation, Direction, Length);
    }

    /// <summary>
    ///     The end point of the ray.
    /// </summary>
    public Vector3d EndPoint => Origin + Direction * Length;

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return HashCode.Combine(Origin.GetHashCode(), Direction.GetHashCode());
    }

    /// <summary>
    ///     Compare two rays for equality.
    /// </summary>
    public static Boolean operator ==(Ray left, Ray right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Compare two rays for inequality.
    /// </summary>
    public static Boolean operator !=(Ray left, Ray right)
    {
        return !(left == right);
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        if (obj is Ray ray) return Equals(ray);

        return false;
    }

    /// <inheritdoc />
    public Boolean Equals(Ray other)
    {
        return Origin.Equals(other.Origin) && Direction.Equals(other.Direction);
    }
}
