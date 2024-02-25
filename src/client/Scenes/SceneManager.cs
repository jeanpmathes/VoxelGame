// <copyright file="SceneManager.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime;
using OpenTK.Mathematics;
using VoxelGame.Client.Visuals;

namespace VoxelGame.Client.Scenes;

/// <summary>
///     Manages scenes, switching between them.
/// </summary>
public class SceneManager
{
    private IScene? current;

    /// <summary>
    ///     Load a scene.
    /// </summary>
    /// <param name="scene">The scene to load, or null to just unload the current scene.</param>
    public void Load(IScene? scene)
    {
        Unload();

        current = scene;

        Load();
    }

    private void Load()
    {
        current?.Load();
    }

    /// <summary>
    ///     Unload the current scene.
    /// </summary>
    public void Unload()
    {
        if (current == null) return;

        current.Unload();
        current.Dispose();

        Graphics.Instance.Reset();

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
    public void Render(float deltaTime)
    {
        current?.Render(deltaTime);
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
    ///     Update the current scene.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    public void Update(double deltaTime)
    {
        current?.Update(deltaTime);
    }

    /// <summary>
    ///     Whether the current scene allows that the window is closed.
    /// </summary>
    public bool CanCloseWindow()
    {
        return current?.CanCloseWindow() ?? true;
    }
}
