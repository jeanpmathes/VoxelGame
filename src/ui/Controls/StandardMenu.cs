// <copyright file="StandardMenu.cs" company="VoxelGame">
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

using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core.App;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI.Controls;

/// <summary>
///     An abstract menu that can be used to create menus with a standard layout.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal abstract class StandardMenu : ControlBase
{
    private protected StandardMenu(ControlBase parent, Context context) : base(parent)
    {
        Context = context;
        Dock = Dock.Fill;
    }

    internal Context Context { get; }

    public override void Show()
    {
        base.Show();

        OnOpen();
    }

    protected virtual void OnOpen() {}

    protected void CreateContent()
    {
        Dock = Dock.Fill;

        GridLayout grid = new(this);
        grid.SetColumnWidths(0.3f, 0.7f);
        grid.SetRowHeights(1.0f);

        GridLayout bar = new(grid)
        {
            Dock = Dock.Fill,
            Margin = Margin.Five
        };

        MakeFiller(bar);
        VerticalLayout title = new(bar);
        CreateTitle(title);

        MakeFiller(bar);
        VerticalLayout menu = new(bar);
        CreateMenu(menu);
        MakeFiller(bar);

        bar.SetColumnWidths(1.0f);
        bar.SetRowHeights(0.05f, 0.15f, 0.55f, 0.20f, 0.05f);

        CreateDisplay(grid);
    }

    private static void MakeFiller(ControlBase control)
    {
        VerticalLayout filler = new(control);

        Control.Used(filler);
    }

    private void CreateTitle(ControlBase bar)
    {
        Label title = new(bar)
        {
            Text = Language.VoxelGame,
            Font = Context.Fonts.Title,
            Alignment = Alignment.Center
        };

        Control.Used(title);

        Label subtitle = new(bar)
        {
            Text = Application.Instance.Version.ToString(),
            Font = Context.Fonts.Subtitle,
            Alignment = Alignment.Center
        };

        Control.Used(subtitle);
    }

    protected abstract void CreateMenu(ControlBase menu);

    protected abstract void CreateDisplay(ControlBase display);
}
