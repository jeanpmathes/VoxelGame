// <copyright file="ToggleButton.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Support.Definition;

namespace VoxelGame.Support.Input.Actions;

/// <summary>
///     A toggle button, which toggles the state every time it is pressed.
/// </summary>
public class ToggleButton : Button
{
    private bool hasReleased;
    private bool state;

    /// <summary>
    ///     Create a new toggle button.
    /// </summary>
    /// <param name="key">The key or button to target.</param>
    /// <param name="input">The input manager providing the input.</param>
    public ToggleButton(VirtualKeys key, InputManager input) : base(key, input) {}

    /// <summary>
    ///     Get the current button state.
    /// </summary>
    public bool State
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
    public bool Changed { get; private set; }

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
    protected override void Update(object? sender, EventArgs e)
    {
        KeyState currentState = Input.State;

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
