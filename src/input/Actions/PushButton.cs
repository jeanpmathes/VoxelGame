// <copyright file="PushButton.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Input.Internal;

namespace VoxelGame.Input.Actions;

/// <summary>
///     A button that can only be pressed again when it is released.
/// </summary>
public class PushButton : Button
{
    private bool hasReleased;
    private bool pushed;

    /// <summary>
    ///     Create a new push button.
    /// </summary>
    /// <param name="keyOrButton">The key or button to target.</param>
    /// <param name="input">The input manager.</param>
    public PushButton(KeyOrButton keyOrButton, InputManager input) : base(keyOrButton, input) {}

    /// <summary>
    ///     Get whether the button is pushed this frame.
    /// </summary>
    public bool Pushed
    {
        get => pushed;
        private set
        {
            pushed = value;
            IsDown = value;
        }
    }

    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <inheritdoc />
    protected override void Update(object? sender, EventArgs e)
    {
        CombinedState state = Input.CurrentState;

        Pushed = false;

        if (hasReleased && state.IsKeyOrButtonDown(KeyOrButton))
        {
            hasReleased = false;
            Pushed = true;
        }
        else if (state.IsKeyOrButtonUp(KeyOrButton))
        {
            hasReleased = true;
        }
    }
}

