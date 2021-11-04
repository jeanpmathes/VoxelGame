// <copyright file="CloseHandel.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using Gwen.Net.Control;

namespace VoxelGame.UI.Utility
{
    public class CloseHandel
    {
        private readonly Window window;

        public CloseHandel(Window window)
        {
            this.window = window;
        }

        public void Close()
        {
            window.Close();
        }
    }
}