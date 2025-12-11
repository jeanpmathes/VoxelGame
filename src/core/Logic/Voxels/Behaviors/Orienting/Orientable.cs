// <copyright file="Orientable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;

/// <summary>
///     Blocks that can be oriented in some way, e.g. by rotation or siding.
/// </summary>
public partial class Orientable : BlockBehavior, IBehavior<Orientable, BlockBehavior, Block>
{
    [Constructible]
    private Orientable(Block subject) : base(subject) {}
}
