// <copyright file="GameControl.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Gwen.Net.Control;
using Gwen.Net;
using Gwen.Net.Control.Layout;

namespace VoxelGame.UI.Controls
{
    public class StartControl : ControlBase
    {
        private readonly VerticalLayout layout;

        private readonly Button start;
        private readonly Button exit;

        public delegate void Click();

        public StartControl(UserInterface parent) : base(parent.Root)
        {
            Dock = Dock.Fill;

            layout = new VerticalLayout(this);

            start = new Button(layout);
            start.Clicked += new GwenEventHandler<ClickedEventArgs>((_, __) => Start?.Invoke());
            start.Text = "START";

            exit = new Button(layout);
            exit.Clicked += new GwenEventHandler<ClickedEventArgs>((_, __) => Exit?.Invoke());
            exit.Text = "EXIT";
        }

        public event Click? Start;

        public event Click? Exit;

        public override void Dispose()
        {
            layout.Dispose();

            start.Dispose();
            exit.Dispose();

            base.Dispose();
        }
    }
}