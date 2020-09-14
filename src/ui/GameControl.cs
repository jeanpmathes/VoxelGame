// <copyright file="GameControl.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Gwen.Net.Control;
using Gwen.Net;

namespace VoxelGame.UI
{
    internal class GameControl : ControlBase
    {
        private readonly Label label;

        internal GameControl(ControlBase parent) : base(parent)
        {
            Dock = Dock.Fill;

            label = new Label(this);
            label.Dock = Dock.Top;
            label.VerticalAlignment = VerticalAlignment.Stretch;
            label.Alignment = Alignment.Top | Alignment.CenterH;
            label.Text = "Hello World!";
        }

        public override void Dispose()
        {
            label.Dispose();

            base.Dispose();
        }
    }
}