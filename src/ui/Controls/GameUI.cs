// <copyright file="GameUI.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Controls
{
    /// <summary>
    ///     The top control managing the in-game UI.
    /// </summary>
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    internal class GameUI : ControlBase
    {
        private readonly InGameDisplay hud;
        private readonly GameUserInterface parent;
        private readonly IPerformanceProvider performanceProvider;
        private readonly IPlayerDataProvider playerDataProvider;
        private readonly List<ISettingsProvider> settingsProviders;

        private Window? gameMenu;
        private bool isSettingsMenuOpen;

        internal GameUI(GameUserInterface parent, List<ISettingsProvider> settingsProviders,
            IConsoleProvider consoleProvider, IPlayerDataProvider playerDataProvider,
            IPerformanceProvider performanceProvider) : base(parent.Root)
        {
            this.parent = parent;
            this.settingsProviders = settingsProviders;
            this.playerDataProvider = playerDataProvider;
            this.performanceProvider = performanceProvider;

            Console = new ConsoleInterface(this, consoleProvider, parent.Context);
            hud = new InGameDisplay(this);

            Console.WindowClosed += parent.DoOverlayClose;
        }

        internal ConsoleInterface Console { get; }

        private bool IsGameMenuOpen => gameMenu != null;

        internal void UpdatePerformanceData()
        {
            hud.SetUpdateRate(performanceProvider.FPS, performanceProvider.UPS);
        }

        internal void UpdatePlayerData()
        {
            hud.SetPlayerData(playerDataProvider);
        }

        internal void UpdatePlayerDebugData()
        {
            hud.SetPlayerDebugData(playerDataProvider);
        }

        internal void ToggleDebugDataView()
        {
            hud.ToggleDebugDataView();
        }

        internal void ToggleInGameMenu()
        {
            if (Console.IsOpen) return;

            if (IsGameMenuOpen) CloseInGameMenu();
            else OpenInGameMenu();
        }

        internal void ToggleConsole()
        {
            if (IsGameMenuOpen) return;

            if (Console.IsOpen) CloseConsole();
            else OpenConsole();
        }

        private void OpenInGameMenu()
        {
            if (IsGameMenuOpen) return;

            hud.Hide();

            gameMenu = new Window(this)
            {
                StartPosition = StartPosition.CenterCanvas,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsClosable = false,
                Resizing = Resizing.None,
                IsDraggingEnabled = false
            };

            gameMenu.MakeModal(dim: true, new Color(a: 170, r: 40, g: 40, b: 40));

            VerticalLayout layout = new(gameMenu)
            {
                Margin = Margin.Ten,
                Padding = Padding.Five
            };

            Button resume = new(layout)
            {
                Text = Language.Resume
            };

            resume.Pressed += (_, _) => CloseInGameMenu();

            Button settings = new(layout)
            {
                Text = Language.Settings
            };

            settings.Pressed += (_, _) => { OpenSettings(); };

            Button exit = new(layout)
            {
                Text = Language.Exit
            };

            exit.Pressed += (_, _) =>
            {
                CloseInGameMenu();
                parent.DoWorldExit();
            };

            Label info = new(layout)
            {
                Text = $"{Language.VoxelGame} - {ApplicationInformation.Instance.Version}",
                Font = parent.Context.Fonts.Subtitle
            };

            parent.DoOverlayOpen();
        }

        private void OpenSettings()
        {
            Window settings = new(this)
            {
                Title = Language.Settings,
                IsClosable = false,
                Resizing = Resizing.None,
                IsDraggingEnabled = false
            };

            SettingsMenu menu = new(settings, settingsProviders, parent.Context);

            menu.Cancel += () =>
            {
                settings.Close();
                isSettingsMenuOpen = false;
            };

            isSettingsMenuOpen = true;
        }

        private void CloseInGameMenu()
        {
            if (!IsGameMenuOpen || isSettingsMenuOpen) return;

            parent.Context.Input.AbsorbMousePress();

            Debug.Assert(gameMenu != null);
            gameMenu.Close();

            hud.Show();
            gameMenu = null;

            parent.DoOverlayClose();
        }

        private void OpenConsole()
        {
            if (Console.IsOpen) return;

            Console.OpenWindow();
            parent.DoOverlayOpen();
        }

        private void CloseConsole()
        {
            if (!Console.IsOpen) return;

            Console.CloseWindow();
            // Parent is informed when the console close event is invoked.
        }
    }
}