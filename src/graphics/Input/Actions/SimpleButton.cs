// <copyright file="SimpleButton.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Graphics.Definition;

namespace VoxelGame.Graphics.Input.Actions;

/// <summary>
///     A simple button, that can be pushed down.
/// </summary>
public class SimpleButton : Button
{
    /// <summary>
    ///     Create a new simple button.
    /// </summary>
    /// <param name="key">The button target.</param>
    /// <param name="input">The input manager.</param>
    public SimpleButton(VirtualKeys key, Input input) : base(key, input) {}

    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <inheritdoc />
    protected override void OnInputUpdated(Object? sender, EventArgs e)
    {
        KeyState state = Input.KeyState;
        IsDown = state.IsKeyDown(Key);
    }
}
