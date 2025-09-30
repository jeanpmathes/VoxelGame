// <copyright file="LateralRotatableModelled.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Orienting;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Visuals;

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

    private BlockModel GetModel(BlockModel original, State state)
    {
        // todo: use a new method on BlockModel to get just the needed model 
        (BlockModel north, BlockModel east, BlockModel south, BlockModel west) models = original.CreateAllOrientations(rotateTopAndBottomTexture: true);

        Orientation orientation = rotatable.GetOrientation(state);

        orientation = OrientationOverride.GetValue(orientation, state);

        return orientation switch
        {
            Orientation.North => models.north,
            Orientation.East => models.east,
            Orientation.South => models.south,
            Orientation.West => models.west,
            _ => throw Exceptions.UnsupportedEnumValue(orientation)
        };
    }
}
