// <copyright file="SettingsMenu.cs" company="VoxelGame">
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
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using Gwen.Net.RichText;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.Providers;
using VoxelGame.UI.Settings;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI.Controls;

/// <summary>
///     A menu that allows settings to be changed.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal sealed class SettingsMenu : StandardMenu
{
    private readonly List<ControlBase> categories = new();
    private readonly List<SettingsProvider> settingsProviders;
    private Int32 currentCategoryIndex = -1;

    internal SettingsMenu(ControlBase parent, IEnumerable<SettingsProvider> settingsProviders,
        Context context) : base(
        parent,
        context)
    {
        this.settingsProviders = new List<SettingsProvider>(settingsProviders);
        CreateContent();
    }

    protected override void CreateMenu(ControlBase menu)
    {
        for (var i = 0; i < settingsProviders.Count; i++)
        {
            Button category = new(menu)
            {
                Text = settingsProviders[i].Category
            };

            Int32 categoryIndex = i;

            category.Released += (_, _) =>
            {
                if (currentCategoryIndex >= 0) categories[currentCategoryIndex].Hide();

                categories[categoryIndex].Show();
                currentCategoryIndex = categoryIndex;
            };
        }

        Button back = new(menu)
        {
            Text = Language.Back
        };

        back.Released += (_, _) => Cancel?.Invoke(this, EventArgs.Empty);
    }

    protected override void CreateDisplay(ControlBase display)
    {
        DockLayout layout = new(display)
        {
            Padding = Padding.Five,
            Margin = Margin.Ten
        };

        foreach (SettingsProvider settingsProvider in settingsProviders)
        {
            GroupBox category = new(layout)
            {
                Text = settingsProvider.Category,
                Dock = Dock.Fill
            };

            GridLayout categoryLayout = new(category);
            categoryLayout.SetColumnWidths(1f);
            categoryLayout.SetRowHeights(0.10f, 0.90f);

            RichLabel description = new(categoryLayout)
            {
                Document = new Document(settingsProvider.Description)
            };

            Control.Used(description);

            ScrollControl scroll = new(categoryLayout)
            {
                AutoHideBars = true,
                CanScrollH = false,
                CanScrollV = true
            };

            Table settings = new(scroll)
            {
                ColumnCount = 1
            };

            foreach (Setting setting in settingsProvider.Settings)
                setting.CreateControl(settings.AddRow(), Context);

            categories.Add(category);
            category.Hide();

            settingsProvider.Validate();
        }
    }

    internal event EventHandler? Cancel;
}
