// <copyright file="Void.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Toolkit.Utilities;

/// <summary>
///     A unit type placeholder.
/// </summary>
public class Void
{
    private Void() {}

    /// <summary>
    ///     Get the singleton instance of the unit type placeholder.
    /// </summary>
    public static Void Instance { get; } = new();
}
