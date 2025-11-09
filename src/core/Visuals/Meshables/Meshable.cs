// <copyright file="Meshable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Visuals.Meshables;

/// <summary>
///     Defines how a block is meshed.
/// </summary>
public enum Meshable
{
    /// <summary>
    ///     See <see cref="SimpleBlock" />.
    /// </summary>
    Simple,

    /// <summary>
    ///     See <see cref="ComplexBlock" />.
    /// </summary>
    Foliage,

    /// <summary>
    ///     See <see cref="PartialHeightBlock" />.
    /// </summary>
    Complex,

    /// <summary>
    ///     See <see cref="PartialHeightBlock" />.
    /// </summary>
    PartialHeight,

    /// <summary>
    ///     See <see cref="UnmeshedBlock" />.
    /// </summary>
    Unmeshed
}
