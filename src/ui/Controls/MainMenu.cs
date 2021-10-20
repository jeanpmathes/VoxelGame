// <copyright file="MainMenu.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net.Control;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.Utility;

namespace VoxelGame.UI.Controls
{
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    internal class MainMenu : VoxelMenu
    {
        internal MainMenu(ControlBase parent, FontHolder fonts) : base(parent, fonts)
        {
            CreateContent();
        }

        internal event Action? SelectExit;
        internal event Action? SelectWorlds;
        internal event Action? SelectSettings;

        protected override void CreateMenu(ControlBase menu)
        {
            Button worlds = new(menu)
            {
                Text = Language.Worlds
            };

            worlds.Clicked += (_, _) => SelectWorlds?.Invoke();

            Button settings = new(menu)
            {
                Text = Language.Settings
            };

            settings.Clicked += (_, _) => SelectSettings?.Invoke();

            Button exit = new(menu)
            {
                Text = Language.Exit
            };

            exit.Clicked += (_, _) => SelectExit?.Invoke();
        }

        protected override void CreateDisplay(ControlBase display)
        {
            TrueRatioImagePanel image = new(display)
            {
                ImageName = Source.GetImageName("start")
            };
        }
    }
}