// <copyright file="KeyboardKeyEventArgs.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Support.Definition;

namespace VoxelGame.Support.Input.Events;

/// <summary>
///     Keyboard key event arguments.
/// </summary>
public class KeyboardKeyEventArgs
{
    /// <summary>
    ///     The key.
    /// </summary>
    public VirtualKeys Key { get; set; }
}

