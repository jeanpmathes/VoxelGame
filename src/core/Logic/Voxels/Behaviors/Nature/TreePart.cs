// <copyright file="TreePart.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature;

/// <summary>
/// Marks a block as part of a tree.
/// </summary>
public partial class TreePart : BlockBehavior, IBehavior<TreePart, BlockBehavior, Block>
{
    [Constructible]
    private TreePart(Block subject) : base(subject)
    {
        
    }
}
