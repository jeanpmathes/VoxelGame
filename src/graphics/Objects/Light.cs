// <copyright file="Light.cs" company="VoxelGame">
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
using System.Runtime.InteropServices.Marshalling;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Core;

namespace VoxelGame.Graphics.Objects;

/// <summary>
///     A directional light. The position is ignored.
/// </summary>
[NativeMarshalling(typeof(LightMarshaller))]
public class Light : Spatial
{
    private Vector3d direction = Vector3d.Zero;
    private Vector4 color = Vector4.One;
    private Single intensity = 1.0f;
    private Boolean dirty = true;

    /// <summary>
    ///     Create a new light.
    /// </summary>
    public Light(IntPtr nativePointer, Space space) : base(nativePointer, space) {}

    /// <summary>
    ///     Get or set the light direction. This is the direction in which the light is emitted.
    /// </summary>
    public Vector3d Direction
    {
        get => direction;
        set
        {
            if (value == direction) return;

            direction = value;
            dirty = true;
        }
    }

    /// <summary>
    ///     Get or set the light intensity.
    /// </summary>
    public Single Intensity
    {
        get => intensity;
        set
        {
            if (MathTools.NearlyEqual(value, intensity)) return;

            intensity = value;
            dirty = true;
        }
    }

    /// <summary>
    ///     Get or set the color of the light.
    /// </summary>
    public ColorS Color
    {
        get => ColorS.FromVector4(color);
        set
        {
            if (value.ToVector4() == color) return;

            color = value.ToVector4();
            dirty = true;
        }
    }

    /// <inheritdoc />
    internal override void Synchronize()
    {
        base.Synchronize();

        if (!dirty) return;

        dirty = false;

        NativeMethods.SetLightConfiguration(this, (Vector3) direction, color.Xyz, intensity);
    }
}

#pragma warning disable S3242
[CustomMarshaller(typeof(Light), MarshalMode.ManagedToUnmanagedIn, typeof(LightMarshaller))]
internal static class LightMarshaller
{
    internal static IntPtr ConvertToUnmanaged(Light managed)
    {
        return managed.Self;
    }

    internal static void Free(IntPtr unmanaged)
    {
        // Nothing to do here.
    }
}
#pragma warning restore S3242
