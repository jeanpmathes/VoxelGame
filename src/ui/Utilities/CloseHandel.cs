// <copyright file="CloseHandel.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using Gwen.Net.Control;

namespace VoxelGame.UI.Utilities;

/// <summary>
///     A handel that allows to close a window.
/// </summary>
internal sealed class CloseHandel
{
    private readonly Window window;

    internal CloseHandel(Window window)
    {
        this.window = window;
    }

    /// <summary>
    ///     Closes the window.
    /// </summary>
    internal void Close()
    {
        window.Close();
    }
}
