// <copyright file="LateralRotatableModelled.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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
public class LateralRotatableModelled : BlockBehavior, IBehavior<LateralRotatableModelled, BlockBehavior, Block>
// todo: make this class just RotatableModelled when the new API on BlockModel to get sided models is done
{
    private readonly LateralRotatable rotatable;

    private LateralRotatableModelled(Block subject) : base(subject)
    {
        OrientationOverride = Aspect<Orientation, State>.New<Exclusive<Orientation, State>>(nameof(OrientationOverride), this);

        rotatable = subject.Require<LateralRotatable>();

        subject.Require<Modelled>().Model.ContributeFunction(GetModel);
    }

    /// <summary>
    ///     Used to override the orientation as provided by the <see cref="LateralRotatable" /> behavior.
    /// </summary>
    public Aspect<Orientation, State> OrientationOverride { get; }

    /// <inheritdoc />
    public static LateralRotatableModelled Construct(Block input)
    {
        return new LateralRotatableModelled(input);
    }

    private Model GetModel(Model original, State state)
    {
        Orientation orientation = rotatable.GetOrientation(state);

        orientation = OrientationOverride.GetValue(orientation, state);

        return original.CreateModelForOrientation(orientation);
    }
}
