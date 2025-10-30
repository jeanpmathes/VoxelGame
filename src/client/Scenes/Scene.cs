// <copyright file="Scene.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Profiling;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Components;
using VoxelGame.Annotations.Attributes;

namespace VoxelGame.Client.Scenes;

/// <summary>
///     The base class for all scenes.
/// </summary>
[ComponentSubject(typeof(SceneComponent))]
public abstract partial class Scene(Application.Client client) : Composed<Scene, SceneComponent>
{
    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Scene>();

    #endregion LOGGING

    /// <summary>
    ///     Get the client that this scene belongs to.
    /// </summary>
    internal Application.Client Client { get; } = client;

    /// <summary>
    ///     Load the scene.
    /// </summary>
    public void Load()
    {
        OnLoad();
        OnLoadComponents();
    }
    
    /// <inheritdoc cref="Scene.OnLoad" />
    [ComponentEvent(nameof(SceneComponent.OnLoad))]
    private partial void OnLoadComponents();

    /// <summary>
    ///     Called when the scene is loaded.
    /// </summary>
    protected virtual void OnLoad() {}

    /// <summary>
    ///     Perform an update cycle.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    /// <param name="timer">A timer for profiling.</param>
    public void LogicUpdate(Double deltaTime, Timer? timer)
    {
        using Timer? subTimer = logger.BeginTimedSubScoped("Scene LogicUpdate", timer);

        OnLogicUpdate(deltaTime, subTimer);
        OnLogicUpdateComponents(deltaTime, subTimer);
    }

    /// <inheritdoc cref="Scene.OnLogicUpdate" />
    [ComponentEvent(nameof(SceneComponent.OnLogicUpdate))]
    private partial void OnLogicUpdateComponents(Double deltaTime, Timer? timer);

    /// <summary>
    ///     Called each logic update cycle.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    /// <param name="timer">A timer for profiling.</param>
    protected virtual void OnLogicUpdate(Double deltaTime, Timer? timer) {}

    /// <summary>
    ///     Perform a render cycle.
    /// </summary>
    /// <param name="deltaTime">The time since the last render.</param>
    /// <param name="timer">A timer for profiling.</param>
    public void RenderUpdate(Double deltaTime, Timer? timer)
    {
        using Timer? subTimer = logger.BeginTimedSubScoped("Scene RenderUpdate", timer);

        OnRenderUpdate(deltaTime, subTimer);
        OnRenderUpdateComponents(deltaTime, subTimer);
    }

    /// <inheritdoc cref="Scene.OnRenderUpdate" />
    [ComponentEvent(nameof(SceneComponent.OnRenderUpdate))]
    private partial void OnRenderUpdateComponents(Double deltaTime, Timer? timer);

    /// <summary>
    ///     Called each render update cycle.
    /// </summary>
    /// <param name="deltaTime">The time since the last render.</param>
    /// <param name="timer">A timer for profiling.</param>
    protected virtual void OnRenderUpdate(Double deltaTime, Timer? timer) {}

    /// <summary>
    ///     Handle a game resize.
    /// </summary>
    /// <param name="size">The new size.</param>
    public void Resize(Vector2i size)
    {
        OnResize(size);
        OnResizeComponents(size);
    }

    /// <inheritdoc cref="Scene.OnResize" />
    [ComponentEvent(nameof(SceneComponent.OnResize))]
    private partial void OnResizeComponents(Vector2i size);

    /// <summary>
    ///     Handle a game resize.
    /// </summary>
    /// <param name="size">The new size.</param>
    protected virtual void OnResize(Vector2i size) {}

    /// <summary>
    ///     Unload this scene. After unloading, a scene must not be used anymore.
    /// </summary>
    public void Unload()
    {
        OnUnload();
        OnUnloadComponents();
    }

    /// <inheritdoc cref="Scene.OnUnload" />
    [ComponentEvent(nameof(SceneComponent.OnUnload))]
    private partial void OnUnloadComponents();

    /// <summary>
    ///     Called when the scene is unloaded.
    /// </summary>
    protected virtual void OnUnload() {}

    /// <summary>
    ///     Whether the window can be closed in this scene.
    /// </summary>
    public abstract Boolean CanCloseWindow();
}
