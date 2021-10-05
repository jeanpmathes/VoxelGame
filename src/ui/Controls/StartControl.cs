// <copyright file="GameControl.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Controls
{
    internal class StartControl : ControlBase
    {
        internal StartControl(StartUserInterface parent) : base(parent.Root)
        {
            Dock = Dock.Fill;

            layout = new VerticalLayout(this);

            start = new Button(layout);

            start.Clicked += (_, _) => Start?.Invoke();

            start.Text = "START";

            exit = new Button(layout);

            exit.Clicked += (_, _) => Exit?.Invoke();

            exit.Text = "EXIT";
        }

        public event Action? Start;

        public event Action? Exit;
#pragma warning disable S1450
        private readonly VerticalLayout layout;

        private readonly Button start;
        private readonly Button exit;
#pragma warning restore S1450
    }
}