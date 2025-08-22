// <copyright file="SingleTextured.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Visuals;

/// <summary>
/// Gives a block a texture defined by a single texture ID (<see cref="TID"/>).
/// </summary>
public class SingleTextured : BlockBehavior, IBehavior<SingleTextured, BlockBehavior, Block>
{
    private SingleTextured(Block subject) : base(subject) 
    {
        DefaultTextureInitializer = Aspect<TID, Block>.New<Exclusive<TID, Block>>(nameof(DefaultTextureInitializer), this);
        ActiveTexture = Aspect<TID, State>.New<Exclusive<TID, State>>(nameof(ActiveTexture), this);
    }

    /// <summary>
    /// The default texture to use for the block.
    /// This should be set through the <see cref="BlockBuilder"/> when defining the block.
    /// </summary>
    public TID DefaultTexture { get; private set; } = TID.MissingTexture;
    
    /// <summary>
    /// Aspect used to initialize the <see cref="DefaultTexture"/> property.
    /// </summary>
    public Aspect<TID, Block> DefaultTextureInitializer { get; }
    
    /// <summary>
    /// The actually used, state dependent texture.
    /// </summary>
    public Aspect<TID, State> ActiveTexture { get; }
    
    /// <inheritdoc />
    public static SingleTextured Construct(Block input)
    {
        return new SingleTextured(input);
    }
    
    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        DefaultTexture = DefaultTextureInitializer.GetValue(TID.MissingTexture, Subject);
    }
    
    /// <summary>
    /// Get the texture index for a given state of the block.
    /// </summary>
    /// <param name="state">The state of the block to get the texture index for.</param>
    /// <param name="textureIndexProvider">The provider to get texture indices from.</param>
    /// <param name="isBlock">Whether the texture is for a block or fluid.</param>
    /// <returns>The texture index for the given state and side.</returns>
    public Int32 GetTextureIndex(State state, ITextureIndexProvider textureIndexProvider, Boolean isBlock)
    {
        return textureIndexProvider.GetTextureIndex( ActiveTexture.GetValue(DefaultTexture, state));
    }
}
