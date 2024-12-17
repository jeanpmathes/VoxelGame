﻿// <copyright file="SceneManager.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Profiling;
using VoxelGame.Logging;

namespace VoxelGame.Client.Scenes;

/// <summary>
///     Manages scenes, switching between them.
/// </summary>
public partial class SceneManager
{
    private IScene? current;

    /// <summary>
    ///     Load a scene.
    /// </summary>
    /// <param name="scene">The scene to load, or null to just unload the current scene.</param>
    public void Load(IScene? scene)
    {
        LogSwitchingScene(logger, current, scene);

        Unload();

        current = scene;

        Load();
    }

    private void Load()
    {
        LogLoadingScene(logger, current);

        current?.Load();
    }

    /// <summary>
    ///     Unload the current scene.
    /// </summary>
    public void Unload()
    {
        if (current == null) return;

        LogUnloadingScene(logger, current);

        current.Unload();
        current.Dispose();

        Visuals.Graphics.Instance.Reset();

        Cleanup();
    }

    private static void Cleanup()
    {
        #pragma warning disable S1215 // When unloading, many objects have just died.
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        #pragma warning restore S1215 // When unloading, many objects have just died.
    }

    /// <summary>
    ///     Render the current scene.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    /// <param name="timer">A timer for profiling.</param>
    public void RenderUpdate(Double deltaTime, Timer? timer)
    {
        current?.RenderUpdate(deltaTime, timer);
    }

    /// <summary>
    ///     Update the current scene.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    /// <param name="timer">A timer for profiling.</param>
    public void LogicUpdate(Double deltaTime, Timer? timer)
    {
        current?.LogicUpdate(deltaTime, timer);
    }

    /// <summary>
    ///     Notify the current scene of the window being resized.
    /// </summary>
    /// <param name="size">The new window size.</param>
    public void OnResize(Vector2i size)
    {
        current?.OnResize(size);
    }

    /// <summary>
    ///     Whether the current scene allows that the window is closed.
    /// </summary>
    public Boolean CanCloseWindow()
    {
        return current?.CanCloseWindow() ?? true;
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<SceneManager>();

    [LoggerMessage(EventId = LogID.SceneManager + 0, Level = LogLevel.Debug, Message = "Initiating scene change from {OldScene} to {NewScene}")]
    private static partial void LogSwitchingScene(ILogger logger, IScene? oldScene, IScene? newScene);

    [LoggerMessage(EventId = LogID.SceneManager + 1, Level = LogLevel.Information, Message = "Loading scene {Scene}")]
    private static partial void LogLoadingScene(ILogger logger, IScene? scene);

    [LoggerMessage(EventId = LogID.SceneManager + 2, Level = LogLevel.Information, Message = "Unloading scene {Scene}")]
    private static partial void LogUnloadingScene(ILogger logger, IScene? scene);

    #endregion LOGGING
}
