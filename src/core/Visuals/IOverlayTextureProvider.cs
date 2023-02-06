using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Provides an overlay texture index.
///     Blocks and fluids implementing this interface should have be a full or varying height block for best effect.
/// </summary>
public interface IOverlayTextureProvider
{
    /// <summary>
    ///     The texture index for the overlay.
    /// </summary>
    int TextureIdentifier { get; }

    /// <summary>
    ///     Get the tint color of this content.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>The tint color.</returns>
    TintColor GetTintColor(Content content);
}


