// <copyright file="Session.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Client.Actors;
using VoxelGame.Client.Logic;
using VoxelGame.Core.Profiling;
using VoxelGame.Toolkit.Components;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Sessions;

/// <summary>
///     Represents a running session, which means a player is playing in a world.
/// </summary>
public sealed class Session : Composed<Session, SessionComponent>
{
    /// <summary>
    ///     Create a new session.
    /// </summary>
    /// <param name="world">The world in which the session is played.</param>
    /// <param name="player">The player playing the session.</param>
    public Session(World world, Player player)
    {
        World = world;
        Player = player;
    }

    /// <inheritdoc />
    protected override Session Self => this;

    /// <summary>
    ///     The player of the session.
    /// </summary>
    public Player Player { get; }

    /// <summary>
    ///     The session in which the player is playing.
    /// </summary>
    private World World { get; }

    /// <summary>
    ///     Perform one logic update cycle.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    /// <param name="timer">A timer to use for profiling.</param>
    public void LogicUpdate(Double deltaTime, Timer? timer)
    {
        Throw.IfDisposed(disposed);

        foreach (SessionComponent component in Components)
        {
            component.OnLogicUpdate(deltaTime, timer);
        }

        World.LogicUpdate(deltaTime, timer);
    }

    /// <summary>
    ///     Perform one render update cycle.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    /// <param name="timer">A timer to use for profiling.</param>
    public void RenderUpdate(Double deltaTime, Timer? timer)
    {
        Throw.IfDisposed(disposed);

        foreach (SessionComponent component in Components)
        {
            component.OnRenderUpdate(deltaTime, timer);
        }

        World.RenderUpdate();
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        if (disposed) return;
        if (!disposing) return;

        World.Dispose();
        Player.Dispose();

        disposed = true;
    }

    #endregion DISPOSABLE
}
