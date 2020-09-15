// <copyright file="UI.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Gwen.Net.Control;

namespace VoxelGame.UI
{
    public static class UI
    {
        /// <summary>
        /// Allows to dispose a control without having ti reference the GWEN.NET DLL.
        /// </summary>
        /// <param name="control">The control to dispose.</param>
        public static void DisposeControl(object? control)
        {
            ((ControlBase?)control)?.Dispose();
        }
    }
}