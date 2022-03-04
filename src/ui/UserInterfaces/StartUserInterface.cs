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
    /// <summary>
    ///     The user interface used for the start menu.
    /// </summary>
    public class StartUserInterface : UserInterface
    {
        private readonly ICollection<ISettingsProvider> settingsProviders;
        private readonly IWorldProvider worldProvider;

        private StartUI? control;

        /// <summary>
        ///     Creates a new start user interface.
        /// </summary>
        /// <param name="window">The game window.</param>
        /// <param name="inputListener">The input listener.</param>
        /// <param name="worldProvider">The world provider.</param>
        /// <param name="settingsProviders">The settings providers.</param>
        /// <param name="drawBackground">Whether to draw the ui background.</param>
        public StartUserInterface(GameWindow window, InputListener inputListener, IWorldProvider worldProvider,
            ICollection<ISettingsProvider> settingsProviders, bool drawBackground) : base(
            window,
            inputListener,
            drawBackground)
        {
            this.worldProvider = worldProvider;
            this.settingsProviders = settingsProviders;
        }

        /// <inheritdoc />
        public override void CreateControl()
        {
            control?.Dispose();
            control = new StartUI(this, worldProvider, settingsProviders);
        }

        /// <summary>
        ///     Set the action to invoke when the user requests a game exit.
        /// </summary>
        /// <param name="exit">The action to invoke.</param>
        public void SetExitAction(Action exit)
        {
            if (control == null) return;

            control.Exit += (_, _) => exit();
        }
    }
}
