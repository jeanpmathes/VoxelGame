// <copyright file="StartScene.cs" company="VoxelGame">
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
using System.Collections.Generic;
using VoxelGame.Client.Application.Worlds;
using VoxelGame.Client.Scenes.Components;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.UI;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes;

/// <summary>
///     The scene the game starts in. It contains the main menu.
/// </summary>
public sealed class StartScene : Scene
{
    private readonly Func<Boolean> isSafeToClose;

    internal StartScene(Application.Client client, UserInterfaceResources uiResources, ResourceLoadingIssueReport? resourceLoadingIssueReport, Int32? loadWorldDirectly) : base(client)
    {
        WorldProvider worldProvider = new(client, Program.WorldsDirectory);
        worldProvider.WorldActivation += (_, world) => client.StartSession(world);

        List<SettingsProvider> settingsProviders =
        [
            SettingsProvider.Wrap(client.Settings),
            SettingsProvider.Wrap(client.Keybinds),
            SettingsProvider.Wrap(client.Graphics)
        ];

        StartUserInterface ui = new(
            client.Input,
            client.Settings,
            worldProvider,
            settingsProviders,
            uiResources,
            drawBackground: true);

        isSafeToClose = () => ui.IsSafeToClose;

        AddComponent<UserInterfaceHook, UserInterface>(ui);
        AddComponent<SetExitAction, StartUserInterface>(ui);

        if (resourceLoadingIssueReport != null)
            AddComponent<ResourceLoadingReportHook, (ResourceLoadingIssueReport, StartUserInterface)>(
                (resourceLoadingIssueReport, ui));

        if (loadWorldDirectly.HasValue) AddComponent<DirectWorldLoad, (IWorldProvider, Int32)>((worldProvider, loadWorldDirectly.Value));
    }

    /// <inheritdoc />
    protected override void OnLoad()
    {
        Client.Input.Mouse.SetCursorLock(locked: false);
    }

    /// <inheritdoc />
    public override Boolean CanCloseWindow()
    {
        return isSafeToClose();
    }
}
