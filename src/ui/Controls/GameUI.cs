// <copyright file="GameUI.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Controls
{
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    internal class GameUI : ControlBase
    {
        private readonly InGameDisplay hud;
        private readonly GameUserInterface parent;

        private bool isMenuOpen;

        internal GameUI(GameUserInterface parent) : base(parent.Root)
        {
            this.parent = parent;
            hud = new InGameDisplay(this);
        }

        internal void SetUpdateRate(double fps, double ups)
        {
            hud.SetUpdateRate(fps, ups);
        }

        internal void SetPlayerSelection(string text)
        {
            hud.SetPlayerSelection(text);
        }

        private void CloseInGameMenu()
        {
            isMenuOpen = false;

            parent.Context.Input.AbsorbMousePress();
            parent.HandleInGameMenuClosed();

            hud.Show();
        }

        internal void OpenInGameMenu()
        {
            if (isMenuOpen) return;
            isMenuOpen = true;

            hud.Hide();

            Window menu = new(this)
            {
                StartPosition = StartPosition.CenterCanvas,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsClosable = false,
                Resizing = Resizing.None,
                IsDraggingEnabled = false
            };

            menu.MakeModal(dim: true, new Color(a: 170, r: 40, g: 40, b: 40));
            menu.Closed += (_, _) => CloseInGameMenu();

            VerticalLayout layout = new(menu)
            {
                Margin = Margin.Ten,
                Padding = Padding.Five
            };

            Button resume = new(layout)
            {
                Text = Language.Resume
            };

            resume.Pressed += (_, _) => menu.Close();

            Button settings = new(layout)
            {
                Text = Language.Settings
            };

            settings.Pressed += (_, _) => {};

            Button exit = new(layout)
            {
                Text = Language.Exit
            };

            exit.Pressed += (_, _) =>
            {
                menu.Close();
                parent.ExitWorld();
            };

            Label info = new(layout)
            {
                Text = $"{Language.VoxelGame} - {GameInformation.Instance.Version}",
                Font = parent.Context.Fonts.Subtitle
            };
        }
    }
}
