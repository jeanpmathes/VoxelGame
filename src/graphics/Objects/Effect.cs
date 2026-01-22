// <copyright file="Effect.cs" company="VoxelGame">
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
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Data;

namespace VoxelGame.Graphics.Objects;

/// <summary>
///     An effect is a object positioned in 3D space that is rendered with a raster pipeline.
/// </summary>
[NativeMarshalling(typeof(EffectMarshaller))]
public class Effect : Drawable
{
    /// <summary>
    ///     Wrap a native mesh and drawable pointer.
    /// </summary>
    public Effect(IntPtr nativePointer, Space space) : base(nativePointer, space) {}

    /// <summary>
    ///     Set the new vertices for this effect.
    /// </summary>
    /// <param name="vertices">The new vertices.</param>
    public void SetNewVertices(Span<EffectVertex> vertices)
    {
        Native.SetEffectVertices(this, vertices);
    }
}

#pragma warning disable S3242
[CustomMarshaller(typeof(Effect), MarshalMode.ManagedToUnmanagedIn, typeof(EffectMarshaller))]
internal static class EffectMarshaller
{
    internal static IntPtr ConvertToUnmanaged(Effect managed)
    {
        return managed.Self;
    }

    internal static void Free(IntPtr unmanaged)
    {
        // Nothing to do here.
    }
}
#pragma warning restore S3242
