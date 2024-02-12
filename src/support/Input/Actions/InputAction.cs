// <copyright file="InputAction.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Support.Input.Actions;

/// <summary>
///     The base input action.
/// </summary>
public abstract class InputAction
{
    /// <summary>
    ///     Create a new input action.
    /// </summary>
    /// <param name="input">The input manager providing the input.</param>
    protected InputAction(Input input)
    {
        Input = input;

        input.OnInputUpdate += Update;
    }

    /// <summary>
    ///     Get the input manager providing the input.
    /// </summary>
    protected Input Input { get; }

    /// <summary>
    ///     Called every frame.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected abstract void Update(object? sender, EventArgs e);
}
