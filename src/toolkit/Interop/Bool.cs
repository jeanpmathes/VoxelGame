// <copyright file="Bool.cs" company="VoxelGame">
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
