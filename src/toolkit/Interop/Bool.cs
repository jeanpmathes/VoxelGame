// <copyright file = "Bool.cs" company = "VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime.InteropServices;
using VoxelGame.Annotations.Attributes;

namespace VoxelGame.Toolkit.Interop;

/// <summary>
///     A 32-bit boolean value for native interop.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[ValueSemantics]
public partial struct Bool
{
    private UInt32 value;

    /// <summary>
    ///     Converts a <see cref="Boolean" /> to a <see cref="Bool" />.
    /// </summary>
    public static implicit operator Bool(Boolean b)
    {
        return new Bool {value = b.ToUInt()};
    }

    /// <summary>
    ///     Converts a <see cref="Bool" /> to a <see cref="Boolean" />.
    /// </summary>
    public static implicit operator Boolean(Bool b)
    {
        return b.value != 0;
    }
}
