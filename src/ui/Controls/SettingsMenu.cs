// <copyright file="SettingsMenu.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

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

namespace VoxelGame.UI.Controls
{
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    public class SettingsMenu : StandardMenu
    {
        private readonly List<ControlBase> categories = new();
        private readonly List<ISettingsProvider> settingsProviders;
        private int currentCategoryIndex = -1;

        internal SettingsMenu(ControlBase parent, List<ISettingsProvider> settingsProviders, Context context) : base(
            parent,
            context)
        {
            this.settingsProviders = settingsProviders;
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

                category.Pressed += (_, _) =>
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

            back.Pressed += (_, _) => Cancel?.Invoke();
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

        public event Action? Cancel;
    }
}