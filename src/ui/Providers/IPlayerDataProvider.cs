// <copyright file="IPlayerDataProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Collections.Properties;

namespace VoxelGame.UI.Providers;

/// <summary>
///     Provides data about a player.
/// </summary>
public interface IPlayerDataProvider
{
    /// <summary>
    ///     The current block/fluid mode.
    /// </summary>
    String Mode { get; }

    /// <summary>
    ///     The current block/fluid selection.
    /// </summary>
    String Selection { get; }

    /// <summary>
    ///     Data for debugging purposes.
    /// </summary>
    Property DebugData { get; }
}
