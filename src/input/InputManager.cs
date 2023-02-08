// <copyright file="InputManager.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelGame.Input.Devices;
using VoxelGame.Input.Internal;

namespace VoxelGame.Input;

/// <summary>
///     The input manager providing input to input actions.
/// </summary>
public class InputManager
{
    private readonly Dictionary<KeyOrButton, bool> overrides = new();
    private readonly HashSet<KeyOrButton> pullDowns = new();

    /// <summary>
    ///     Create a new input manager.
    /// </summary>
    /// <param name="window">The window that receives system input.</param>
    public InputManager(GameWindow window)
    {
        Window = window;

        Mouse = new Mouse(this);
        Listener = new InputListener(this);
    }

    /// <summary>
    ///     Get the mouse device.
    /// </summary>
    public Mouse Mouse { get; }

    /// <summary>
    ///     Get the input listener.
    /// </summary>
    public InputListener Listener { get; }

    internal GameWindow Window { get; }

    internal CombinedState CurrentState { get; private set; }

    /// <summary>
    ///     Update the current state.
    /// </summary>
    /// <param name="keyboard">The state of the keyboard.</param>
    /// <param name="mouse">The state of the mouse.</param>
    public void UpdateState(KeyboardState keyboard, MouseState mouse)
    {
        SetOverrides(new CombinedState(keyboard, mouse, new Dictionary<KeyOrButton, bool>()));
        CurrentState = new CombinedState(keyboard, mouse, overrides);

        Mouse.Update();
        OnUpdate.Invoke(this, EventArgs.Empty);

        Listener.ProcessInput(CurrentState);
    }

    private void SetOverrides(CombinedState actualState)
    {
        pullDowns.RemoveWhere(
            keyOrButton =>
            {
                if (actualState.IsKeyOrButtonDown(keyOrButton)) return false;

                overrides.Remove(keyOrButton);

                return true;
            });
    }

    /// <summary>
    ///     Add a pull down, that pulls down the key until it is released.
    /// </summary>
    /// <param name="keyOrButton">The key or button to pull down.</param>
    public void AddPullDown(KeyOrButton keyOrButton)
    {
        pullDowns.Add(keyOrButton);
        overrides[keyOrButton] = false;
    }

    /// <summary>
    ///     Called when the input manager updates.
    /// </summary>
    public event EventHandler OnUpdate = delegate {};
}

