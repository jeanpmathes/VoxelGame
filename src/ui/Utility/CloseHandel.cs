// <copyright file="CloseHandel.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using Gwen.Net.Control;

namespace VoxelGame.UI.Utility
{
    /// <summary>
    ///     A handel that allows to close a window.
    /// </summary>
    internal class CloseHandel
    {
        private readonly Window window;

        public CloseHandel(Window window)
        {
            this.window = window;
        }

        /// <summary>
        ///     Closes the window.
        /// </summary>
        public void Close()
        {
            window.Close();
        }
    }
}
