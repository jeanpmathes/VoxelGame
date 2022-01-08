namespace VoxelGame.Core.Visuals
{
    /// <summary>
    ///     Provides an overlay texture index.
    /// </summary>
    public interface IOverlayTextureProvider
    {
        /// <summary>
        ///     The texture index for the overlay.
        /// </summary>
        int TextureIdentifier { get; }
    }
}