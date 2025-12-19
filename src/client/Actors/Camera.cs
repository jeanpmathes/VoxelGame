// <copyright file = "Camera.cs" company = "VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Actors.Components;

namespace VoxelGame.Client.Actors;

/// <summary>
///     Wraps a <see cref="Graphics.Objects.Camera" /> so that it is an actor.
/// </summary>
public class Camera : Actor
{
    /// <summary>
    ///     Create a new camera actor.
    /// </summary>
    /// <param name="view">The graphics camera to wrap.</param>
    public Camera(Graphics.Objects.Camera view)
    {
        View = view;
        Transform = AddComponent<Transform>();

        Transform.OnTransformChanged += OnTransformChanged;
    }

    /// <summary>
    ///     The internal graphics camera.
    /// </summary>
    public Graphics.Objects.Camera View { get; }

    /// <summary>
    ///     Get the transform of the camera.
    /// </summary>
    public Transform Transform { get; }

    private void OnTransformChanged(Object? sender, EventArgs args)
    {
        View.Position = Transform.Position;
        View.SetOrientation(Transform.Forward, Transform.Right, Transform.Up);
    }

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        if (disposing) Transform.OnTransformChanged -= OnTransformChanged;

        base.Dispose(disposing);
    }
}
