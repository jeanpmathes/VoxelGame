// <copyright file="MainMenu.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utility;

namespace VoxelGame.UI.Controls;

/// <summary>
///     The main menu of the game, allowing to access the different sub-menus.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal class MainMenu : StandardMenu
{
    private Button? worlds;

    internal MainMenu(ControlBase parent, Context context) : base(parent, context)
    {
        CreateContent();
    }

    internal event EventHandler SelectExit = delegate {};
    internal event EventHandler SelectWorlds = delegate {};
    internal event EventHandler SelectSettings = delegate {};
    internal event EventHandler SelectCredits = delegate {};

    protected override void CreateMenu(ControlBase menu)
    {
        worlds = new Button(menu)
        {
            Text = Language.Worlds
        };

        worlds.Released += (_, _) => SelectWorlds(this, EventArgs.Empty);

        Button settings = new(menu)
        {
            Text = Language.Settings
        };

        settings.Released += (_, _) => SelectSettings(this, EventArgs.Empty);

        Button credits = new(menu)
        {
            Text = Language.Credits
        };

        credits.Released += (_, _) => SelectCredits(this, EventArgs.Empty);

        Button exit = new(menu)
        {
            Text = Language.Exit
        };

        exit.Released += (_, _) => SelectExit(this, EventArgs.Empty);
    }

    internal void DisableWorlds()
    {
        worlds?.Disable();
    }

    protected override void CreateDisplay(ControlBase display)
    {
        TrueRatioImagePanel image = new(display)
        {
            ImageName = Context.Resources.StartImage,
            Dock = Dock.Fill
        };

        Control.Used(image);
    }
}
