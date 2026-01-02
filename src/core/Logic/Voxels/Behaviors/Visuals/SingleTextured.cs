// <copyright file="SingleTextured.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Visuals;
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
