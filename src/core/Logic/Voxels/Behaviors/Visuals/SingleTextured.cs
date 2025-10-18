// <copyright file="SingleTextured.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Utilities;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     Gives a block a texture defined by a single texture ID (<see cref="TID" />).
/// </summary>
public partial class SingleTextured : BlockBehavior, IBehavior<SingleTextured, BlockBehavior, Block>
{
    [Constructible]
    private SingleTextured(Block subject) : base(subject)
    {
        ActiveTexture = Aspect<TID, State>.New<Exclusive<TID, State>>(nameof(ActiveTexture), this);
    }

    /// <summary>
    ///     The default texture to use for the block.
    ///     This should be set through the <see cref="BlockBuilder" /> when defining the block.
    /// </summary>
    public ResolvedProperty<TID> DefaultTexture { get; } = ResolvedProperty<TID>.New<Exclusive<TID, Void>>(nameof(DefaultTexture), TID.MissingTexture);

    /// <summary>
    ///     The actually used, state dependent texture.
    /// </summary>
    public Aspect<TID, State> ActiveTexture { get; }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        DefaultTexture.Initialize(this);
    }

    /// <summary>
    ///     Get the texture index for a given state of the block.
    /// </summary>
    /// <param name="state">The state of the block to get the texture index for.</param>
    /// <param name="textureIndexProvider">The provider to get texture indices from.</param>
    /// <param name="isBlock">Whether the texture is for a block or fluid.</param>
    /// <returns>The texture index for the given state and side.</returns>
    public Int32 GetTextureIndex(State state, ITextureIndexProvider textureIndexProvider, Boolean isBlock)
    {
        return textureIndexProvider.GetTextureIndex(ActiveTexture.GetValue(DefaultTexture.Get(), state));
    }
}
