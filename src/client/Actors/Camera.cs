// <copyright file="Camera.cs" company="VoxelGame">
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
