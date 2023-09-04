// <copyright file="RasterPipeline.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Support.Objects;

/// <summary>
///     A pipeline for raster-based rendering.
/// </summary>
public class RasterPipeline : NativeObject
{
    /// <summary>
    ///     Creates a new <see cref="RasterPipeline" />.
    /// </summary>
    public RasterPipeline(IntPtr nativePointer, Client client) : base(nativePointer, client) {}
}
