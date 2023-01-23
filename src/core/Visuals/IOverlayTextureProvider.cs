namespace VoxelGame.Core.Visuals;

/// <summary>
///     Provides an overlay texture index.
///     Blocks implementing this interface should have be a full or varying height block for best effect.
/// </summary>
public interface IOverlayTextureProvider
{
    /// <summary>
    ///     The texture index for the overlay.
    /// </summary>
    int TextureIdentifier { get; }
}

