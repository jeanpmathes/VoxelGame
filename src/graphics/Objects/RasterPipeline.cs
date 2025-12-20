// <copyright file="RasterPipeline.cs" company="VoxelGame">
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
using VoxelGame.Graphics.Core;

namespace VoxelGame.Graphics.Objects;

/// <summary>
///     A pipeline for raster-based rendering.
/// </summary>
[NativeMarshalling(typeof(RasterPipelineMarshaller))]
public class RasterPipeline : NativeObject
{
    /// <summary>
    ///     Creates a new <see cref="RasterPipeline" />.
    /// </summary>
    public RasterPipeline(IntPtr nativePointer, Client client) : base(nativePointer, client) {}
}

#pragma warning disable S3242
[CustomMarshaller(typeof(RasterPipeline), MarshalMode.ManagedToUnmanagedIn, typeof(RasterPipelineMarshaller))]
internal static class RasterPipelineMarshaller
{
    internal static IntPtr ConvertToUnmanaged(RasterPipeline managed)
    {
        return managed.Self;
    }

    internal static void Free(IntPtr unmanaged)
    {
        // Nothing to do here.
    }
}
#pragma warning restore S3242
