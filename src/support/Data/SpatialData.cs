// <copyright file="SpatialData.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace VoxelGame.Support.Data;

#pragma warning disable S3898 // No equality comparison used.

/// <summary>
///     The vertex layout used by all basic meshes.
///     A mesh is simply a sequence of quads with their vertices in CW order.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpatialVertex
{
    /// <summary>
    ///     The position of the vertex.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    ///     Additional data for the vertex. The complete shader data is split over the four vertices of a quad.
    /// </summary>
    public uint Data;
}

/// <summary>
///     An axis-aligned bounding box.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
#pragma warning disable S101
public readonly struct AABB
#pragma warning restore S101
{
    /// <summary>
    ///     Creates a new AABB from the given min and max values.
    /// </summary>
    public AABB(Vector3 min, Vector3 max) : this()
    {
        MinX = min.X;
        MinY = min.Y;
        MinZ = min.Z;
        MaxX = max.X;
        MaxY = max.Y;
        MaxZ = max.Z;
    }

    /// <summary>
    ///     Creates a new AABB from the given box.
    /// </summary>
    public AABB(Box3 box) : this()
    {
        MinX = box.Min.X;
        MinY = box.Min.Y;
        MinZ = box.Min.Z;
        MaxX = box.Max.X;
        MaxY = box.Max.Y;
        MaxZ = box.Max.Z;
    }

    /// <summary>
    ///     Gets the AABB as a <see cref="Box3" />.
    /// </summary>
    public Box3 ToBox3()
    {
        return new Box3(new Vector3(MinX, MinY, MinZ), new Vector3(MaxX, MaxY, MaxZ));
    }

    /// <summary>
    ///     The minimum x value.
    /// </summary>
    private readonly float MinX;

    /// <summary>
    ///     The minimum y value.
    /// </summary>
    private readonly float MinY;

    /// <summary>
    ///     The minimum z value.
    /// </summary>
    private readonly float MinZ;

    /// <summary>
    ///     The maximum x value.
    /// </summary>
    private readonly float MaxX;

    /// <summary>
    ///     The maximum y value.
    /// </summary>
    private readonly float MaxY;

    /// <summary>
    ///     The maximum z value.
    /// </summary>
    private readonly float MaxZ;
}

/// <summary>
///     The data associated with an AABB.
///     See the wiki for more information.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct BoundsData
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BoundsData" /> struct, given a tuple of data.
    /// </summary>
    /// <param name="data">The data tuple.</param>
    public BoundsData(ref (uint a, uint b, uint c, uint d) data) : this()
    {
        A = data.a;
        B = data.b;
        C = data.c;
        D = data.d;
    }

    /// <summary>
    ///     Converts the data to a tuple.
    /// </summary>
    public (uint a, uint b, uint c, uint d) ToTuple()
    {
        return (A, B, C, D);
    }

    /// <summary>
    ///     The first component of the data.
    /// </summary>
    private readonly uint A;

    /// <summary>
    ///     The second component of the data.
    /// </summary>
    private readonly uint B;

    /// <summary>
    ///     The third component of the data.
    /// </summary>
    private readonly uint C;

    /// <summary>
    ///     The fourth component of the data.
    /// </summary>
    private readonly uint D;
}

/// <summary>
///     An AABB with additional data.
///     This is used by meshes that use custom ray intersections.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpatialBounds
{
    /// <summary>
    ///     The AABB.
    /// </summary>
    public AABB AABB;

    /// <summary>
    ///     Additional data for the bounds.
    /// </summary>
    public BoundsData Data;
}
