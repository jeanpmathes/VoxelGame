// <copyright file="IPerformanceProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.UI.Providers;

/// <summary>
///     Provides game performance information.
/// </summary>
public interface IPerformanceProvider
{
    /// <summary>
    ///     The current FPS (frames per second).
    /// </summary>
    public double FPS { get; }

    /// <summary>
    ///     The current UPS (updates per second).
    /// </summary>
    public double UPS { get; }
}
