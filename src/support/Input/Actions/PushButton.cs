// <copyright file="PushButton.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Support.Definition;

namespace VoxelGame.Support.Input.Actions;

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
    /// <param name="key">The key or button to target.</param>
    /// <param name="input">The input manager.</param>
    public PushButton(VirtualKeys key, Input input) : base(key, input) {}

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
        KeyState state = Input.KeyState;

        Pushed = false;

        if (hasReleased && state.IsKeyDown(Key))
        {
            hasReleased = false;
            Pushed = true;
        }
        else if (state.IsKeyUp(Key))
        {
            hasReleased = true;
        }
    }
}
