// <copyright file="Overlay.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     Allows blocks to provide an overlay texture.
/// </summary>
/// <seealso cref="IOverlayTextureProvider" />
public partial class Overlay : BlockBehavior, IBehavior<Overlay, BlockBehavior, Block>
{
    private IOverlayTextureProvider? overlayTextureProvider;

    [Constructible]
    private Overlay(Block subject) : base(subject)
    {
        OverlayTextureProvider = Aspect<IOverlayTextureProvider?, Block>.New<Exclusive<IOverlayTextureProvider?, Block>>(nameof(OverlayTextureProvider), this);
    }

    /// <summary>
    ///     The overlay texture provider used by the block.
    /// </summary>
    public Aspect<IOverlayTextureProvider?, Block> OverlayTextureProvider { get; }

    /// <summary>
    ///     The overlay texture provider for the block.
    /// </summary>
    public IOverlayTextureProvider Provider
    {
        get
        {
            overlayTextureProvider ??= OverlayTextureProvider.GetValue(original: null, Subject) ?? new DefaultOverlayTextureProvider();

            return overlayTextureProvider;
        }
    }

    private sealed class DefaultOverlayTextureProvider : IOverlayTextureProvider
    {
        public OverlayTexture GetOverlayTexture(Content content)
        {
            return new OverlayTexture(ITextureIndexProvider.MissingTextureIndex, ColorS.NoTint, IsAnimated: false);
        }
    }
}
