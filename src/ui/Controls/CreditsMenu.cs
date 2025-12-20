// <copyright file="CreditsMenu.cs" company="VoxelGame">
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.RichText;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI.Controls;

/// <summary>
///     The menu that shows the credits.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal sealed class CreditsMenu : StandardMenu
{
    internal CreditsMenu(ControlBase parent, Context context) : base(parent, context)
    {
        CreateContent();
    }

    internal event EventHandler? Cancel;

    protected override void CreateMenu(ControlBase menu)
    {
        Button exit = new(menu)
        {
            Text = Language.Back
        };

        exit.Released += (_, _) => Cancel?.Invoke(this, EventArgs.Empty);
    }

    protected override void CreateDisplay(ControlBase display)
    {
        TabControl tabs = new(display)
        {
            Dock = Dock.Fill
        };

        foreach ((Document credits, String name) in Context.Resources.Attributions.Select(attribution => attribution.CreateDocument(Context)))
        {
            ScrollControl page = new(tabs)
            {
                CanScrollH = false
            };

            RichLabel content = new(page)
            {
                Document = credits
            };

            Control.Used(content);

            tabs.AddPage(name, page);
        }
    }
}
