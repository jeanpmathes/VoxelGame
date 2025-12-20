// <copyright file="ScreenshotController.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
using VoxelGame.Core.Profiling;
using VoxelGame.Graphics.Input.Actions;

namespace VoxelGame.Client.Scenes.Components;

/// <summary>
///     Controls the screenshot functionality in a <see cref="SessionScene" />.
/// </summary>
public partial class ScreenshotController : SceneComponent
{
    private readonly PushButton button;
    private readonly SessionScene scene;

    [Constructible]
    private ScreenshotController(SessionScene scene) : base(scene)
    {
        this.scene = scene;

        button = scene.Client.Keybinds.GetPushButton(scene.Client.Keybinds.Screenshot);
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime, Timer? timer)
    {
        if (scene.CanHandleGameInput && button.Pushed) scene.Client.TakeScreenshot(Program.ScreenshotDirectory);
    }
}
