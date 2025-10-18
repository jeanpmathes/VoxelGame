// <copyright file="DirectionalSidePlacement.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Annotations;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Siding;

/// <summary>
///     Places sided blocks based on the direction the placing actor is facing.
/// </summary>
public partial class DirectionalSidePlacement : BlockBehavior, IBehavior<DirectionalSidePlacement, BlockBehavior, Block>
{
    private readonly Sided siding;

    [Constructible]
    private DirectionalSidePlacement(Block subject) : base(subject)
    {
        siding = subject.Require<Sided>();

        subject.PlacementState.ContributeFunction(GetPlacementState);
    }

    private State GetPlacementState(State original, (World world, Vector3i position, Actor? actor) context)
    {
        (World _, Vector3i _, Actor? actor) = context;

        var orientation = actor?.Head?.Forward.ToOrientation();

        if (orientation == null) return original;

        return siding.SetSides(original, orientation.Value.ToSide().Opposite().ToFlag()) ?? original;
    }
}
