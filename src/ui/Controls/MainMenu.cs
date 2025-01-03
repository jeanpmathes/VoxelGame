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
using VoxelGame.UI.Controls.Common;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;

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

    internal event EventHandler? SelectExit;
    internal event EventHandler? SelectWorlds;
    internal event EventHandler? SelectSettings;
    internal event EventHandler? SelectCredits;

    protected override void CreateMenu(ControlBase menu)
    {
        worlds = new Button(menu)
        {
            Text = Language.Worlds
        };

        worlds.Released += (_, _) => SelectWorlds?.Invoke(this, EventArgs.Empty);

        Button settings = new(menu)
        {
            Text = Language.Settings
        };

        settings.Released += (_, _) => SelectSettings?.Invoke(this, EventArgs.Empty);

        Button credits = new(menu)
        {
            Text = Language.Credits
        };

        credits.Released += (_, _) => SelectCredits?.Invoke(this, EventArgs.Empty);

        Button exit = new(menu)
        {
            Text = Language.Exit
        };

        exit.Released += (_, _) => SelectExit?.Invoke(this, EventArgs.Empty);
    }

    internal void DisableWorlds()
    {
        worlds?.Disable();
    }

    protected override void CreateDisplay(ControlBase display)
    {
        TrueRatioImagePanel image = new(display)
        {
            ImageName = Icons.Instance.StartImage,
            Dock = Dock.Fill
        };

        Control.Used(image);
    }
}
