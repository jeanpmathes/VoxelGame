// <copyright file="Button.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Graphics.Definition;

namespace VoxelGame.Graphics.Input.Actions;

/// <summary>
///     A button input action.
/// </summary>
public abstract class Button : InputAction
{
    /// <summary>
    ///     Create a new button.
    /// </summary>
    /// <param name="key">The trigger key.</param>
    /// <param name="input">The input manager.</param>
    protected Button(VirtualKeys key, Input input) : base(input)
    {
        Key = key;
    }

    /// <summary>
    ///     Get the used key or button.
    /// </summary>
    public VirtualKeys Key { get; private set; }

    /// <summary>
    ///     Get whether the button is pressed.
    /// </summary>
    public Boolean IsDown { get; private protected set; }

    /// <summary>
    ///     Get whether the button is up.
    /// </summary>
    public Boolean IsUp => !IsDown;

    /// <summary>
    ///     Set the binding to a different key or button.
    /// </summary>
    /// <param name="keyOrButton">The new key or button.</param>
    public void SetBinding(VirtualKeys keyOrButton)
    {
        Key = keyOrButton;
    }
}
