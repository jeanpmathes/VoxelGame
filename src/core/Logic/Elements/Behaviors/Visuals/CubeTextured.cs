// <copyright file="CubeTextured.cs" company="VoxelGame">
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
///     Gives a block a texture defined by a <see cref="TextureLayout" />.
///     The texture layout corresponds to texturing each side of a cube with a specific texture.
/// </summary>
public class CubeTextured : BlockBehavior, IBehavior<CubeTextured, BlockBehavior, Block>
{
    private CubeTextured(Block subject) : base(subject)
    {
        DefaultTextureInitializer = Aspect<TextureLayout, Block>.New<Exclusive<TextureLayout, Block>>(nameof(DefaultTextureInitializer), this);
        ActiveTexture = Aspect<TextureLayout, State>.New<Exclusive<TextureLayout, State>>(nameof(ActiveTexture), this);
    }

    /// <summary>
    ///     The default texture layout to use for the block.
    ///     This should be set through the <see cref="BlockBuilder" /> when defining the block.
    /// </summary>
    public TextureLayout DefaultTexture { get; private set; } = TextureLayout.Uniform(TID.MissingTexture);

    /// <summary>
    ///     Aspect used to initialize the <see cref="DefaultTexture" /> property.
    /// </summary>
    public Aspect<TextureLayout, Block> DefaultTextureInitializer { get; }

    /// <summary>
    ///     The actually used, state dependent texture layout.
    /// </summary>
    public Aspect<TextureLayout, State> ActiveTexture { get; }

    /// <inheritdoc />
    public static CubeTextured Construct(Block input)
    {
        return new CubeTextured(input);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        DefaultTexture = DefaultTextureInitializer.GetValue(TextureLayout.Uniform(TID.MissingTexture), Subject);
    }

    /// <summary>
    ///     Get the texture index for a given state and side of the block.
    /// </summary>
    /// <param name="state">The state of the block to get the texture index for.</param>
    /// <param name="side">The side of the block to get the texture index for.</param>
    /// <param name="textureIndexProvider">The provider to get texture indices from.</param>
    /// <param name="isBlock">Whether the texture is for a block or fluid.</param>
    /// <returns>The texture index for the given state and side.</returns>
    public Int32 GetTextureIndex(State state, Side side, ITextureIndexProvider textureIndexProvider, Boolean isBlock)
    {
        return ActiveTexture.GetValue(DefaultTexture, state).GetTextureIndex(side, textureIndexProvider, isBlock);
    }
}
