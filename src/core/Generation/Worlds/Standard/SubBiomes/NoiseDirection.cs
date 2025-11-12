// <copyright file="NoiseDirection.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Generation.Worlds.Standard.SubBiomes;

/// <summary>
///     The direction in which the noise-based offset is applied to the sub-biome height.
/// </summary>
public enum NoiseDirection
{
    /// <summary>
    ///     The noise-based offset is applied in both directions, up and down.
    /// </summary>
    Both,

    /// <summary>
    ///     The noise-based offset is applied only upwards, essentially using the absolute value of the noise value.
    /// </summary>
    Up,

    /// <summary>
    ///     The noise-based offset is applied only downwards, essentially using the negative absolute value of the noise value.
    /// </summary>
    Down
}
