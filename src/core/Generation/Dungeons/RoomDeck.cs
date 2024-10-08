// <copyright file="RoomDeck.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;

namespace VoxelGame.Core.Generation.Dungeons;

/// <summary>
///     A deck of rooms that can be used to generate a part of a dungeon.
/// </summary>
public class RoomDeck : ElementDeck<Room>
{
    /// <inheritdoc />
    public RoomDeck(IEnumerable<Room> elements) : base(elements) {}
}
