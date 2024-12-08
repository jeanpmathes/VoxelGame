// <copyright file="IWorldStates.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

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
    /// <param name="onComplete">Called when the world has successfully terminated.</param>
    /// <returns>True if the world has begun terminating, false if it cannot terminate in the current state.</returns>
    public Boolean BeginTerminating(Action onComplete);

    /// <summary>
    ///     Begin saving the world.
    /// </summary>
    /// <param name="onComplete">Called when the world has successfully saved.</param>
    /// <returns>True if the world has begun saving, false if it cannot save in the current state.</returns>
    public Boolean BeginSaving(Action onComplete);

    /// <summary>
    ///     Fired when the world enters an active state.
    /// </summary>
    public event EventHandler<EventArgs>? Activated;

    /// <summary>
    ///     Fired when the world leaves an active state.
    /// </summary>
    public event EventHandler<EventArgs>? Deactivated;

    /// <summary>
    ///     Fired when the world enters a terminating state.
    /// </summary>
    public event EventHandler<EventArgs>? Terminated;
}
