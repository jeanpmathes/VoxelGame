// <copyright file="SceneManager.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;

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
    /// <param name="scene">The scene to load.</param>
    public void Load(IScene scene)
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
}