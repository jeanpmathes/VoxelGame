// <copyright file="MainMenu.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.Utility;

namespace VoxelGame.UI.Controls
{
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    internal class MainMenu : ControlBase
    {
        private readonly FontHolder fonts;

        internal MainMenu(ControlBase parent, FontHolder fonts) : base(parent)
        {
            this.fonts = fonts;

            Dock = Dock.Fill;

            CreateContent();
        }

        private void CreateContent()
        {
            GridLayout start = new(this);
            start.SetColumnWidths(0.3f, 0.7f);
            start.SetRowHeights(1.0f);

            GridLayout bar = new(start)
            {
                Dock = Dock.Fill
            };

            MakeFiller(bar);
            VerticalLayout title = new(bar);
            CreateTitle(title);

            MakeFiller(bar);
            VerticalLayout menu = new(bar);
            CreateMenu(menu);
            MakeFiller(bar);

            bar.SetColumnWidths(1.0f);
            bar.SetRowHeights(0.05f, 0.15f, 0.55f, 0.15f, 0.10f);

            TrueRatioImagePanel image = new(start)
            {
                ImageName = Source.GetImageName("start")
            };
        }

        private static void MakeFiller(ControlBase control)
        {
            VerticalLayout filler = new(control);
        }

        private void CreateTitle(ControlBase bar)
        {
            Label title = new(bar)
            {
                Text = Language.VoxelGame,
                Font = fonts.Title,
                Alignment = Alignment.Center
            };

            Label subtitle = new(bar)
            {
                Text = GameInformation.Instance.Version,
                Font = fonts.Subtitle,
                Alignment = Alignment.Center
            };
        }

        private void CreateMenu(ControlBase menu)
        {
            Button worlds = new(menu)
            {
                Text = Language.Worlds
            };

            worlds.Clicked += (_, _) => Start?.Invoke();

            Button exit = new(menu)
            {
                Text = Language.Exit
            };

            exit.Clicked += (_, _) => Exit?.Invoke();
        }

        public event Action? Start;

        public event Action? Exit;
    }
}
