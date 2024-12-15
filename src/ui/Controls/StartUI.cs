// <copyright file="StartUI.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.Controls.Common;
using VoxelGame.UI.Controls.Worlds;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Controls;

/// <summary>
///     Controls the ui of the start scene.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal class StartUI : ControlBase
{
    private const Int32 MainMenuIndex = 0;
    private const Int32 SettingsMenuIndex = 1;
    private const Int32 WorldSelectionMenuIndex = 2;
    private const Int32 CreditsMenuIndex = 3;

    private readonly List<StandardMenu> menus = new();
    private readonly MainMenu mainMenu;

    private readonly Context context;

    internal StartUI(StartUserInterface parent, IWorldProvider worldProvider,
        IEnumerable<SettingsProvider> settingsProviders) : base(parent.Root)
    {
        context = parent.Context;

        Dock = Dock.Fill;

        mainMenu = new MainMenu(this, parent.Context);
        mainMenu.SelectExit += (_, _) => Exit?.Invoke(this, EventArgs.Empty);
        mainMenu.SelectSettings += (_, _) => OpenMenu(SettingsMenuIndex);
        mainMenu.SelectWorlds += (_, _) => OpenMenu(WorldSelectionMenuIndex);
        mainMenu.SelectCredits += (_, _) => OpenMenu(CreditsMenuIndex);

        SettingsMenu settingsMenu = new(this, settingsProviders, parent.Context);
        settingsMenu.Cancel += (_, _) => OpenMenu(MainMenuIndex);

        WorldSelection worldSelection = new(this, worldProvider, parent.Context);
        worldSelection.Cancel += (_, _) => OpenMenu(MainMenuIndex);

        CreditsMenu creditsMenu = new(this, parent.Context);
        creditsMenu.Cancel += (_, _) => OpenMenu(MainMenuIndex);

        menus.Add(mainMenu);
        menus.Add(settingsMenu);
        menus.Add(worldSelection);
        menus.Add(creditsMenu);

        OpenMenu(MainMenuIndex);
    }

    private void OpenMenu(Int32 index)
    {
        foreach (StandardMenu menu in menus) menu.Hide();

        menus[index].Show();
    }

    internal void DisableWorldSelection()
    {
        mainMenu.DisableWorlds();
    }

    internal void OpenMissingResourcesWindow(Property resources)
    {
        Window window = new(this)
        {
            Title = Language.MissingResources,
            DeleteOnClose = true,
            StartPosition = StartPosition.CenterCanvas,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MinimumSize = new Size(width: 1000, height: 1000)
        };

        PropertyBasedTreeControl tree = new(window, resources, context);

        tree.ExpandAll();
    }

    internal event EventHandler? Exit;
}
