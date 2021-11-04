// <copyright file="InGameDisplay.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;

namespace VoxelGame.UI.Controls
{
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    internal class InGameDisplay : ControlBase
    {
        private readonly Label performance;
        private readonly Label playerSelection;

        internal InGameDisplay(ControlBase parent) : base(parent)
        {
            Dock = Dock.Fill;

            DockLayout top = new(this)
            {
                Dock = Dock.Top,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = Margin.Ten
            };

            playerSelection = new Label(top)
            {
                Text = "Block: _____",
                Dock = Dock.Left
            };

            performance = new Label(top)
            {
                Text = "FPS/UPS: 000/000",
                Dock = Dock.Right
            };
        }

        internal void SetUpdateRate(double fps, double ups)
        {
            performance.Text = $"FPS/UPS: {fps:000}/{ups:000}";
        }

        internal void SetPlayerSelection(string text)
        {
            playerSelection.Text = text;
        }
    }
}
