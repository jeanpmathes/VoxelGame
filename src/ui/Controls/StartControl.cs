// <copyright file="GameControl.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Controls
{
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    internal class StartControl : UserInterfaceControl
    {
        private readonly MainMenu mainMenu;

        internal StartControl(StartUserInterface parent) : base(parent)
        {
            Dock = Dock.Fill;
            mainMenu = new MainMenu(this, Fonts);

            mainMenu.Start += () => Start?.Invoke();
            mainMenu.Exit += () => Exit?.Invoke();
        }

        public event Action? Start;

        public event Action? Exit;
    }
}
