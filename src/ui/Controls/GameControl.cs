// <copyright file="GameControl.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Gwen.Net.Control;
using Gwen.Net;
using VoxelGame.Core;
using Gwen.Net.Control.Layout;

namespace VoxelGame.UI.Controls
{
    public class GameControl : ControlBase
    {
#pragma warning disable S1450
        private readonly Label label;
        private readonly HorizontalLayout layout;
#pragma warning restore S1450

        public GameControl(UserInterface parent) : base(parent.Root)
        {
            Dock = Dock.Fill;

            layout = new HorizontalLayout(this);
            layout.VerticalAlignment = VerticalAlignment.Top;
            layout.HorizontalAlignment = HorizontalAlignment.Center;

            label = new Label(layout);
            label.Text = $"VoxelGame {Game.Version}";
            label.TextPadding = Padding.Five;
        }
    }
}