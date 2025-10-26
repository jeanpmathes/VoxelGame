// <copyright file="IWorldStates.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Updates;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Gives access to the world states and allows to observe them.
/// </summary>
public interface IWorldStates
{
    /// <summary>
    ///     Whether the world is active.
    /// </summary>
    public Boolean IsActive { get; }

    /// <summary>
    ///     Whether the world is terminating.
    /// </summary>
    public Boolean IsTerminating { get; }

    /// <summary>
    ///     Begin terminating the world.
    /// </summary>
    /// <returns>The started activity, or <c>null</c> if the world cannot be terminated.</returns>
    public Activity? BeginTerminating();

    /// <summary>
    ///     Begin saving the world.
    /// </summary>
    /// <returns>The started activity, or <c>null</c> if the world cannot be saved.</returns>
    public Activity? BeginSaving();

    /// <summary>
    ///     Fired when the world enters an active state.
    ///     Note that this is also fired after saving is complete.
    /// </summary>
    public event EventHandler<EventArgs>? Activating;

    /// <summary>
    ///     Fired when the world leaves an active state.
    /// </summary>
    public event EventHandler<EventArgs>? Deactivating;

    /// <summary>
    ///     Fired when the world enters a terminating state.
    /// </summary>
    public event EventHandler<EventArgs>? Terminating;
}
