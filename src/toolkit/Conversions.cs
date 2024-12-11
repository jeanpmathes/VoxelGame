// <copyright file="Conversions.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Toolkit;

/// <summary>
/// Helpful conversion methods.
/// </summary>
public static class Conversions
{
    /// <summary>
    ///     Convert a bool to an int.
    /// </summary>
    public static Int32 ToInt(this Boolean b)
    {
        return b ? 1 : 0;
    }

    /// <summary>
    ///     Convert a bool to an uint.
    /// </summary>
    public static UInt32 ToUInt(this Boolean b)
    {
        return b ? 1u : 0u;
    }
}
