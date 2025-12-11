// <copyright file="IInputControl.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Client.Inputs;

namespace VoxelGame.Client.Scenes;

/// <summary>
///     Controls retrieving and handling input.
/// </summary>
public interface IInputControl
{
    /// <summary>
    ///     Whether it is OK to handle game input currently.
    /// </summary>
    Boolean CanHandleGameInput { get; }

    /// <summary>
    ///     Whether it is OK to handle meta input currently.
    /// </summary>
    Boolean CanHandleMetaInput { get; }

    /// <summary>
    ///     Get the keybinds to use for input handling.
    /// </summary>
    KeybindManager Keybinds { get; }
}
