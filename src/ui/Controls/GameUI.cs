// <copyright file="GameUI.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI.Controls;

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
    private readonly ICollection<ISettingsProvider> settingsProviders;

    private Window? gameMenu;
    private bool isSettingsMenuOpen;

    internal GameUI(GameUserInterface parent, ICollection<ISettingsProvider> settingsProviders,
        IConsoleProvider consoleProvider, IPlayerDataProvider playerDataProvider,
        IPerformanceProvider performanceProvider) : base(parent.Root)
    {
        this.parent = parent;
        this.settingsProviders = settingsProviders;
        this.playerDataProvider = playerDataProvider;
        this.performanceProvider = performanceProvider;

        Console = new ConsoleInterface(this, consoleProvider, parent.Context);
        hud = new InGameDisplay(this);

        Console.WindowClosed += (_, _) => parent.DoOverlayClose();
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

        resume.Released += (_, _) => CloseInGameMenu();

        Button settings = new(layout)
        {
            Text = Language.Settings
        };

        settings.Released += (_, _) => { OpenSettings(); };

        Button exit = new(layout)
        {
            Text = Language.Exit
        };

        exit.Released += (_, _) =>
        {
            CloseInGameMenu();
            parent.DoWorldExit();
        };

        Label info = new(layout)
        {
            Text = $"{Language.VoxelGame} - {ApplicationInformation.Instance.Version}",
            Font = parent.Context.Fonts.Subtitle,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        Control.Used(info);

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
