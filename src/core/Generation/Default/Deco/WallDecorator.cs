// <copyright file="WallDecorator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Default.Deco;

/// <summary>
///     Selects empty positions that have a solid block next to them.
/// </summary>
public class WallDecorator : Decorator
{
    /// <inheritdoc />
    public override bool CanPlace(Vector3i position, in Decoration.PlacementContext context, IReadOnlyGrid grid)
    {
        Content? content = grid.GetContent(position);

        if (content is not {IsEmpty: true}) return false;

        foreach (Orientation orientation in Orientations.All)
        {
            Content? neighbor = grid.GetContent(orientation.Offset(position));

            if (neighbor?.Block.IsSolidAndFull == true) return true;
        }

        return false;
    }
}
