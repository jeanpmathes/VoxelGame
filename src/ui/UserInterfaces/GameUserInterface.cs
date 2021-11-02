// <copyright file="GameUserInterface.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Windowing.Desktop;
using VoxelGame.Input;
using VoxelGame.UI.Controls;

namespace VoxelGame.UI.UserInterfaces
{
    public class GameUserInterface : UserInterface
    {
        private GameUI? control;

        public GameUserInterface(GameWindow window, InputListener inputListener, bool drawBackground) : base(
            window,
            inputListener,
            drawBackground) {}

        public bool IsHidden
        {
            get => control?.IsHidden ?? false;
            set
            {
                if (control == null) return;
                control.IsHidden = value;
            }
        }

        public override void CreateControl()
        {
            control?.Dispose();
            control = new GameUI(this);
        }

        public event Action? WorldExit;

        public event Action? MenuOpen;
        public event Action? MenuClose;

        public void SetUpdateRate(double fps, double ups)
        {
            control?.SetUpdateRate(fps, ups);
        }

        public void SetPlayerSelection(string category, string selection)
        {
            control?.SetPlayerSelection($"{category}: {selection}");
        }

        public void OpenInGameMenu()
        {
            if (control == null) return;

            control.OpenInGameMenu();
            MenuOpen?.Invoke();
        }

        internal void HandleInGameMenuClosed()
        {
            MenuClose?.Invoke();
        }

        internal void ExitWorld()
        {
            WorldExit?.Invoke();
        }
    }
}