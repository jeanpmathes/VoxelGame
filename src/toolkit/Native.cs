// <copyright file="Native.cs" company="VoxelGame">
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
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using VoxelGame.Toolkit.Noise;

namespace VoxelGame.Toolkit;

/// <summary>
///     Utility methods for calling some of the native methods easily.
/// </summary>
#pragma warning disable S3242 // The specific types are matched on the native side.
#pragma warning disable S1200 // This class intentionally contains all native functions.
internal static class Native
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IntPtr CreateNoise(NoiseDefinition definition)
    {
        return NativeMethods.CreateNoise(definition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Single GetNoise2D(IntPtr noise, Vector2d position)
    {
        return NativeMethods.GetNoise2D(noise, (Single) position.X, (Single) position.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Single GetNoise3D(IntPtr noise, Vector3d position)
    {
        return NativeMethods.GetNoise3D(noise, (Single) position.X, (Single) position.Y, (Single) position.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe void GetNoiseGrid2D(IntPtr noise, Vector2i position, Vector2i size, Span<Single> data)
    {
        fixed (Single* ptr = data)
        {
            NativeMethods.GetNoiseGrid2D(noise, position.X, position.Y, size.X, size.Y, ptr);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe void GetNoiseGrid3D(IntPtr noise, Vector3i position, Vector3i size, Span<Single> data)
    {
        fixed (Single* ptr = data)
        {
            NativeMethods.GetNoiseGrid3D(noise, position.X, position.Y, position.Z, size.X, size.Y, size.Z, ptr);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DeleteNoise(IntPtr noise)
    {
        NativeMethods.DeleteNoise(noise);
    }
}
