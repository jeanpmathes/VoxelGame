// <copyright file="KeyboardKeyEventArgs.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Graphics.Definition;

namespace VoxelGame.Graphics.Input.Events;

/// <summary>
///     Keyboard key event arguments.
/// </summary>
public class KeyboardKeyEventArgs
{
    /// <summary>
    ///     The key.
    /// </summary>
    public VirtualKeys Key { get; init; }
}
