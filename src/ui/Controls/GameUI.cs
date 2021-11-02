// <copyright file="GameUI.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics.CodeAnalysis;
using Gwen.Net.Control;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Controls
{
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    internal class GameUI : ControlBase
    {
        private readonly InGameDisplay hud;

        internal GameUI(GameUserInterface parent) : base(parent.Root)
        {
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
    }
}
