// <copyright file="MouseButtonEventArgs.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Support.Definition;

namespace VoxelGame.Support.Input.Events;

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
    public bool IsPressed { get; init; }
}
