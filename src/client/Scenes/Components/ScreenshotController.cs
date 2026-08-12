// <copyright file="ScreenshotController.cs" company="VoxelGame">
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
using VoxelGame.Annotations.Attributes;
using VoxelGame.Client.Inputs;
using VoxelGame.Client.Inputs.Actions;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Scenes.Components;

/// <summary>
///     Controls the screenshot functionality in a <see cref="SessionScene" />.
/// </summary>
public partial class ScreenshotController : SceneComponent
{
    private readonly SessionScene scene;
    private readonly PushButton button;

    [Constructible]
    private ScreenshotController(SessionScene scene) : base(scene)
    {
        this.scene = scene;

        button = scene.Client.Keybinds.Use(Keybinds.Screenshot);
    }

    /// <inheritdoc />
    public override void OnInputUpdate(Delta delta, Timer? timer)
    {
        if (button.Pushed) scene.Client.TakeScreenshot(Program.ScreenshotDirectory);
    }

    #region DISPOSABLE

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        if (disposing) button.Dispose();
    }

    #endregion DISPOSABLE
}
