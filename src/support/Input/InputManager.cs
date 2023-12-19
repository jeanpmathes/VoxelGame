// <copyright file="InputManager.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Support.Core;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Input.Devices;

namespace VoxelGame.Support.Input;

/// <summary>
///     The input manager providing input to input actions.
/// </summary>
public class InputManager
{
    /// <summary>
    ///     Create a new input manager.
    /// </summary>
    /// <param name="window">The window that receives system input.</param>
    public InputManager(Client window)
    {
        Window = window;

        Mouse = new Mouse(this);
        Listener = new InputListener(this);
    }

    /// <summary>
    ///     The current state of the keyboard.
    /// </summary>
    public KeyState State { get; private set; } = null!;

    /// <summary>
    ///     Get the mouse device.
    /// </summary>
    public Mouse Mouse { get; }

    /// <summary>
    ///     Get the input listener.
    /// </summary>
    public InputListener Listener { get; }

    internal Client Window { get; }

    /// <summary>
    ///     Update the current state.
    /// </summary>
    /// <param name="state">The new state.</param>
    public void UpdateState(KeyState state)
    {
        State = state;

        Mouse.Update();
        OnUpdate.Invoke(this, EventArgs.Empty);

        Listener.ProcessInput(state);
    }

    /// <summary>
    ///     Add a pull down, that pulls down the key until it is released.
    /// </summary>
    /// <param name="key">The key to pull down.</param>
    public void AddPullDown(VirtualKeys key)
    {
        // todo: re-implement pull down, but directly at event level and with modification of client key state:
        //  1. modify the client state so that the key is up
        //  2. wait until the actual release event, then remove the override
        //  functionality should be added to the client class
    }

    /// <summary>
    ///     Called when the input manager updates.
    /// </summary>
    public event EventHandler OnUpdate = delegate {};
}
