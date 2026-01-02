// <copyright file="SpatialData.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace VoxelGame.Graphics.Data;

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
    public UInt32 Data;
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
    private readonly Single MinX;

    /// <summary>
    ///     The minimum y value.
    /// </summary>
    private readonly Single MinY;

    /// <summary>
    ///     The minimum z value.
    /// </summary>
    private readonly Single MinZ;

    /// <summary>
    ///     The maximum x value.
    /// </summary>
    private readonly Single MaxX;

    /// <summary>
    ///     The maximum y value.
    /// </summary>
    private readonly Single MaxY;

    /// <summary>
    ///     The maximum z value.
    /// </summary>
    private readonly Single MaxZ;
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
    public BoundsData(ref (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data) : this()
    {
        A = data.a;
        B = data.b;
        C = data.c;
        D = data.d;
    }

    /// <summary>
    ///     Converts the data to a tuple.
    /// </summary>
    public (UInt32 a, UInt32 b, UInt32 c, UInt32 d) ToTuple()
    {
        return (A, B, C, D);
    }

    /// <summary>
    ///     The first component of the data.
    /// </summary>
    private readonly UInt32 A;

    /// <summary>
    ///     The second component of the data.
    /// </summary>
    private readonly UInt32 B;

    /// <summary>
    ///     The third component of the data.
    /// </summary>
    private readonly UInt32 C;

    /// <summary>
    ///     The fourth component of the data.
    /// </summary>
    private readonly UInt32 D;
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
