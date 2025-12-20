// <copyright file="ShaderBuffer.cs" company="VoxelGame">
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
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;
using VoxelGame.Graphics.Core;
using VoxelGame.Toolkit.Utilities.Constants;

namespace VoxelGame.Graphics.Objects;

/// <summary>
///     Common constants for shader buffers.
/// </summary>
public static class ShaderBuffers
{
    /// <summary>
    ///     The recommended packing for data structs.
    /// </summary>
    public const Int32 Pack = 4;

    /// <summary>
    ///     The maximum field offset between two fields.
    ///     Use this when padding would be required.
    /// </summary>
    public const Int32 FieldOffset = 4 * sizeof(Single);
}

/// <summary>
///     Base class for shader buffers.
/// </summary>
[NativeMarshalling(typeof(ShaderBufferMarshaller))]
public class ShaderBuffer : NativeObject
{
    /// <summary>
    ///     Creates a new <see cref="ShaderBuffer" />.
    /// </summary>
    protected ShaderBuffer(IntPtr nativePointer, Client client) : base(nativePointer, client) {}
}

/// <summary>
///     Represents a shader constant buffer.
/// </summary>
public class ShaderBuffer<T> : ShaderBuffer where T : unmanaged, IEquatable<T>, IDefault<T>
{
    /// <summary>
    ///     Delegate for modifying the data of the buffer.
    /// </summary>
    public delegate void ModifyDelegate(ref T data);

    private T data = T.Default;
    private Boolean dirty = true;

    /// <summary>
    ///     Creates a new <see cref="ShaderBuffer{T}" />.
    /// </summary>
    public ShaderBuffer(IntPtr nativePointer, Client client) : base(nativePointer, client) {}

    /// <summary>
    ///     Get or set the data of the buffer.
    /// </summary>
    public T Data
    {
        get => data;
        set
        {
            if (EqualityComparer<T>.Default.Equals(Data, value)) return;

            data = value;

            if (Client.IsOutOfCycle) Write();
            else dirty = true;
        }
    }

    /// <summary>
    ///     Modifies the data of the buffer.
    /// </summary>
    /// <param name="modifier">The modifier.</param>
    public void Modify(ModifyDelegate modifier)
    {
        T copy = data;
        modifier(ref copy);
        Data = copy;
    }

    internal override void Synchronize()
    {
        if (!dirty) return;

        Write();

        dirty = false;
    }

    private void Write()
    {
        unsafe
        {
            fixed (T* ptr = &data)
            {
                NativeMethods.SetShaderBufferData(this, ptr);
            }
        }
    }
}

#pragma warning disable S3242
[CustomMarshaller(typeof(ShaderBuffer), MarshalMode.ManagedToUnmanagedIn, typeof(ShaderBufferMarshaller))]
internal static class ShaderBufferMarshaller
{
    internal static IntPtr ConvertToUnmanaged(ShaderBuffer managed)
    {
        return managed.Self;
    }

    internal static void Free(IntPtr unmanaged)
    {
        // Nothing to do here.
    }
}
#pragma warning restore S3242
