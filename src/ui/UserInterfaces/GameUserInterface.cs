﻿// <copyright file="GameUserInterface.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenToolkit.Windowing.Desktop;
using VoxelGame.Input;
using VoxelGame.UI.Controls;
using VoxelGame.UI.Providers;

namespace VoxelGame.UI.UserInterfaces
{
    /// <summary>
    ///     The user interface to use in-game.
    /// </summary>
    public class GameUserInterface : UserInterface
    {
        private IConsoleProvider? consoleProvider;

        private GameUI? control;
        private IPerformanceProvider? performanceProvider;
        private IPlayerDataProvider? playerDataProvider;
        private List<ISettingsProvider>? settingsProviders;

        /// <summary>
        ///     Creates a new game user interface.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="inputListener">The input listener.</param>
        /// <param name="drawBackground">Whether to draw background.</param>
        public GameUserInterface(GameWindow window, InputListener inputListener, bool drawBackground) : base(
            window,
            inputListener,
            drawBackground) {}

        /// <summary>
        ///     Get the in-game console.
        /// </summary>
        public ConsoleInterface? Console => control?.Console;

        /// <summary>
        ///     Get or set whether the ui is hidden.
        /// </summary>
        public bool IsHidden
        {
            get => control?.IsHidden ?? false;
            set
            {
                if (control == null) return;
                control.IsHidden = value;
            }
        }

        /// <summary>
        ///     Set the console provider.
        /// </summary>
        public void SetConsoleProvider(IConsoleProvider newConsoleProvider)
        {
            consoleProvider = newConsoleProvider;
        }

        /// <summary>
        ///     Set the settings providers.
        /// </summary>
        public void SetSettingsProviders(List<ISettingsProvider> newSettingsProviders)
        {
            settingsProviders = newSettingsProviders;
        }

        /// <summary>
        ///     Set the player data provider.
        /// </summary>
        public void SetPlayerDataProvider(IPlayerDataProvider newPlayerDataProvider)
        {
            playerDataProvider = newPlayerDataProvider;
        }

        /// <summary>
        ///     Set the performance provider.
        /// </summary>
        public void SetPerformanceProvider(IPerformanceProvider newPerformanceProvider)
        {
            performanceProvider = newPerformanceProvider;
        }


        /// <inheritdoc />
        public override void CreateControl()
        {
            Debug.Assert(settingsProviders != null);
            Debug.Assert(consoleProvider != null);
            Debug.Assert(playerDataProvider != null);
            Debug.Assert(performanceProvider != null);

            control?.Dispose();
            control = new GameUI(this, settingsProviders, consoleProvider, playerDataProvider, performanceProvider);
        }

        /// <summary>
        ///     Invoked when the world is exited.
        /// </summary>
        public event Action? WorldExit;

        /// <summary>
        ///     Invoked when any overlay is opened.
        /// </summary>
        public event Action? AnyOverlayOpen;

        /// <summary>
        ///     Invoked when any overlay is closed.
        /// </summary>
        public event Action? AnyOverlayClosed;

        /// <summary>
        ///     Update the displayed performance data.
        /// </summary>
        public void UpdatePerformanceData()
        {
            control?.UpdatePerformanceData();
        }

        /// <summary>
        ///     Update the displayed player data.
        /// </summary>
        public void UpdatePlayerData()
        {
            control?.UpdatePlayerData();
        }

        /// <summary>
        ///     Update the displayed player debug data.
        /// </summary>
        public void UpdatePlayerDebugData()
        {
            control?.UpdatePlayerDebugData();
        }

        /// <summary>
        ///     Toggle whether debug data is shown.
        /// </summary>
        public void ToggleDebugDataView()
        {
            control?.ToggleDebugDataView();
        }

        /// <summary>
        ///     Cause an escape-action.
        /// </summary>
        public void DoEscape()
        {
            control?.ToggleInGameMenu();
        }

        /// <summary>
        ///     Toggle the in-game console.
        /// </summary>
        public void ToggleConsole()
        {
            control?.ToggleConsole();
        }

        internal void DoWorldExit()
        {
            WorldExit?.Invoke();
        }

        internal void DoOverlayOpen()
        {
            AnyOverlayOpen?.Invoke();
        }

        internal void DoOverlayClose()
        {
            AnyOverlayClosed?.Invoke();
        }
    }
}
