// <copyright file="Area.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Generation.Dungeons;

/// <summary>
///     An area is a larger region of a dungeon, each containing a central Point of Interest and several rooms.
/// </summary>
public class Area
{
    /// <summary>
    ///     Create a new area.
    /// </summary>
    /// <param name="category">The category of the area.</param>
    public Area(AreaCategory category = AreaCategory.Generic)
    {
        Category = category;
    }

    /// <summary>
    ///     Get the category of the area.
    /// </summary>
    public AreaCategory Category { get; }
}
