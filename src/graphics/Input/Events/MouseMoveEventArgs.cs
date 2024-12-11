// <copyright file="MouseMoveEventArgs.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;

namespace VoxelGame.Graphics.Input.Events;

/// <summary>
///     Mouse move event arguments.
/// </summary>
public class MouseMoveEventArgs : EventArgs
{
    /// <summary>
    ///     The new position of the mouse.
    /// </summary>
    public Vector2 Position { get; init; }
}
