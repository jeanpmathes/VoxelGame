// <copyright file="AreaDeck.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;

namespace VoxelGame.Core.Generation.Dungeons;

/// <summary>
///     A deck of areas.
/// </summary>
public class AreaDeck : ElementDeck<Area>
{
    /// <inheritdoc />
    public AreaDeck(IEnumerable<Area> elements) : base(elements) {}
}
