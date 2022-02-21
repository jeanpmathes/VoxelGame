// <copyright file="MainMenu.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net.Control;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utility;

namespace VoxelGame.UI.Controls
{
    /// <summary>
    ///     The main menu of the game, allowing to access the different sub-menus.
    /// </summary>
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    internal class MainMenu : StandardMenu
    {
        internal MainMenu(ControlBase parent, Context context) : base(parent, context)
        {
            CreateContent();
        }

        internal event Action? SelectExit;
        internal event Action? SelectWorlds;
        internal event Action? SelectSettings;
        internal event Action? SelectCredits;

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

            settings.Pressed += (_, _) => SelectSettings?.Invoke();

            Button credits = new(menu)
            {
                Text = Language.Credits
            };

            credits.Pressed += (_, _) => SelectCredits?.Invoke();

            Button exit = new(menu)
            {
                Text = Language.Exit
            };

            exit.Pressed += (_, _) => SelectExit?.Invoke();
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
