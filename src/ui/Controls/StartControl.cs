// <copyright file="GameControl.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Controls
{
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    internal class StartControl : UserInterfaceControl
    {
        private readonly MainMenu mainMenu;
        private readonly WorldSelection worldSelection;

        internal StartControl(StartUserInterface parent, IWorldProvider worldProvider) : base(parent)
        {
            Dock = Dock.Fill;

            mainMenu = new MainMenu(this, Fonts);
            mainMenu.Exit += () => Exit?.Invoke();
            mainMenu.Worlds += OpenWorldSelection;

            worldSelection = new WorldSelection(this, worldProvider, Fonts);
            worldSelection.Cancel += OpenMainMenu;

            worldSelection.Hide();
        }

        private void OpenWorldSelection()
        {
            mainMenu.Hide();
            worldSelection.Show();

            worldSelection.Refresh();
        }

        private void OpenMainMenu()
        {
            worldSelection.Hide();
            mainMenu.Show();
        }

        public event Action? Exit;
    }
}
