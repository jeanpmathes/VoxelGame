// <copyright file="UnicodeMarshaller.cs" company="VoxelGame">
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
using VoxelGame.Toolkit.Interop;

namespace VoxelGame.Graphics.Interop;

/// <summary>
///     Helper for marshalling strings to unmanaged code.
/// </summary>
public abstract class UnicodeStringMarshaller : IMarshaller<String?, IntPtr>
{
    /// <summary>
    ///     Convert a managed string to an unmanaged string.
    /// </summary>
    /// <param name="managed">The managed string to convert.</param>
    /// <returns>The unmanaged string.</returns>
    public static IntPtr ConvertToUnmanaged(String? managed)
    {
        return Marshal.StringToHGlobalUni(managed);
    }

    /// <summary>
    ///     Free the unmanaged string.
    /// </summary>
    /// <param name="unmanaged">The unmanaged string to free.</param>
    public static void Free(IntPtr unmanaged)
    {
        Marshal.FreeHGlobal(unmanaged);
    }
}
