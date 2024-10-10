// <copyright file="AreaCategory.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Generation.Dungeons;

/// <summary>
///     The category of an area.
/// </summary>
public enum AreaCategory
{
    /// <summary>
    ///     The starting area.
    /// </summary>
    Start,

    /// <summary>
    ///     An area just with corridors, connecting other areas.
    /// </summary>
    Corridor,

    /// <summary>
    ///     A generic area.
    /// </summary>
    Generic,

    /// <summary>
    ///     The end area.
    /// </summary>
    End
}
