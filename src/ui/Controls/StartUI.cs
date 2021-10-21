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
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    internal class StartUI : ControlBase
    {
        private readonly MainMenu mainMenu;
        private readonly SettingsMenu settingsMenu;
        private readonly WorldSelection worldSelection;

        internal StartUI(StartUserInterface parent, IWorldProvider worldProvider,
            List<ISettingsProvider> settingsProviders) : base(parent.Root)
        {
            Dock = Dock.Fill;

            mainMenu = new MainMenu(this, parent.Context);
            mainMenu.SelectExit += () => Exit?.Invoke();
            mainMenu.SelectWorlds += OpenWorldSelection;
            mainMenu.SelectSettings += OpenSettingsMenu;

            worldSelection = new WorldSelection(this, worldProvider, parent.Context);
            worldSelection.Cancel += OpenMainMenu;

            settingsMenu = new SettingsMenu(this, settingsProviders, parent.Context);
            settingsMenu.Cancel += OpenMainMenu;

            OpenMainMenu();
        }

        private void OpenWorldSelection()
        {
            mainMenu.Hide();
            settingsMenu.Hide();

            worldSelection.Show();

            worldSelection.Refresh();
        }

        private void OpenMainMenu()
        {
            worldSelection.Hide();
            settingsMenu.Hide();

            mainMenu.Show();
        }

        private void OpenSettingsMenu()
        {
            mainMenu.Hide();
            settingsMenu.Hide();

            settingsMenu.Show();
        }

        public event Action? Exit;
    }
}