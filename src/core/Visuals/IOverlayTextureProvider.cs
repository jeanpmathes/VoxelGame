using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Describes an overlay texture.
/// </summary>
/// <param name="TextureIdentifier">The texture identifier, in the texture space of the content type.</param>
/// <param name="Tint">The tint color.</param>
/// <param name="IsAnimated">Whether the texture is animated.</param>
public record struct OverlayTexture(int TextureIdentifier, TintColor Tint, bool IsAnimated);

/// <summary>
///     Provides an overlay texture index.
///     Blocks and fluids implementing this interface should have be a full or varying height block for best effect.
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
