﻿// <copyright file="SessionHook.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using VoxelGame.Client.Sessions;
using VoxelGame.Core.Profiling;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Scenes.Components;

/// <summary>
/// Attaches a <see cref="Session"/> to a <see cref="Scene"/>.
/// </summary>
public class SessionHook : SceneComponent, IConstructible<Scene, Session, SessionHook>
{
    private readonly Session session;
    
    private SessionHook(Scene subject, Session session) : base(subject)
    {
        this.session = session;
    }
    
    /// <inheritdoc />
    public static SessionHook Construct(Scene input1, Session input2)
    {
        return new SessionHook(input1, input2);
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

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<SessionHook>();

    #endregion LOGGING
}
