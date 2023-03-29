// <copyright file="MouseWheelEventArgs.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Support.Input.Events;

/// <summary>
///     Mouse wheel event arguments.
/// </summary>
public class MouseWheelEventArgs : EventArgs
{
    /// <summary>
    ///     The mouse wheel delta.
    /// </summary>
    public int Delta { get; }
}

