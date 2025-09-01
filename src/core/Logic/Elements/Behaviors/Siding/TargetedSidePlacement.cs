// <copyright file="TargetedSidePlacement.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Siding;

/// <summary>
/// Places sided blocks based on the targeted side of the placing actor.
/// </summary>
public class TargetedSidePlacement : BlockBehavior, IBehavior<TargetedSidePlacement, BlockBehavior, Block>
{
    private readonly Sided siding;
    
    private TargetedSidePlacement(Block subject) : base(subject)
    {
        siding = subject.Require<Sided>();
        
        subject.PlacementState.ContributeFunction(GetPlacementState);
    }

    /// <inheritdoc />
    public static TargetedSidePlacement Construct(Block input)
    {
        return new TargetedSidePlacement(input);
    }
    
    private State GetPlacementState(State original, (World world, Vector3i position, Actor? actor) context)
    {
        (World _, Vector3i _, Actor? actor) = context;
        
        Side? side = actor?.GetTargetedSide()?.Opposite();
        
        if (side == null) return original;

        return siding.SetSides(original, side.Value.ToFlag()) ?? original;
    }
}
