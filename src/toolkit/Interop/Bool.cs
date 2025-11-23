// <copyright file = "Bool.cs" company = "VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime.InteropServices;

namespace VoxelGame.Toolkit.Interop;

/// <summary>
/// A 32-bit boolean value for native interop.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Bool
{
    private UInt32 value;
    
    /// <summary>
    /// Converts a <see cref="Boolean"/> to a <see cref="Bool"/>.
    /// </summary>
    public static implicit operator Bool(Boolean b) => new() { value = b.ToUInt() };
    
    /// <summary>
    /// Converts a <see cref="Bool"/> to a <see cref="Boolean"/>.
    /// </summary>
    public static implicit operator Boolean(Bool b) => b.value != 0;
}
