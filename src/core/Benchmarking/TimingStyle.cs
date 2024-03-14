// <copyright file="TimingStyle.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Benchmarking;

/// <summary>
///     How a time measurement is performed and evaluated.
/// </summary>
public enum TimingStyle
{
    /// <summary>
    ///     The measurement is performed multiple times and a sliding window is used.
    /// </summary>
    Reoccurring,

    /// <summary>
    ///     The measurement is performed once and the result is used.
    /// </summary>
    Once
}
