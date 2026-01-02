// <copyright file="NativeMethods.cs" company="VoxelGame">
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
using VoxelGame.Toolkit.Noise;

namespace VoxelGame.Toolkit;

internal static partial class NativeMethods
{
    private const String DllFilePath = @".\NativeToolkit.dll";

    [LibraryImport(DllFilePath, EntryPoint = "NativeCreateAllocator")]
    internal static partial IntPtr CreateAllocator();

    [LibraryImport(DllFilePath, EntryPoint = "NativeAllocate")]
    internal static partial IntPtr Allocate(IntPtr allocator, UInt64 size);

    [LibraryImport(DllFilePath, EntryPoint = "NativeDeallocate")]
    internal static partial Int32 Deallocate(IntPtr allocator, IntPtr ptr);

    [LibraryImport(DllFilePath, EntryPoint = "NativeDeleteAllocator")]
    internal static partial void DeleteAllocator(IntPtr allocator);

    [LibraryImport(DllFilePath, EntryPoint = "NativeCreateNoise")]
    internal static partial IntPtr CreateNoise(NoiseDefinition definition);

    [LibraryImport(DllFilePath, EntryPoint = "NativeGetNoise2D")]
    internal static partial Single GetNoise2D(IntPtr noise, Single x, Single y);

    [LibraryImport(DllFilePath, EntryPoint = "NativeGetNoise3D")]
    internal static partial Single GetNoise3D(IntPtr noise, Single x, Single y, Single z);

    [LibraryImport(DllFilePath, EntryPoint = "NativeGetNoiseGrid2D")]
    internal static unsafe partial void GetNoiseGrid2D(IntPtr noise, Int32 x, Int32 y, Int32 width, Int32 height, Single* data);

    [LibraryImport(DllFilePath, EntryPoint = "NativeGetNoiseGrid3D")]
    internal static unsafe partial void GetNoiseGrid3D(IntPtr noise, Int32 x, Int32 y, Int32 z, Int32 width, Int32 height, Int32 depth, Single* data);

    [LibraryImport(DllFilePath, EntryPoint = "NativeDeleteNoise")]
    internal static partial void DeleteNoise(IntPtr noise);
}
