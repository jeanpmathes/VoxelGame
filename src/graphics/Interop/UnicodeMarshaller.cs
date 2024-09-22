// <copyright file="UnicodeMarshaller.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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
