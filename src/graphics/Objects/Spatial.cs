// <copyright file="Spatial.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
using System.Runtime.InteropServices.Marshalling;
using OpenTK.Mathematics;
using VoxelGame.Graphics.Core;

namespace VoxelGame.Graphics.Objects;

/// <summary>
///     A native object that can be part of a space.
/// </summary>
[NativeMarshalling(typeof(SpatialMarshaller))]
public class Spatial : NativeObject
{
    private Boolean dirty = true;

    private Vector3d position = Vector3d.Zero;
    private Quaterniond rotation = Quaterniond.Identity;

    /// <summary>
    ///     Create a new native spatial object.
    /// </summary>
    /// <param name="nativePointer">The native pointer.</param>
    /// <param name="space">The space in which the object is.</param>
    protected Spatial(IntPtr nativePointer, Space space) : base(nativePointer, space.Client)
    {
        Space = space;
    }

    /// <summary>
    ///     Get the space in which the object is.
    /// </summary>
    private Space Space { get; }

    /// <summary>
    ///     Get or set the spatial object position.
    /// </summary>
    public Vector3d Position
    {
        get => position;
        set
        {
            position = value;
            dirty = true;
        }
    }

    /// <summary>
    ///     Get or set the spatial object rotation.
    /// </summary>
    public Quaterniond Rotation
    {
        get => rotation;
        set
        {
            rotation = value;
            dirty = true;
        }
    }

    internal override void Synchronize()
    {
        if (dirty || Space.HasAdjustmentChanged) NativeMethods.UpdateSpatialData(this, Space.GetAdjustedData(this));

        dirty = false;
    }
}

#pragma warning disable S3242
[CustomMarshaller(typeof(Spatial), MarshalMode.ManagedToUnmanagedIn, typeof(SpatialMarshaller))]
internal static class SpatialMarshaller
{
    internal static IntPtr ConvertToUnmanaged(Spatial managed)
    {
        return managed.Self;
    }

    internal static void Free(IntPtr unmanaged)
    {
        // Nothing to do here.
    }
}
#pragma warning restore S3242
