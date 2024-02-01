﻿// <copyright file="GameUserInterface.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using VoxelGame.Support.Input;
using VoxelGame.UI.Controls;
using VoxelGame.UI.Providers;

namespace VoxelGame.UI.UserInterfaces;

/// <summary>
///     The user interface to use in-game.
/// </summary>
public class GameUserInterface : UserInterface
{
    private GameUI? control;

    private IConsoleProvider? consoleProvider;
    private IPerformanceProvider? performanceProvider;
    private IPlayerDataProvider? playerDataProvider;
    private ICollection<ISettingsProvider>? settingsProviders;

    private bool isActive;
    private bool isHidden;

    /// <summary>
    ///     Creates a new game user interface.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="scale">Provides the scale of the ui.</param>
    /// <param name="resources">The resources.</param>
    /// <param name="drawBackground">Whether to draw background.</param>
    public GameUserInterface(Input input, IScaleProvider scale, UIResources resources, bool drawBackground) : base(
        input,
        scale,
        resources,
        drawBackground) {}

    /// <summary>
    ///     Get the in-game console.
    /// </summary>
    public ConsoleInterface? Console => control?.Console;

    /// <summary>
    /// Toggle whether the UI is hidden.
    /// An active UI will not drawn when hidden.
    /// </summary>
    public void ToggleHidden()
    {
        isHidden = !isHidden;

        UpdateControlVisibility();
    }

    /// <summary>
    /// Set whether the UI is active.
    /// If the UI is not active, it will not be drawn.
    /// </summary>
    /// <param name="active">Whether the UI is active.</param>
    public void SetActive(bool active)
    {
        isActive = active;

        UpdateControlVisibility();
    }

    private void UpdateControlVisibility()
    {
        if (control == null) return;

        bool visible = isActive && !isHidden;
        control.IsHidden = !visible;
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
    public void SetSettingsProviders(ICollection<ISettingsProvider> newSettingsProviders)
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
    protected override void CreateNewControl()
    {
        Debug.Assert(settingsProviders != null);
        Debug.Assert(consoleProvider != null);
        Debug.Assert(playerDataProvider != null);
        Debug.Assert(performanceProvider != null);

        control = new GameUI(this, settingsProviders, consoleProvider, playerDataProvider, performanceProvider);

        UpdateControlVisibility();
    }

    /// <summary>
    ///     Invoked when the world is exited.
    /// </summary>
    public event EventHandler WorldExit = delegate {};

    /// <summary>
    ///     Invoked when any overlay is opened.
    /// </summary>
    public event EventHandler AnyOverlayOpen = delegate {};

    /// <summary>
    ///     Invoked when any overlay is closed.
    /// </summary>
    public event EventHandler AnyOverlayClosed = delegate {};

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
    ///     Handle an escape-action.
    /// </summary>
    public void HandleEscape()
    {
        control?.HandleEscape();
    }

    /// <summary>
    ///     Handle a loss of focus.
    /// </summary>
    public void HandleLossOfFocus()
    {
        control?.HandleLossOfFocus();
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
        WorldExit(this, EventArgs.Empty);
    }

    internal void DoOverlayOpen()
    {
        AnyOverlayOpen(this, EventArgs.Empty);
    }

    internal void DoOverlayClose()
    {
        AnyOverlayClosed(this, EventArgs.Empty);
    }
}
