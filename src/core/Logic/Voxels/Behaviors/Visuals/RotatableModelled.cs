// <copyright file="RotatableModelled.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     Rotates the models used by a <see cref="Modelled" /> block and changes the selection to match the rotation.
/// </summary>
public class RotatableModelled : BlockBehavior, IBehavior<RotatableModelled, BlockBehavior, Block>
{
    private readonly Rotatable rotatable;

    private RotatableModelled(Block subject) : base(subject)
    {
        RotationOverride = Aspect<(Axis axis, Int32 turns), State>
            .New<Exclusive<(Axis axis, Int32 turns), State>>(nameof(RotationOverride), this);

        rotatable = subject.Require<Rotatable>();

        subject.Require<Modelled>().Model.ContributeFunction(GetModel);
    }

    /// <summary>
    ///     Used to override the rotation as provided by the <see cref="Rotatable" /> behavior.
    /// </summary>
    public Aspect<(Axis axis, Int32 turns), State> RotationOverride { get; }

    /// <inheritdoc />
    public static RotatableModelled Construct(Block input)
    {
        return new RotatableModelled(input);
    }

    private Model GetModel(Model original, State state)
    {
        Axis axis = rotatable.GetCurrentAxis(state);
        Int32 turns = rotatable.GetCurrentTurns(state);

        (axis, turns) = RotationOverride.GetValue((axis, turns), state);

        return original.CreateModelFor(axis, turns);
    }
}
