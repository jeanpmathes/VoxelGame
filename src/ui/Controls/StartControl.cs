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
#pragma warning disable S1450
        private readonly VerticalLayout layout;

        private readonly Button start;
        private readonly Button exit;
#pragma warning restore S1450

        public delegate void Click();

        public StartControl(UserInterface parent) : base(parent.Root)
        {
            Dock = Dock.Fill;

            layout = new VerticalLayout(this);

            start = new Button(layout);
            start.Clicked += new GwenEventHandler<ClickedEventArgs>((ControlBase __, ClickedEventArgs _) => Start?.Invoke());
            start.Text = "START";

            exit = new Button(layout);
            exit.Clicked += new GwenEventHandler<ClickedEventArgs>((ControlBase __, ClickedEventArgs _) => Exit?.Invoke());
            exit.Text = "EXIT";
        }

        public event Click? Start;

        public event Click? Exit;
    }
}