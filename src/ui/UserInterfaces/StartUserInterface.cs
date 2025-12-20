// <copyright file="StartUserInterface.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Graphics.Input;
using VoxelGame.UI.Controls;
using VoxelGame.UI.Providers;

namespace VoxelGame.UI.UserInterfaces;

/// <summary>
///     The user interface used for the start menu.
/// </summary>
public class StartUserInterface : UserInterface
{
    private readonly ICollection<SettingsProvider> settingsProviders;
    private readonly IWorldProvider worldProvider;

    private StartUI? control;

    /// <summary>
    ///     Creates a new start user interface.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="scale">Provides the scale of the ui.</param>
    /// <param name="worldProvider">The world provider.</param>
    /// <param name="settingsProviders">The settings providers.</param>
    /// <param name="resources">The resources.</param>
    /// <param name="drawBackground">Whether to draw the ui background.</param>
    public StartUserInterface(Input input, IScaleProvider scale, IWorldProvider worldProvider,
        ICollection<SettingsProvider> settingsProviders, UserInterfaceResources resources, Boolean drawBackground) : base(
        input,
        scale,
        resources,
        drawBackground)
    {
        this.worldProvider = worldProvider;
        this.settingsProviders = settingsProviders;
    }

    /// <inheritdoc />
    protected override void CreateNewControl()
    {
        control = new StartUI(this, worldProvider, settingsProviders);
    }

    /// <summary>
    ///     Set the action to invoke when the user requests a game exit.
    /// </summary>
    /// <param name="exit">The action to invoke.</param>
    public void SetExitAction(Action exit)
    {
        Debug.Assert(control != null);

        control.Exit += (_, _) => exit();
    }

    /// <summary>
    ///     Set information about the loading results of the resource loading process.
    /// </summary>
    /// <param name="resources">The resources that are missing.</param>
    /// <param name="isCriticalMissing">Whether a critical resource is missing, preventing the game from starting.</param>
    public void PresentResourceLoadingIssueReport(Property resources, Boolean isCriticalMissing)
    {
        Debug.Assert(control != null);

        control.OpenMissingResourcesWindow(resources);

        if (isCriticalMissing) control.DisableWorldSelection();
    }
}
