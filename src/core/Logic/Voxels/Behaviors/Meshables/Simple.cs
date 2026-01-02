// <copyright file="Simple.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;

/// <summary>
///     Corresponds to <see cref="Meshable.Simple" />.
/// </summary>
public partial class Simple : BlockBehavior, IBehavior<Simple, BlockBehavior, Block>, IMeshable
{
    private readonly Meshed meshed;
    private readonly CubeTextured textured;

    [Constructible]
    private Simple(Block subject) : base(subject)
    {
        meshed = subject.Require<Meshed>();
        textured = subject.Require<CubeTextured>();

        IsTextureRotated = Aspect<Boolean, (State, Side)>.New<Exclusive<Boolean, (State, Side)>>(nameof(IsTextureRotated), this);
    }

    /// <summary>
    ///     Whether the texture is rotated.
    /// </summary>
    public Aspect<Boolean, (State state, Side side)> IsTextureRotated { get; }

    /// <inheritdoc />
    public Meshable Type => Meshable.Simple;

    /// <summary>
    ///     Get the mesh data for a given side and state of the block.
    /// </summary>
    /// <param name="state">The state to get the mesh data for.</param>
    /// <param name="side">The side of the block to get the mesh data for.</param>
    /// <param name="textureIndexProvider">Provides texture indices for given texture IDs.</param>
    /// <returns>The mesh data for the given side and state.</returns>
    public MeshData GetMeshData(State state, Side side, ITextureIndexProvider textureIndexProvider)
    {
        Boolean isTextureRotated = IsTextureRotated.GetValue(original: false, (state, side));
        ColorS tint = meshed.Tint.GetValue(ColorS.NoTint, state);
        Boolean isAnimated = meshed.IsAnimated.GetValue(original: false, state);

        Int32 textureIndex = textured.GetTextureIndex(state, side, textureIndexProvider, isBlock: true);

        return new MeshData(textureIndex, isTextureRotated, tint, isAnimated && textureIndex != ITextureIndexProvider.MissingTextureIndex);
    }

    /// <summary>
    ///     The mesh data for a simple block.
    /// </summary>
    /// <param name="TextureIndex">The index of the texture to use.</param>
    /// <param name="IsTextureRotated">Whether the texture is rotated.</param>
    /// <param name="Tint">The tint color to apply to the mesh.</param>
    /// <param name="IsAnimated">Whether the texture is animated.</param>
    public readonly record struct MeshData(Int32 TextureIndex, Boolean IsTextureRotated, ColorS Tint, Boolean IsAnimated);
}
