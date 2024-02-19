// <copyright file="SettingsMenu.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
internal class SettingsMenu : StandardMenu
{
    private readonly List<ControlBase> categories = new();
    private readonly List<ISettingsProvider> settingsProviders;
    private int currentCategoryIndex = -1;

    internal SettingsMenu(ControlBase parent, IEnumerable<ISettingsProvider> settingsProviders,
        Context context) : base(
        parent,
        context)
    {
        this.settingsProviders = new List<ISettingsProvider>(settingsProviders);
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

            int categoryIndex = i;

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

        back.Released += (_, _) => Cancel(this, EventArgs.Empty);
    }

    protected override void CreateDisplay(ControlBase display)
    {
        DockLayout layout = new(display)
        {
            Padding = Padding.Five,
            Margin = Margin.Ten
        };

        foreach (ISettingsProvider settingsProvider in settingsProviders)
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

            VerticalLayout settings = new(scroll);

            foreach (Setting setting in settingsProvider.Settings) setting.CreateControl(settings, Context);

            categories.Add(category);
            category.Hide();

            settingsProvider.Validate();
        }
    }

    internal event EventHandler Cancel = delegate {};
}
