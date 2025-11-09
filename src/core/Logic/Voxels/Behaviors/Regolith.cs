// <copyright file="Regolith.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;

namespace VoxelGame.Core.Logic.Voxels.Behaviors;

/// <summary>
/// Regolith blocks make up the loose material of the terrain.
/// World generation can safely replace regolith blocks when placing structures and decorations.
/// </summary>
public partial class Regolith : BlockBehavior, IBehavior<Regolith, BlockBehavior, Block>
{
    [Constructible]
    private Regolith(Block subject) : base(subject)
    {
        
    }
}
