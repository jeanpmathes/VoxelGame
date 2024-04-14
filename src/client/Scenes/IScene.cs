// <copyright file="IScene.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Profiling;

namespace VoxelGame.Client.Scenes;

/// <summary>
///     The base interface for all scenes.
/// </summary>
public interface IScene : IDisposable
{
    /// <summary>
    ///     Load the scene.
    /// </summary>
    void Load();

    /// <summary>
    ///     Perform an update cycle.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    /// <param name="timer">A timer for profiling.</param>
    void Update(Double deltaTime, Timer? timer);

    /// <summary>
    ///     Handle a game resize.
    /// </summary>
    /// <param name="size">The new size.</param>
    void OnResize(Vector2i size);

    /// <summary>
    ///     Perform a render cycle.
    /// </summary>
    /// <param name="deltaTime">The time since the last render.</param>
    /// <param name="timer">A timer for profiling.</param>
    void Render(Double deltaTime, Timer? timer);

    /// <summary>
    ///     Unload this scene. After unloading, a scene must not be used anymore.
    /// </summary>
    void Unload();

    /// <summary>
    ///     Whether the window can be closed in this scene.
    /// </summary>
    Boolean CanCloseWindow();
}
