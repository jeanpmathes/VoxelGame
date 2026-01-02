// <copyright file="SessionHook.cs" company="VoxelGame">
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
using Microsoft.Extensions.Logging;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Client.Sessions;
using VoxelGame.Core.Profiling;
using VoxelGame.Logging;

namespace VoxelGame.Client.Scenes.Components;

/// <summary>
///     Attaches a <see cref="Session" /> to a <see cref="Scene" />.
/// </summary>
public partial class SessionHook : SceneComponent
{
    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<SessionHook>();

    #endregion LOGGING

    private readonly Session session;

    [Constructible]
    private SessionHook(Scene subject, Session session) : base(subject)
    {
        this.session = session;
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime, Timer? timer)
    {
        using (logger.BeginTimedSubScoped("SessionHook LogicUpdate", timer))
        {
            session.LogicUpdate(deltaTime, timer);
        }
    }

    /// <inheritdoc />
    public override void OnRenderUpdate(Double deltaTime, Timer? timer)
    {
        using (logger.BeginTimedSubScoped("SessionHook RenderUpdate", timer))
        {
            session.RenderUpdate(deltaTime, timer);
        }
    }
}
