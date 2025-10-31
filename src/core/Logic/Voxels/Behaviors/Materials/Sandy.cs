// <copyright file = "Sandy.cs" company = "VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Materials;

/// <summary>
/// Marks a block as being sandy, meaning it is made up of loose sand particles.
/// </summary>
public partial class Sandy : BlockBehavior, IBehavior<Sandy, BlockBehavior, Block>
{
    [Constructible]
    private Sandy(Block subject) : base(subject)
    {
        subject.Require<Loose>();
    }
}
