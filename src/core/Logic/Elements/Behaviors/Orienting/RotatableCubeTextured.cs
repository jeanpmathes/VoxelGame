// <copyright file="RotatableCubeTextured.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Visuals;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Orienting;

/// <summary>
/// Glue behavior for blocks that are both <see cref="Rotatable"/> and <see cref="CubeTextured"/>.
/// </summary>
public class RotatableCubeTextured : BlockBehavior, IBehavior<RotatableCubeTextured, BlockBehavior, Block>
{
    private readonly Rotatable rotatable;
    
    private RotatableCubeTextured(Block subject) : base(subject)
    {
        rotatable = subject.Require<Rotatable>();
        
        subject.Require<CubeTextured>().ActiveTexture.ContributeFunction(GetActiveTexture);
    }

    
    /// <inheritdoc />
    public static RotatableCubeTextured Construct(Block input)
    {
        return new RotatableCubeTextured(input);
    }
    
    private TextureLayout GetActiveTexture(TextureLayout original, State state)
    {
        Side front = rotatable.GetCurrentFront(state);
        return original.Rotated(front);
    }
}
