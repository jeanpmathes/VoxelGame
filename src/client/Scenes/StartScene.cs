// <copyright file="StartScene.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
