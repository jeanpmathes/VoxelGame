// <copyright file="MetaController.cs" company="VoxelGame">
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
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Utilities;
using VoxelGame.Graphics.Input.Actions;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes.Components;

/// <summary>
///     Responsible for handling all meta input in a <see cref="SessionScene" />.
/// </summary>
public partial class MetaController : SceneComponent
{
    private readonly ToggleButton consoleToggle;
    private readonly PushButton escapeButton;
    private readonly SessionScene scene;
    private readonly InGameUserInterface ui;
    private readonly PushButton unlockMouseButton;

    private Boolean isMouseUnlockedByUserRequest;

    [Constructible]
    private MetaController(SessionScene scene, InGameUserInterface ui) : base(scene)
    {
        this.scene = scene;
        this.ui = ui;

        consoleToggle = scene.Client.Keybinds.GetToggle(scene.Client.Keybinds.Console);
        escapeButton = scene.Client.Keybinds.GetPushButton(scene.Client.Keybinds.Escape);
        unlockMouseButton = scene.Client.Keybinds.GetPushButton(scene.Client.Keybinds.UnlockMouse);

        OnSideliningEnd();

        ui.AnyMetaControlOpened += (_, _) => OnSideliningStart();
        ui.AnyMetaControlClosed += (_, _) => OnSideliningEnd();
    }

    /// <summary>
    ///     Get whether the actual game content is currently sidelined, meaning it is not the main focus of the user.
    ///     One reason for this could be that meta UI is shown on top of the game.
    ///     Note that this does not relate to whether the window is focused or not.
    ///     If sidelined, in-game input should not be handled.
    /// </summary>
    public Boolean IsSidelined { get; private set; }

    /// <inheritdoc />
    public override void OnLogicUpdate(Delta delta, Timer? timer)
    {
        if (!scene.Client.IsFocused)
            return;

        if (unlockMouseButton.Pushed)
        {
            if (isMouseUnlockedByUserRequest)
            {
                OnSideliningEnd();
            }
            else if (!IsSidelined)
            {
                OnSideliningStart();
                isMouseUnlockedByUserRequest = true;
            }
        }

        if (escapeButton.Pushed)
            ui.HandleEscape();

        if (consoleToggle.Changed)
            ui.ToggleConsole();
    }

    private void OnSideliningEnd()
    {
        IsSidelined = false;
        scene.Client.Input.Mouse.SetCursorLock(locked: true);

        isMouseUnlockedByUserRequest = false;
    }

    private void OnSideliningStart()
    {
        IsSidelined = true;
        scene.Client.Input.Mouse.SetCursorLock(locked: false);

        // The mouse was unlocked, but the user did not necessarily explicitly request it.
        isMouseUnlockedByUserRequest = false;
    }
}
