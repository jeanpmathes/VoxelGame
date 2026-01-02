// <copyright file="WorldList.cs" company="VoxelGame">
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI.Controls.Worlds;

/// <summary>
///     Display a list of worlds.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
public class WorldList : ControlBase
{
    private readonly Context context;
    private readonly WorldSelection menu;
    private readonly ControlBase tableParent;

    private readonly IWorldProvider worldProvider;

    private Table? table;

    /// <summary>
    ///     Create a new world list.
    /// </summary>
    /// <param name="parent">The parent control.</param>
    /// <param name="worldProvider">The world provider to use.</param>
    /// <param name="context">The context in which the control is created.</param>
    /// <param name="menu">The world selection menu that this list is part of.</param>
    internal WorldList(ControlBase parent, IWorldProvider worldProvider, Context context, WorldSelection menu)
        : base(parent)
    {
        this.menu = menu;
        tableParent = new VerticalLayout(this);

        this.worldProvider = worldProvider;
        this.context = context;

        BuildList();
    }

    private Table CreateWorldTable()
    {
        table?.Parent?.RemoveChild(table, dispose: true);

        table = new Table(tableParent)
        {
            ColumnCount = 1,
            AlternateColor = true
        };

        table.Disable();

        return table;
    }

    /// <summary>
    ///     Build a list of worlds. This will remove any previous content.
    /// </summary>
    /// <param name="filter">A filter to apply. Only worlds matching the filter will be displayed.</param>
    public void BuildList(String filter = "")
    {
        Table worlds = CreateWorldTable();

        IEnumerable<IWorldProvider.IWorldInfo> entries = worldProvider.Worlds;

        if (filter.Length > 0) entries = entries.Where(entry => entry.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase));

        entries = entries
            .OrderByDescending(entry => entry.IsFavorite)
            .ThenByDescending(entry => entry.DateTimeOfLastLoad ?? DateTime.MaxValue);

        foreach (IWorldProvider.IWorldInfo data in entries)
        {
            WorldElement element = new(worlds, data, worldProvider, context, menu)
            {
                Margin = Margin.Five
            };

            Control.Used(element);
        }

        if (!worldProvider.Worlds.Any()) BuildText(Language.NoWorldsFound);
    }

    /// <summary>
    ///     Build a text display. This will remove any previous content.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="isError">Whether the text is an error message.</param>
    public void BuildText(String text, Boolean isError = false)
    {
        Table worlds = CreateWorldTable();
        TableRow row = worlds.AddRow();

        Label label = new(worlds.Parent!)
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextColor = isError ? Colors.Error : Colors.Secondary
        };

        row.SetCellContents(column: 0, label);
    }
}
