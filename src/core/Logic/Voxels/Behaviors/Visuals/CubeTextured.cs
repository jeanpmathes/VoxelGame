// <copyright file="CubeTextured.cs" company="VoxelGame">
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
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     Gives a block a texture defined by a <see cref="TextureLayout" />.
///     The texture layout corresponds to texturing each side of a cube with a specific texture.
/// </summary>
public partial class CubeTextured : BlockBehavior, IBehavior<CubeTextured, BlockBehavior, Block>
{
    [Constructible]
    private CubeTextured(Block subject) : base(subject)
    {
        ActiveTexture = Aspect<TextureLayout, State>.New<Exclusive<TextureLayout, State>>(nameof(ActiveTexture), this);
        Rotation = Aspect<(Axis, Int32), State>.New<Exclusive<(Axis, Int32), State>>(nameof(Rotation), this);
    }

    /// <summary>
    ///     The default texture layout to use for the block.
    ///     This should be set through the <see cref="BlockBuilder" /> when defining the block.
    /// </summary>
    public ResolvedProperty<TextureLayout> DefaultTexture { get; }
        = ResolvedProperty<TextureLayout>.New<Exclusive<TextureLayout, Void>>(nameof(DefaultTexture), TextureLayout.Uniform(TID.MissingTexture));

    /// <summary>
    ///     The actually used, state dependent texture layout.
    /// </summary>
    public Aspect<TextureLayout, State> ActiveTexture { get; }

    /// <summary>
    ///     The rotation of the texture on the block.
    /// </summary>
    public Aspect<(Axis axis, Int32 turns), State> Rotation { get; }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        DefaultTexture.Initialize(this);
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
        TextureLayout layout = ActiveTexture.GetValue(DefaultTexture.Get(), state);

        return layout.GetTextureIndex(side, textureIndexProvider, isBlock, Rotation.GetValue((Axis.Y, 0), state));
    }
}
