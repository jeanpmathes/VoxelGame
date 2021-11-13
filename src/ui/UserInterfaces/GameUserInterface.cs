﻿// <copyright file="GameUserInterface.cs" company="VoxelGame">
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
    public class GameUserInterface : UserInterface
    {
        private readonly IConsoleProvider consoleProvider;
        private readonly List<ISettingsProvider> settingsProviders;

        private GameUI? control;

        public GameUserInterface(GameWindow window,
            InputListener inputListener, List<ISettingsProvider> settingsProviders, IConsoleProvider consoleProvider,
            bool drawBackground) : base(
            window,
            inputListener,
            drawBackground)
        {
            this.settingsProviders = settingsProviders;
            this.consoleProvider = consoleProvider;
        }

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
            control = new GameUI(this, settingsProviders, consoleProvider);
        }

        public event Action? WorldExit;

        public event Action? AnyOverlayOpen;
        public event Action? AnyOverlayClosed;

        public void SetUpdateRate(double fps, double ups)
        {
            control?.SetUpdateRate(fps, ups);
        }

        public void SetPlayerSelection(string category, string selection)
        {
            control?.SetPlayerSelection($"{category}: {selection}");
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