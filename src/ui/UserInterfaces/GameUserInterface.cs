// <copyright file="GameUserInterface.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.UI.Controls;

namespace VoxelGame.UI.UserInterfaces
{
    public class GameUserInterface : UserInterface
    {
        private GameControl control = null!;

        public bool IsHidden
        {
            get => control.IsHidden;
            set => control.IsHidden = value;
        }

        public GameUserInterface(OpenToolkit.Windowing.Desktop.GameWindow window, bool drawBackground) : base(
            window,
            drawBackground) {}

        public override void CreateControl()
        {
            control?.Dispose();
            control = new GameControl(this);
        }

        public void SetUpdateRate(double fps, double ups)
        {
            control.SetUpdateRate(fps, ups);
        }

        public void SetPlayerSelection(string category, string selection)
        {
            control.SetPlayerSelection($"{category}: {selection}");
        }
    }
}