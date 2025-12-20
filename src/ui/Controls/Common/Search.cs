// <copyright file="Search.cs" company="VoxelGame">
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
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Controls.Common;

/// <summary>
///     A text input control for searching.
/// </summary>
public class Search : ControlBase
{
    /// <summary>
    ///     Creates a new instance of the <see cref="Search" /> class.
    /// </summary>
    /// <param name="parent">The parent control.</param>
    /// <param name="context">The context in which the user interface is running.</param>
    internal Search(ControlBase? parent, Context context) : base(parent)
    {
        DockLayout layout = new(this);

        ImagePanel icon = context.CreateIcon(layout, Icons.Instance.Search);
        icon.Dock = Dock.Left;

        TextBox filter = new(layout)
        {
            ToolTipText = Language.Search,
            Dock = Dock.Fill
        };

        filter.TextChanged += (_, _) => UpdateFilter(filter.Text);

        Button button = context.CreateIconButton(layout, Icons.Instance.Clear, Language.ClearInput);
        button.Dock = Dock.Right;

        button.Released += (_, _) =>
        {
            filter.Text = String.Empty;
            // Event is invoked by the text box.
        };
    }

    /// <summary>
    ///     Get the current search filter.
    ///     If no filter is set, the string is empty.
    /// </summary>
    public String Filter { get; private set; } = String.Empty;

    private void UpdateFilter(String filter)
    {
        Filter = filter;

        FilterChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Invoked when the filter changes.
    /// </summary>
    public event EventHandler? FilterChanged;
}
