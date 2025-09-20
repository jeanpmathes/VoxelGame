// <copyright file="ToggleButton.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Graphics.Definition;

namespace VoxelGame.Graphics.Input.Actions;

/// <summary>
///     A toggle button, which toggles the state every time it is pressed.
/// </summary>
public class ToggleButton : Button
{
    private Boolean hasReleased;
    private Boolean state;

    /// <summary>
    ///     Create a new toggle button.
    /// </summary>
    /// <param name="key">The key or button to target.</param>
    /// <param name="input">The input manager providing the input.</param>
    public ToggleButton(VirtualKeys key, Input input) : base(key, input) {}

    /// <summary>
    ///     Get the current button state.
    /// </summary>
    public Boolean State
    {
        get => state;
        private set
        {
            state = value;
            IsDown = value;
        }
    }

    /// <summary>
    ///     Whether the button was toggled this frame.
    /// </summary>
    public Boolean Changed { get; private set; }

    /// <summary>
    ///     Reset the button state.
    /// </summary>
    public void Clear()
    {
        State = false;
    }

    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <inheritdoc />
    protected override void OnInputUpdated(Object? sender, EventArgs e)
    {
        KeyState currentState = Input.KeyState;

        Changed = false;

        if (hasReleased && currentState.IsKeyDown(Key))
        {
            hasReleased = false;

            State = !State;
            Changed = true;
        }
        else if (currentState.IsKeyUp(Key))
        {
            hasReleased = true;
        }
    }
}
