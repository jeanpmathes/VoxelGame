// <copyright file="WallDecorator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Default.Decorations;

/// <summary>
///     Selects empty positions that have a solid block next to them.
/// </summary>
public class WallDecorator : Decorator
{
    /// <inheritdoc />
    public override Boolean CanPlace(Vector3i position, in Decoration.PlacementContext context, IReadOnlyGrid grid)
    {
        Content? content = grid.GetContent(position);

        if (content is not {IsEmpty: true}) return false;

        foreach (Orientation orientation in Orientations.All)
        {
            Content? neighbor = grid.GetContent(orientation.Offset(position));

            if (neighbor?.Block.IsFullySolid == true) return true;
        }

        return false;
    }
}
