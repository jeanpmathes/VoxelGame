// <copyright file="StartUI.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Controls
{
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
        private readonly CreditsMenu creditsMenu;
        private readonly MainMenu mainMenu;

        private readonly List<StandardMenu> menus = new();
        private readonly SettingsMenu settingsMenu;
        private readonly WorldSelection worldSelection;

        internal StartUI(StartUserInterface parent, IWorldProvider worldProvider,
            List<ISettingsProvider> settingsProviders) : base(parent.Root)
        {
            Dock = Dock.Fill;

            mainMenu = new MainMenu(this, parent.Context);
            mainMenu.SelectExit += () => Exit?.Invoke();
            mainMenu.SelectSettings += () => OpenMenu(SettingsMenuIndex);
            mainMenu.SelectWorlds += () => OpenMenu(WorldSelectionMenuIndex);
            mainMenu.SelectCredits += () => OpenMenu(CreditsMenuIndex);

            settingsMenu = new SettingsMenu(this, settingsProviders, parent.Context);
            settingsMenu.Cancel += () => OpenMenu(MainMenuIndex);

            worldSelection = new WorldSelection(this, worldProvider, parent.Context);
            worldSelection.Cancel += () => OpenMenu(MainMenuIndex);

            creditsMenu = new CreditsMenu(this, parent.Context);
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

        public event Action? Exit;
    }
}
