// <copyright file="FourWayRotatableModelled.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Orienting;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Visuals;

/// <summary>
/// Rotates the models used by a <see cref="Modelled"/> block and changes the selection to match the rotation.
/// </summary>
public class FourWayRotatableModelled : BlockBehavior, IBehavior<FourWayRotatableModelled, BlockBehavior, Block>
// todo: make this class just RotatableModelled when the new API on BlockModel to get sided models is done
{
    private readonly FourWayRotatable rotatable;
    
    private FourWayRotatableModelled(Block subject) : base(subject)
    {
        OrientationOverride = Aspect<Utilities.Orientation, State>.New<Exclusive<Utilities.Orientation, State>>(nameof(OrientationOverride), this);
        
        rotatable = subject.Require<FourWayRotatable>();
        
        subject.Require<Modelled>().Model.ContributeFunction(GetModel);
    }

    /// <inheritdoc />
    public static FourWayRotatableModelled Construct(Block input)
    {
        return new FourWayRotatableModelled(input);
    }
    
    /// <summary>
    /// Used to override the orientation as provided by the <see cref="FourWayRotatable"/> behavior.
    /// </summary>
    public Aspect<Utilities.Orientation, State> OrientationOverride { get; }
    
    private BlockModel GetModel(BlockModel original, State state)
    {
        // todo: use a new method on BlockModel to get just the needed model 
        (BlockModel north, BlockModel east, BlockModel south, BlockModel west) models = original.CreateAllOrientations(rotateTopAndBottomTexture: true);

        Utilities.Orientation orientation = rotatable.GetOrientation(state);
        
        orientation = OrientationOverride.GetValue(orientation, state);
        
        return orientation switch
        {
            Utilities.Orientation.North => models.north,
            Utilities.Orientation.East => models.east,
            Utilities.Orientation.South => models.south,
            Utilities.Orientation.West => models.west,
            _ => throw Exceptions.UnsupportedEnumValue(orientation)
        };
    }
}
