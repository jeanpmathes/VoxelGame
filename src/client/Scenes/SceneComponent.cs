// <copyright file="SceneComponent.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Profiling;
using VoxelGame.Toolkit.Components;

namespace VoxelGame.Client.Scenes;

/// <summary>
///     Base class for all components of the <see cref="Scene" /> class.
/// </summary>
public class SceneComponent(Scene subject) : Component<Scene>(subject)
{
    /// <inheritdoc cref="Scene.Load" />
    public virtual void OnLoad() {}

    /// <inheritdoc cref="Scene.LogicUpdate" />
    public virtual void OnLogicUpdate(Double deltaTime, Timer? timer) {}

    /// <inheritdoc cref="Scene.RenderUpdate" />
    public virtual void OnRenderUpdate(Double deltaTime, Timer? timer) {}

    /// <inheritdoc cref="Scene.Resize" />
    public virtual void OnResize(Vector2i size) {}

    /// <inheritdoc cref="Scene.Unload" />
    public virtual void OnUnload() {}
}
