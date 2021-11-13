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
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    internal class GameUI : ControlBase
    {
        private const int MaxInputLogLength = 30;
        private readonly InGameDisplay hud;

        private readonly LinkedList<string> inputLog = new();
        private readonly GameUserInterface parent;
        private readonly List<ISettingsProvider> settingsProviders;

        private Window? console;

        private Window? gameMenu;
        private bool isSettingsMenuOpen;

        internal GameUI(GameUserInterface parent, List<ISettingsProvider> settingsProviders) : base(parent.Root)
        {
            this.parent = parent;
            this.settingsProviders = settingsProviders;

            hud = new InGameDisplay(this);
        }

        private bool IsGameMenuOpen => gameMenu != null;

        private bool IsConsoleOpen => console != null;

        internal void SetUpdateRate(double fps, double ups)
        {
            hud.SetUpdateRate(fps, ups);
        }

        internal void SetPlayerSelection(string text)
        {
            hud.SetPlayerSelection(text);
        }

        internal void ToggleInGameMenu()
        {
            if (IsConsoleOpen) return;

            if (IsGameMenuOpen) CloseInGameMenu();
            else OpenInGameMenu();
        }

        internal void ToggleConsole()
        {
            if (IsGameMenuOpen) return;

            if (IsConsoleOpen) CloseConsole();
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
                Text = $"{Language.VoxelGame} - {GameInformation.Instance.Version}",
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
            if (IsConsoleOpen) return;

            hud.Hide();

            console = new Window(this)
            {
                StartPosition = StartPosition.Manual,
                Position = new Point(x: 0, y: 0),
                Size = new Size(width: 900, height: 400),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Resizing = Resizing.None,
                IsDraggingEnabled = false
            };

            console.Closed += (_, _) => CloseConsole(closeConsoleWindow: false);
            console.MakeModal(dim: true, new Color(a: 170, r: 40, g: 40, b: 40));

            GridLayout layout = new(console)
            {
                Dock = Dock.Fill,
                Margin = Margin.Ten
            };

            layout.SetColumnWidths(1f);
            layout.SetRowHeights(0.9f, 0.1f);

            ListBox consoleOutput = new(layout)
            {
                AlternateColor = false,
                CanScrollH = false,
                CanScrollV = true,
                Dock = Dock.Fill,
                Margin = Margin.One
            };

            DockLayout bottomBar = new(layout)
            {
                Margin = Margin.One
            };

            TextBox consoleInput = new(bottomBar)
            {
                Dock = Dock.Fill
            };

            Button consoleSubmit = new(bottomBar)
            {
                Dock = Dock.Right,
                Text = Language.Submit
            };

            consoleInput.SubmitPressed += (_, _) => Submit();
            consoleSubmit.Pressed += (_, _) => Submit();

            parent.DoOverlayOpen();

            foreach (string input in inputLog) consoleOutput.AddRow(input);

            consoleOutput.ScrollToBottom();

            void Submit()
            {
                string input = consoleInput.Text;
                consoleInput.SetText("");

                inputLog.AddLast(input);
                if (inputLog.Count > MaxInputLogLength) inputLog.RemoveFirst();

                consoleOutput.AddRow(input);
                consoleOutput.ScrollToBottom();
            }
        }

        private void CloseConsole(bool closeConsoleWindow = true)
        {
            if (!IsConsoleOpen) return;

            parent.Context.Input.AbsorbMousePress();

            Debug.Assert(console != null);
            if (closeConsoleWindow) console.Close();

            hud.Show();
            console = null;

            parent.DoOverlayClose();
        }
    }
}
