// <copyright file="SceneManager.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Client.Application.Components;
using VoxelGame.Core.App;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Updates;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;
using Activity = VoxelGame.Core.Updates.Activity;

namespace VoxelGame.Client.Scenes;

/// <summary>
///     Manages scenes, switching between them.
/// </summary>
public partial class SceneManager : ApplicationComponent, IConstructible<Core.App.Application, SceneManager>
{
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly SceneOperationDispatch? dispatch;

    private Scene? current;
    private (Scene? scene, Action completion)? next;
    
    private SceneManager(Core.App.Application application) : base(application)
    {
        dispatch = application.GetComponent<SceneOperationDispatch>();
    }

    /// <inheritdoc />
    public static SceneManager Construct(Core.App.Application input)
    {
        return new SceneManager(input);
    }
    
    /// <summary>
    ///     Whether a scene is currently loaded or is currently being loaded.
    /// </summary>
    public Boolean IsActive => current is not null || next is {scene: not null};

    /// <summary>
    ///     Begin loading a new scene, unloading the current one if necessary.
    ///     Actual loading and unloading will happen in the next update cycle.
    /// </summary>
    /// <param name="scene">The scene to load.</param>
    /// <returns>The activity that is used to track the loading process.</returns>
    public Activity BeginLoad(Scene scene)
    {
        Debug.Assert(next == null);
        
        LogSwitchingScene(logger, current, scene);
        
        var activity = Activity.Create(out Action completion);
        
        next = (scene, completion);
        
        return activity;
    }

    private void Transition()
    {
        if (next is not {scene: var scene, completion: {} completion}) 
            return;
        
        Unload();

        if (scene != null)
            Load(scene);
        
        completion();
        next = null;
    }

    private void Load(Scene scene)
    {
        LogLoadingScene(logger, current);
        
        current = scene;
        current.Load();
    }

    private void Unload()
    {
        if (current == null) return;

        if (dispatch != null)
            CancelOrCompleteDispatch(dispatch);

        LogUnloadingScene(logger, current);

        current.Unload();
        current.Dispose();
        current = null;

        Visuals.Graphics.Instance.Reset();

        Cleanup();
    }

    /// <summary>
    ///     Begin unloading the current scene, if any.
    ///     Actual unloading will happen in the next update cycle.
    /// </summary>
    /// <returns>The activity that is used to track the unloading process.</returns>
    public Activity BeginUnload()
    {
        LogStartingOfUnloadingScene(logger, current);
        
        var activity = Activity.Create(out Action completion);
        
        next = (null, completion);
        
        return activity;
    }
    
    /// <summary>
    /// Unload the current scene immediately, without waiting for the next update cycle.
    /// </summary>
    public void UnloadImmediately()
    {
        Unload();
    }

    private static void CancelOrCompleteDispatch(OperationUpdateDispatch dispatch)
    {
        LogCompletingDispatch(logger);

        dispatch.CancelAll();
        dispatch.CompleteAll();

        LogCompletedDispatch(logger);
    }

    private static void Cleanup()
    {
        #pragma warning disable S1215 // When unloading, many objects have just died.
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        #pragma warning restore S1215 // When unloading, many objects have just died.
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double delta, Timer? timer)
    {
        Transition();
        
        current?.LogicUpdate(delta, timer);
    }

    /// <inheritdoc />
    public override void OnRenderUpdate(Double delta, Timer? timer)
    {
        current?.RenderUpdate(delta, timer);
    }

    /// <summary>
    ///     Notify the current scene of the window being resized.
    /// </summary>
    /// <param name="size">The new window size.</param>
    public void OnResize(Vector2i size)
    {
        current?.Resize(size);
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
    private static partial void LogSwitchingScene(ILogger logger, Scene? oldScene, Scene? newScene);
    
    [LoggerMessage(EventId = LogID.SceneManager + 1, Level = LogLevel.Debug, Message = "Initiating unloading of {OldScene}")]
    private static partial void LogStartingOfUnloadingScene(ILogger logger, Scene? oldScene);

    [LoggerMessage(EventId = LogID.SceneManager + 2, Level = LogLevel.Information, Message = "Loading scene {Scene}")]
    private static partial void LogLoadingScene(ILogger logger, Scene? scene);

    [LoggerMessage(EventId = LogID.SceneManager + 3, Level = LogLevel.Information, Message = "Unloading scene {Scene}")]
    private static partial void LogUnloadingScene(ILogger logger, Scene? scene);

    [LoggerMessage(EventId = LogID.SceneManager + 4, Level = LogLevel.Debug, Message = "Cancelling and completing operations")]
    private static partial void LogCompletingDispatch(ILogger logger);

    [LoggerMessage(EventId = LogID.SceneManager + 5, Level = LogLevel.Debug, Message = "Cancelled and completed operations")]
    private static partial void LogCompletedDispatch(ILogger logger);

    #endregion LOGGING
}
