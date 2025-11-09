// <copyright file="TargetedSidePlacement.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Siding;

/// <summary>
///     Places sided blocks based on the targeted side of the placing actor.
/// </summary>
public partial class TargetedSidePlacement : BlockBehavior, IBehavior<TargetedSidePlacement, BlockBehavior, Block>
{
    private readonly Sided siding;

    [Constructible]
    private TargetedSidePlacement(Block subject) : base(subject)
    {
        siding = subject.Require<Sided>();

        subject.PlacementState.ContributeFunction(GetPlacementState);
    }

    private State GetPlacementState(State original, (World world, Vector3i position, Actor? actor) context)
    {
        (World _, Vector3i _, Actor? actor) = context;

        Side? side = actor?.GetTargetedSide()?.Opposite();

        if (side == null) return original;

        return siding.SetSides(original, side.Value.ToFlag()) ?? original;
    }
}
