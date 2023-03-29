// <copyright file="TextInputEventArgs.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Support.Input.Events;

/// <summary>
///     The text input event arguments.
/// </summary>
public class TextInputEventArgs : EventArgs
{
    /// <summary>
    ///     The character.
    /// </summary>
    public char Character { get; set; }
}

