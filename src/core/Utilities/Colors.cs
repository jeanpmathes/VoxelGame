// <copyright file="Colors.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Drawing;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Extension methods and other utilities for colors.
/// </summary>
public static class Colors
{
    /// <summary>
    ///     Check if a given color has any opaqueness, meaning the alpha channel is not 0.
    /// </summary>
    public static bool HasOpaqueness(this Color color)
    {
        return color.A != 0;
    }
}
