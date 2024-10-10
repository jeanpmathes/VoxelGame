// <copyright file="Area.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Utilities;

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

    /// <summary>
    ///     Get the sides of the area that are connected to other areas.
    /// </summary>
    public BlockSides Connections { get; private set; }

    /// <summary>
    ///     Add a connection to the area.
    /// </summary>
    /// <param name="orientation">The orientation of the connection.</param>
    public void Connect(Orientation orientation)
    {
        Connections |= orientation.ToBlockSide().ToFlag();
    }
}
