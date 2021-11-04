// <copyright file="StartUserInterface.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using OpenToolkit.Windowing.Desktop;
using VoxelGame.Input;
using VoxelGame.UI.Controls;
using VoxelGame.UI.Providers;

namespace VoxelGame.UI.UserInterfaces
{
    public class StartUserInterface : UserInterface
    {
        private readonly List<ISettingsProvider> settingsProviders;
        private readonly IWorldProvider worldProvider;

        private StartUI? control;

        public StartUserInterface(GameWindow window, InputListener inputListener, IWorldProvider worldProvider,
            List<ISettingsProvider> settingsProviders, bool drawBackground) : base(
            window,
            inputListener,
            drawBackground)
        {
            this.worldProvider = worldProvider;
            this.settingsProviders = settingsProviders;
        }

        public override void CreateControl()
        {
            control?.Dispose();
            control = new StartUI(this, worldProvider, settingsProviders);
        }

        public void SetExitAction(Action exit)
        {
            if (control == null) return;

            control.Exit += exit;
        }
    }
}