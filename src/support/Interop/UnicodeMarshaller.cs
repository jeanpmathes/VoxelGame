// <copyright file="UnicodeMarshaller.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices;

namespace VoxelGame.Support.Interop;

/// <summary>
///     Helper for marshalling strings to unmanaged code.
/// </summary>
#pragma warning disable S1694
public abstract class UnicodeStringMarshaller : IMarshaller<string?, IntPtr>
#pragma warning restore S1694
{
    /// <summary>
    ///     Convert a managed string to an unmanaged string.
    /// </summary>
    /// <param name="managed">The managed string to convert.</param>
    /// <returns>The unmanaged string.</returns>
    public static IntPtr ConvertToUnmanaged(string? managed)
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
