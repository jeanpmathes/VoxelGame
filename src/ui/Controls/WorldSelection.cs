﻿// <copyright file="WorldSelection.cs" company="VoxelGame">
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
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utility;

namespace VoxelGame.UI.Controls
{
    /// <summary>
    ///     The menu displaying worlds, allowing to select and create worlds.
    /// </summary>
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    internal class WorldSelection : StandardMenu
    {
        private readonly IWorldProvider worldProvider;

        private Window? worldCreationWindow;

        private ControlBase? worldList;

        internal WorldSelection(ControlBase parent, IWorldProvider worldProvider, Context context) : base(
            parent,
            context)
        {
            this.worldProvider = worldProvider;
            CreateContent();
        }

        protected override void CreateMenu(ControlBase menu)
        {
            Button back = new(menu)
            {
                Text = Language.Back
            };

            back.Pressed += (_, _) =>
            {
                worldCreationWindow?.Close();
                Cancel(this, EventArgs.Empty);
            };
        }

        protected override void CreateDisplay(ControlBase display)
        {
            DockLayout layout = new(display)
            {
                Padding = Padding.Five,
                Margin = Margin.Ten
            };

            GroupBox scrollBox = new(layout)
            {
                Text = Language.Worlds,
                Dock = Dock.Fill
            };

            ScrollControl scroll = new(scrollBox)
            {
                AutoHideBars = true,
                CanScrollH = false,
                CanScrollV = true,
                Dock = Dock.Fill
            };

            worldList = new VerticalLayout(scroll);
            FillWorldList();

            GroupBox options = new(layout)
            {
                Text = Language.Options,
                Dock = Dock.Bottom
            };

            Button newWorld = new(options)
            {
                Text = Language.CreateNewWorld
            };

            newWorld.Pressed += (_, _) => OpenWorldCreationWindow();
        }

        internal void Refresh()
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
                    Text = $"{info.Creation.ToLongDateString()} - {info.Creation.ToLongTimeString()}",
                    Font = Fonts.Small
                };

                Label version = new(infoPanel)
                {
                    Text = info.Version,
                    Font = Fonts.Small,
                    TextColor = ApplicationInformation.Instance.Version == info.Version ? Color.Green : Color.Red
                };

                Label file = new(infoPanel)
                {
                    Text = path,
                    Font = Fonts.Path,
                    TextColor = Color.Grey
                };

                HorizontalLayout buttons = new(layout)
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };

                Button load = new(buttons)
                {
                    ImageName = Source.GetIconName("load"),
                    ImageSize = new Size(width: 40, height: 40),
                    ToolTipText = Language.Load

                };

                Button delete = new(buttons)
                {
                    ImageName = Source.GetIconName("delete"),
                    ImageSize = new Size(width: 40, height: 40),
                    ToolTipText = Language.Delete
                };

                load.Pressed += (_, _) => worldProvider.LoadWorld(info, path);

                delete.Pressed += (_, _) => Modals.OpenBooleanModal(
                    this,
                    Language.DeleteWorldQuery,
                    () =>
                    {
                        worldProvider.DeleteWorld(path);
                        Refresh();
                    },
                    () => {});
            }
        }

        private void OpenWorldCreationWindow()
        {
            worldCreationWindow = new Window(this)
            {
                Title = Language.CreateNewWorld,
                StartPosition = StartPosition.CenterCanvas,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Resizing = Resizing.None
            };

            VerticalLayout layout = new(worldCreationWindow)
            {
                Padding = Padding.Five,
                Margin = Margin.Ten
            };

            Label info = new(layout)
            {
                Text = Language.EnterWorldName,
                HorizontalAlignment = HorizontalAlignment.Center,
                Padding = Padding.Five
            };

            TextBox name = new(layout)
            {
                Text = "Hello World",
                Padding = Padding.Five
            };

            Button create = new(layout)
            {
                Text = Language.Create,
                Padding = Padding.Five
            };

            name.TextChanged += (_, _) => ValidateInput(out _);
            create.Pressed += (_, _) => CreateWorld();

            void ValidateInput(out bool isValid)
            {
                string input = name.Text;
                isValid = worldProvider.IsWorldNameValid(input);

                name.TextColor = isValid ? Color.White : Color.Red;

                create.IsDisabled = !isValid;
                create.UpdateColors();
            }

            void CreateWorld()
            {
                ValidateInput(out bool isValid);

                if (isValid) worldProvider.CreateWorld(name.Text);
            }
        }

        internal event EventHandler Cancel = delegate {};
    }
}
