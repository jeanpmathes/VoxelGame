// <copyright file="VectorMarshallers.cs" company="VoxelGame">
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
using JetBrains.Annotations;
using OpenTK.Mathematics;

namespace VoxelGame.Graphics.Interop;

/// <summary>
///     Marshaller for <see cref="Vector3" />.
/// </summary>
[CustomMarshaller(typeof(Vector3), MarshalMode.ManagedToUnmanagedIn, typeof(Vector3Marshaller))]
public static class Vector3Marshaller
{
    /// <summary>
    ///     Convert a managed <see cref="Vector3" /> to an unmanaged <see cref="Unmanaged" />.
    /// </summary>
    public static Unmanaged ConvertToUnmanaged(Vector3 managed)
    {
        return new Unmanaged
        {
            x = managed.X,
            y = managed.Y,
            z = managed.Z
        };
    }

    /// <summary>
    ///     Free the unmanaged <see cref="Unmanaged" />.
    /// </summary>
    public static void Free(Unmanaged unmanaged)
    {
        // Nothing to do here.
    }

    /// <summary>
    ///     The unmanaged representation of <see cref="Vector3" />.
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public ref struct Unmanaged
    {
        internal Single x;
        internal Single y;
        internal Single z;
    }
}

/// <summary>
///     Marshaller for <see cref="Vector4" />.
/// </summary>
[CustomMarshaller(typeof(Vector4), MarshalMode.ManagedToUnmanagedIn, typeof(Vector4Marshaller))]
public static class Vector4Marshaller
{
    /// <summary>
    ///     Convert a managed <see cref="Vector4" /> to an unmanaged <see cref="Unmanaged" />.
    /// </summary>
    public static Unmanaged ConvertToUnmanaged(Vector4 managed)
    {
        return new Unmanaged
        {
            x = managed.X,
            y = managed.Y,
            z = managed.Z,
            w = managed.W
        };
    }

    /// <summary>
    ///     Free the unmanaged <see cref="Unmanaged" />.
    /// </summary>
    public static void Free(Unmanaged unmanaged)
    {
        // Nothing to do here.
    }

    /// <summary>
    ///     The unmanaged representation of <see cref="Vector4" />.
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public ref struct Unmanaged
    {
        internal Single x;
        internal Single y;
        internal Single z;
        internal Single w;
    }
}
