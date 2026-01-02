// <copyright file="Session.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Client.Actors;
using VoxelGame.Client.Logic;
using VoxelGame.Core.Profiling;
using VoxelGame.Toolkit.Components;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Sessions;

/// <summary>
///     Represents a running session, which means a player is playing in a world.
/// </summary>
[ComponentSubject(typeof(SessionComponent))]
public sealed partial class Session : Composed<Session, SessionComponent>
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
        ExceptionTools.ThrowIfDisposed(disposed);

        OnLogicUpdate(deltaTime, timer);

        World.LogicUpdate(deltaTime, timer);
    }

    /// <inheritdoc cref="Session.LogicUpdate" />
    [ComponentEvent(nameof(SessionComponent.OnLogicUpdate))]
    private partial void OnLogicUpdate(Double deltaTime, Timer? timer);

    /// <summary>
    ///     Perform one render update cycle.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    /// <param name="timer">A timer to use for profiling.</param>
    public void RenderUpdate(Double deltaTime, Timer? timer)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        OnRenderUpdate(deltaTime, timer);

        World.RenderUpdate();
    }

    /// <inheritdoc cref="Session.RenderUpdate" />
    [ComponentEvent(nameof(SessionComponent.OnRenderUpdate))]
    private partial void OnRenderUpdate(Double deltaTime, Timer? timer);

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
