// <copyright file="WorldSelection.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.Providers;
using VoxelGame.UI.Utility;

namespace VoxelGame.UI.Controls
{
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    public class WorldSelection : StartMenu
    {
        private readonly IWorldProvider worldProvider;

        private ControlBase? worldList;

        internal WorldSelection(ControlBase parent, IWorldProvider worldProvider, FontHolder fonts) : base(
            parent,
            fonts)
        {
            this.worldProvider = worldProvider;
            FillWorldList();
        }

        protected override void CreateMenu(ControlBase menu)
        {
            Button back = new(menu)
            {
                Text = Language.Back
            };

            back.Clicked += (_, _) => Cancel?.Invoke();
        }

        protected override void CreateDisplay(ControlBase display)
        {
            GroupBox scrollBox = new(display)
            {
                Text = Language.Worlds,
                Padding = Padding.Five,
                Margin = Margin.Ten
            };

            ScrollControl scroll = new(scrollBox)
            {
                AutoHideBars = true,
                CanScrollH = false,
                CanScrollV = true,
                Dock = Dock.Fill
            };

            worldList = new VerticalLayout(scroll);

        }

        public void Refresh()
        {
            worldProvider.Refresh();
            FillWorldList();
        }

        private void FillWorldList()
        {
            Debug.Assert(worldList != null);

            worldList.DeleteAllChildren();

            foreach ((WorldInformation info, string path) in worldProvider.Worlds)
            {
                GroupBox world = new(worldList)
                {
                    Text = info.Name
                };

                DockLayout layout = new(world);

                VerticalLayout infoPanel = new(layout)
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };

                Label date = new(infoPanel)
                {
                    Text = info.Creation.ToShortDateString(),
                    Font = Fonts.Small
                };

                Label version = new(infoPanel)
                {
                    Text = info.Version,
                    Font = Fonts.Small,
                    TextColor = GameInformation.Instance.Version == info.Version ? Color.Green : Color.Red
                };

                Label file = new(infoPanel)
                {
                    Text = path,
                    Font = Fonts.Path,
                    TextColor = Color.Grey
                };

                Button load = new(layout)
                {
                    ImageName = Source.GetIconName("load"),
                    ImageSize = new Size(width: 40, height: 40),
                    ToolTipText = Language.Load,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };

                load.Pressed += (_, _) => { worldProvider.LoadWorld(info, path); };
            }
        }

        public event Action? Cancel;
    }
}
