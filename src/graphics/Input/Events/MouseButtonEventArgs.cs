// <copyright file="MouseButtonEventArgs.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Graphics.Definition;

namespace VoxelGame.Graphics.Input.Events;

/// <summary>
///     Called when a mouse button is pressed or released.
/// </summary>
public class MouseButtonEventArgs : EventArgs
{
    /// <summary>
    ///     The button that was pressed or released.
    /// </summary>
    public VirtualKeys Button { get; init; }

    /// <summary>
    ///     Whether the button was pressed or released.
    /// </summary>
    public Boolean IsPressed { get; init; }
}
