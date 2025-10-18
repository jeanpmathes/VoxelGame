// <copyright file="Unmeshed.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;

/// <summary>
///     Corresponds to <see cref="Meshable.Simple" />
/// </summary>
public partial class Unmeshed : BlockBehavior, IBehavior<Unmeshed, BlockBehavior, Block>, IMeshable
{
    [Constructible]
    private Unmeshed(Block subject) : base(subject) {}

    /// <inheritdoc />
    public Meshable Type => Meshable.Unmeshed;

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        properties.IsOpaque.ContributeConstant(value: false);
    }
}
