// <copyright file="TextureOverride.cs" company="VoxelGame">
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
using System.Collections.Generic;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Visuals;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     Override the texture provided by models with custom textures.
/// </summary>
public partial class TextureOverride : BlockBehavior, IBehavior<TextureOverride, BlockBehavior, Block>
{
    [Constructible]
    private TextureOverride(Block subject) : base(subject) {}

    /// <summary>
    ///     Optional textures to override the texture provided by a model.
    /// </summary>
    public ResolvedProperty<IReadOnlyDictionary<Int32, TID>?> Textures { get; }
        = ResolvedProperty<IReadOnlyDictionary<Int32, TID>?>.New<Exclusive<IReadOnlyDictionary<Int32, TID>?, Void>>(nameof(Textures));

    /// <summary>
    ///     Override all textures with the given replacement texture.
    /// </summary>
    /// <param name="replacement">The replacement texture.</param>
    /// <returns>The created replacement dictionary.</returns>
    public static IReadOnlyDictionary<Int32, TID> All(TID replacement)
    {
        return new Dictionary<Int32, TID> {[key: -1] = replacement};
    }

    /// <summary>
    ///     Override a single texture at the given index with the given replacement texture.
    /// </summary>
    /// <param name="index">The index, corresponding to the order of textures in the model.</param>
    /// <param name="replacement">The replacement texture.</param>
    /// <returns>The created replacement dictionary.</returns>
    public static IReadOnlyDictionary<Int32, TID> Single(Int32 index, TID replacement)
    {
        return new Dictionary<Int32, TID> {[index] = replacement};
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Textures.Initialize(this);
    }
}
