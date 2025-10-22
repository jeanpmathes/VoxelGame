// <copyright file="StraightPipe.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

/// <summary>
///     A variant of a <see cref="Pipe" /> that only connects to other pipes in a straight line.
/// </summary>
public partial class StraightPipe : BlockBehavior, IBehavior<StraightPipe, BlockBehavior, Block>
{
    private readonly Piped piped;
    private readonly AxisRotatable rotation;

    [Constructible]
    private StraightPipe(Block subject) : base(subject)
    {
        rotation = subject.Require<AxisRotatable>();
        piped = subject.Require<Piped>();

        piped.IsConnectionAllowed.ContributeFunction(GetIsConnectionAllowed);

        subject.Require<Pipe>().OpenSides.ContributeFunction(GetOpenSides);

        subject.Require<Modelled>().Model.ContributeFunction(GetModel);
        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);

        subject.PlacementState.ContributeFunction(GetPlacementState);
    }

    private Boolean GetIsConnectionAllowed(Boolean original, (State state, Side side) context)
    {
        (State state, Side side) = context;

        return rotation.GetAxis(state) == side.Axis();
    }

    private Sides GetOpenSides(Sides original, State state)
    {
        return rotation.GetAxis(state).Sides();
    }

    private Model GetModel(Model original, State state)
    {
        Axis axis = rotation.GetAxis(state);

        return original.CreateModelForAxis(axis, Model.TransformationMode.Reshape);
    }

    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        Double diameter = Piped.GetPipeDiameter(piped.Tier.Get());

        Axis axis = rotation.GetAxis(state);

        return new BoundingVolume(new Vector3d(x: 0.5, y: 0.5, z: 0.5), axis.Vector3(onAxis: 0.5, diameter));
    }

    private State GetPlacementState(State original, (World world, Vector3i position, Actor? actor) context)
    {
        (World _, Vector3i _, Actor? actor) = context;

        return rotation.SetAxis(original, (actor?.GetTargetedSide() ?? Side.Front).Axis());
    }
}
