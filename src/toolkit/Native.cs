//  <copyright file="Native.cs" company="VoxelGame">
//      MIT License
// 	 For full license see the repository.
//  </copyright>
//  <author>jeanpmathes</author>

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
