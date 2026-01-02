// <copyright file="InGameUI.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core.App;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI.Controls;

/// <summary>
///     The top control managing the in-game UI.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal sealed class InGameUI : ControlBase
{
    private readonly InGameDisplay hud;
    private readonly InGameUserInterface parent;

    private readonly IPerformanceProvider performanceProvider;
    private readonly IPlayerDataProvider playerDataProvider;
    private readonly ICollection<SettingsProvider> settingsProviders;

    private Window? gameMenu;
    private Boolean isSettingsMenuOpen;

    internal InGameUI(InGameUserInterface parent, ICollection<SettingsProvider> settingsProviders,
        IConsoleProvider consoleProvider, IPlayerDataProvider playerDataProvider,
        IPerformanceProvider performanceProvider) : base(parent.Root)
    {
        this.parent = parent;
        this.settingsProviders = settingsProviders;
        this.playerDataProvider = playerDataProvider;
        this.performanceProvider = performanceProvider;

        Console = new ConsoleInterface(this, consoleProvider, parent.Context);
        hud = new InGameDisplay(this);

        Console.WindowClosed += (_, _) => parent.DoMetaControlClose();
    }

    internal ConsoleInterface Console { get; }

    private Boolean IsGameMenuOpen => gameMenu != null;

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

    internal void HandleEscape()
    {
        if (Console.IsOpen)
        {
            CloseConsole();
        }
        else
        {
            if (IsGameMenuOpen) CloseInGameMenu();
            else OpenInGameMenu();
        }
    }

    internal void HandleLossOfFocus()
    {
        if (IsGameMenuOpen || Console.IsOpen) return;

        OpenInGameMenu();
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
            DeleteOnClose = true,

            Resizing = Resizing.None,
            IsDraggingEnabled = false
        };

        parent.Context.MakeModal(gameMenu);

        VerticalLayout layout = new(gameMenu)
        {
            Margin = Margin.Ten,
            Padding = Padding.Five
        };

        Button resume = new(layout)
        {
            Text = Language.Resume
        };

        resume.Released += (_, _) => CloseInGameMenu();

        Button save = new(layout)
        {
            Text = Language.Save
        };

        save.Released += (_, _) =>
        {
            CloseInGameMenu();
            parent.DoWorldSave();
        };

        Button settings = new(layout)
        {
            Text = Language.Settings
        };

        settings.Released += (_, _) => { OpenSettings(); };

        Button exitToMenu = new(layout)
        {
            Text = Language.Exit
        };

        exitToMenu.Released += (_, _) =>
        {
            CloseInGameMenu();
            parent.DoWorldExit(exitToOS: false);
        };

        Button exitToOS = new(layout)
        {
            Text = Language.ExitToOS
        };

        exitToOS.Released += (_, _) =>
        {
            CloseInGameMenu();
            parent.DoWorldExit(exitToOS: true);
        };

        Label info = new(layout)
        {
            Text = $"{Language.VoxelGame} - {Application.Instance.Version}",
            Font = parent.Context.Fonts.Subtitle,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        Control.Used(info);

        parent.DoMetaControlOpen();
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

        Empty margins = new(settings)
        {
            Margin = Margin.Two
        };

        SettingsMenu menu = new(margins, settingsProviders, parent.Context);

        menu.Cancel += (_, _) =>
        {
            settings.Close();
            isSettingsMenuOpen = false;
        };

        isSettingsMenuOpen = true;
    }

    private void CloseInGameMenu()
    {
        if (!IsGameMenuOpen || isSettingsMenuOpen) return;

        Debug.Assert(gameMenu != null);
        gameMenu.Close();

        hud.Show();
        gameMenu = null;

        parent.DoMetaControlClose();
    }

    private void OpenConsole()
    {
        if (Console.IsOpen) return;

        Console.OpenWindow();
        parent.DoMetaControlOpen();
    }

    private void CloseConsole()
    {
        if (!Console.IsOpen) return;

        Console.CloseWindow();
        // Parent is informed when the console close event is invoked.
    }
}
