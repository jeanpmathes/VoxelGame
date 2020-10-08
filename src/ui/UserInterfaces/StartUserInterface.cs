// <copyright file="StartUserInterface.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using VoxelGame.UI.Controls;

namespace VoxelGame.UI.UserInterfaces
{
    public class StartUserInterface : UserInterface
    {
        private StartControl control = null!;

        public StartUserInterface(OpenToolkit.Windowing.Desktop.GameWindow window, bool drawBackground) : base(window, drawBackground)
        {
        }

        public override void CreateControl()
        {
            control?.Dispose();
            control = new StartControl(this);
        }

        public void SetActions(Action start, Action exit)
        {
            control.Start += start;
            control.Exit += exit;
        }
    }
}