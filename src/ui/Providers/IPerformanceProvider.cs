// <copyright file="IPerformanceProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.UI.Providers;

/// <summary>
///     Provides game performance information.
/// </summary>
public interface IPerformanceProvider
{
    /// <summary>
    ///     The current FPS (frames per second).
    /// </summary>
    Double FPS { get; }

    /// <summary>
    ///     The current UPS (updates per second).
    /// </summary>
    Double UPS { get; }
}
