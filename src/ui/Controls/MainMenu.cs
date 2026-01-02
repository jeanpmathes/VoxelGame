// <copyright file="MainMenu.cs" company="VoxelGame">
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
internal sealed class MainMenu : StandardMenu
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
