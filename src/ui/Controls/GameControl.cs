// <copyright file="GameControl.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Gwen.Net.Control;
using Gwen.Net;
using VoxelGame.Core;

namespace VoxelGame.UI.Controls
{
    public class GameControl : ControlBase
    {
        private readonly Label label;

        public GameControl(UserInterface parent) : base(parent.Root)
        {
            Dock = Dock.Fill;

            label = new Label(this);
            label.Dock = Dock.Top;
            label.VerticalAlignment = VerticalAlignment.Stretch;
            label.Alignment = Alignment.Top | Alignment.CenterH;
            label.Text = $"VoxelGame {Game.Version}";
        }

        public override void Dispose()
        {
            label.Dispose();

            base.Dispose();
        }
    }
}