// <copyright file="NativeMethods.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices;
using VoxelGame.Toolkit.Noise;

namespace VoxelGame.Toolkit;

internal static partial class NativeMethods
{
    private const String DllFilePath = @".\NativeToolkit.dll";

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
