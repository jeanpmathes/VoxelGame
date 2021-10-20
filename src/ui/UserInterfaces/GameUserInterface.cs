// <copyright file="GameUserInterface.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Windowing.Desktop;
using VoxelGame.UI.Controls;

namespace VoxelGame.UI.UserInterfaces
{
    public class GameUserInterface : UserInterface
    {
        private GameUI? control;

        public GameUserInterface(GameWindow window, bool drawBackground) : base(
            window,
            drawBackground) {}

        public bool IsHidden
        {
            get => control?.IsHidden ?? false;
            set
            {
                if (control == null) return;
                control.IsHidden = value;
            }
        }

        public override void CreateControl()
        {
            control?.Dispose();
            control = new GameUI(this);
        }

        public void SetUpdateRate(double fps, double ups)
        {
            control?.SetUpdateRate(fps, ups);
        }

        public void SetPlayerSelection(string category, string selection)
        {
            control?.SetPlayerSelection($"{category}: {selection}");
        }
    }
}