// <copyright file="StartUserInterface.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using VoxelGame.Core.Collections;
using VoxelGame.Support.Input;
using VoxelGame.UI.Controls;
using VoxelGame.UI.Providers;

namespace VoxelGame.UI.UserInterfaces;

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
    /// <param name="input">The input.</param>
    /// <param name="worldProvider">The world provider.</param>
    /// <param name="settingsProviders">The settings providers.</param>
    /// <param name="resources">The resources.</param>
    /// <param name="drawBackground">Whether to draw the ui background.</param>
    public StartUserInterface(Input input, IWorldProvider worldProvider,
        ICollection<ISettingsProvider> settingsProviders, UIResources resources, bool drawBackground) : base(
        input,
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
    public void PresentResourceLoadingFailure(Tree<string> resources, bool isCriticalMissing)
    {
        Debug.Assert(control != null);

        control.OpenMissingResourcesWindow(resources);

        if (isCriticalMissing) control.DisableWorldSelection();
    }
}
