// <copyright file="GameUserInterface.cs" company="VoxelGame">
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
    public class GameUserInterface : UserInterface
    {
        private IConsoleProvider? consoleProvider;

        private GameUI? control;
        private IPerformanceProvider? performanceProvider;
        private IPlayerDataProvider? playerDataProvider;
        private List<ISettingsProvider>? settingsProviders;

        public GameUserInterface(GameWindow window, InputListener inputListener, bool drawBackground) : base(
            window,
            inputListener,
            drawBackground) {}

        public ConsoleInterface? Console => control?.Console;

        public bool IsHidden
        {
            get => control?.IsHidden ?? false;
            set
            {
                if (control == null) return;
                control.IsHidden = value;
            }
        }

        public void SetConsoleProvider(IConsoleProvider newConsoleProvider)
        {
            consoleProvider = newConsoleProvider;
        }

        public void SetSettingsProviders(List<ISettingsProvider> newSettingsProviders)
        {
            settingsProviders = newSettingsProviders;
        }

        public void SetPlayerDataProvider(IPlayerDataProvider newPlayerDataProvider)
        {
            playerDataProvider = newPlayerDataProvider;
        }

        public void SetPerformanceProvider(IPerformanceProvider newPerformanceProvider)
        {
            performanceProvider = newPerformanceProvider;
        }

        public override void CreateControl()
        {
            Debug.Assert(settingsProviders != null);
            Debug.Assert(consoleProvider != null);
            Debug.Assert(playerDataProvider != null);
            Debug.Assert(performanceProvider != null);

            control?.Dispose();
            control = new GameUI(this, settingsProviders, consoleProvider, playerDataProvider, performanceProvider);
        }

        public event Action? WorldExit;

        public event Action? AnyOverlayOpen;
        public event Action? AnyOverlayClosed;

        public void UpdatePerformanceData()
        {
            control?.UpdatePerformanceData();
        }

        public void UpdatePlayerData()
        {
            control?.UpdatePlayerData();
        }

        public void UpdatePlayerDebugData()
        {
            control?.UpdatePlayerDebugData();
        }

        public void ToggleDebugDataView()
        {
            control?.ToggleDebugDataView();
        }

        public void DoEscape()
        {
            control?.ToggleInGameMenu();
        }

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