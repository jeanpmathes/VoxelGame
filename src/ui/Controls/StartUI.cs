// <copyright file="StartUI.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Resources.Language;
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
    private const int MainMenuIndex = 0;
    private const int SettingsMenuIndex = 1;
    private const int WorldSelectionMenuIndex = 2;
    private const int CreditsMenuIndex = 3;
    private readonly MainMenu mainMenu;

    private readonly List<StandardMenu> menus = new();

    private readonly WorldSelection worldSelection;

    internal StartUI(StartUserInterface parent, IWorldProvider worldProvider,
        ICollection<ISettingsProvider> settingsProviders) : base(parent.Root)
    {
        Dock = Dock.Fill;

        Exit = delegate {};

        mainMenu = new MainMenu(this, parent.Context);
        mainMenu.SelectExit += (_, _) => Exit(this, EventArgs.Empty);
        mainMenu.SelectSettings += (_, _) => OpenMenu(SettingsMenuIndex);
        mainMenu.SelectWorlds += (_, _) => OpenMenu(WorldSelectionMenuIndex);
        mainMenu.SelectCredits += (_, _) => OpenMenu(CreditsMenuIndex);

        SettingsMenu settingsMenu = new(this, settingsProviders, parent.Context);
        settingsMenu.Cancel += (_, _) => OpenMenu(MainMenuIndex);

        worldSelection = new WorldSelection(this, worldProvider, parent.Context);
        worldSelection.Cancel += (_, _) => OpenMenu(MainMenuIndex);

        CreditsMenu creditsMenu = new(this, parent.Context);
        creditsMenu.Cancel += (_, _) => OpenMenu(MainMenuIndex);

        menus.Add(mainMenu);
        menus.Add(settingsMenu);
        menus.Add(worldSelection);
        menus.Add(creditsMenu);

        OpenMenu(MainMenuIndex);
    }

    private void OpenMenu(int index)
    {
        foreach (StandardMenu menu in menus) menu.Hide();

        menus[index].Show();

        if (index == WorldSelectionMenuIndex) worldSelection.Refresh();
    }

    internal void DisableWorldSelection()
    {
        mainMenu.DisableWorlds();
    }

    internal void OpenMissingResourcesWindow(Tree<string> resources)
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

        Tree tree = new(window);
        tree.SetContent(resources);
    }

    internal event EventHandler Exit;
}
