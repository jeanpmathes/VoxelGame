// <copyright file="GameControl.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Controls
{
    internal class GameControl : ControlBase
    {
#pragma warning disable S4487 // Unread "private" fields should be removed
#pragma warning disable IDE0052 // Remove unread private members
        private readonly GridLayout grid;
        private readonly Label playerSelection;
        private readonly Label version;
        private readonly Label performance;
#pragma warning restore IDE0052 // Remove unread private members
#pragma warning restore S4487 // Unread "private" fields should be removed

        internal GameControl(GameUserInterface parent) : base(parent.Root)
        {
            Dock = Dock.Fill;

            grid = new GridLayout(this);
            grid.Dock = Dock.Fill;
            grid.SetColumnWidths(0.33f, 0.33f, 0.33f);
            grid.SetRowHeights(0.1f, 0.8f);

            playerSelection = BuildLabel("Block: _____");
            version = BuildLabel($"VoxelGame {GameInformation.Instance.Version}");
            performance = BuildLabel("FPS/UPS: 000/000");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "General",
            "RCS1130:Bitwise operation on enum without Flags attribute.",
            Justification = "Intended by Gwen.Net")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Critical Code Smell",
            "S3265:Non-flags enums should not be used in bitwise operations",
            Justification = "Intended by Gwen.Net")]
        private Label BuildLabel(string text)
        {
            Label label = new Label(grid);
            label.Alignment = Alignment.Top | Alignment.CenterH;
            label.Text = text;

            return label;
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