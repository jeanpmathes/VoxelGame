// <copyright file="RenderType.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Visuals;

/// <summary>
///     The render type of a fluid.
/// </summary>
public enum RenderType
{
    /// <summary>
    ///     The fluid is not rendered.
    /// </summary>
    NotRendered,

    /// <summary>
    ///     The fluid is opaque.
    /// </summary>
    Opaque,

    /// <summary>
    ///     The fluid is transparent.
    /// </summary>
    Transparent
}
