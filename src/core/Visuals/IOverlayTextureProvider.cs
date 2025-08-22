// <copyright file="IOverlayTextureProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic.Elements;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Describes an overlay texture.
/// </summary>
/// <param name="TextureIndex">The texture index, in the texture space of the content type.</param>
/// <param name="Tint">The tint color.</param>
/// <param name="IsAnimated">Whether the texture is animated.</param>
public record struct OverlayTexture(Int32 TextureIndex, ColorS Tint, Boolean IsAnimated);

/// <summary>
///     Provides an overlay texture index.
///     Blocks and fluids implementing this interface should be a full or varying height block for best effect.
/// </summary>
public interface IOverlayTextureProvider
{
    /// <summary>
    ///     Get the overlay texture.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>The overlay texture.</returns>
    OverlayTexture GetOverlayTexture(Content content);
}
