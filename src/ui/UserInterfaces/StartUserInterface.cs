// <copyright file="StartUserInterface.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Windowing.Desktop;
using VoxelGame.UI.Controls;
using VoxelGame.UI.Providers;

namespace VoxelGame.UI.UserInterfaces
{
    public class StartUserInterface : UserInterface
    {
        private readonly IWorldProvider worldProvider;

        private StartUI? control;

        public StartUserInterface(GameWindow window, IWorldProvider worldProvider, bool drawBackground) : base(
            window,
            drawBackground)
        {
            this.worldProvider = worldProvider;
        }

        public override void CreateControl()
        {
            control?.Dispose();
            control = new StartUI(this, worldProvider);
        }

        public void SetExitAction(Action exit)
        {
            if (control == null) return;

            control.Exit += exit;
        }
    }
}