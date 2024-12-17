// <copyright file="Input.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Input.Devices;
using VoxelGame.Graphics.Input.Events;

namespace VoxelGame.Graphics.Input;

/// <summary>
///     Responsible for handling input events of the client.
/// </summary>
public class Input
{
    private readonly ISet<VirtualKeys> mouseButtons = new HashSet<VirtualKeys>
    {
        VirtualKeys.LeftButton,
        VirtualKeys.RightButton,
        VirtualKeys.MiddleButton,
        VirtualKeys.ExtraButton1,
        VirtualKeys.ExtraButton2
    };

    private readonly ISet<VirtualKeys> ignoredKeys = new HashSet<VirtualKeys>();

    private readonly List<Action<VirtualKeys>> callbackListForAnyPress = new();

    internal Input(Client client)
    {
        Mouse = new Mouse(client);

        client.FocusChanged += (_, _) =>
        {
            if (!client.IsFocused) KeyState.Wipe();
        };
    }

    /// <summary>
    ///     Get the mouse device.
    /// </summary>
    public Mouse Mouse { get; }

    /// <summary>
    ///     Get the current key state.
    /// </summary>
    public KeyState KeyState { get; } = new();

    /// <summary>
    ///     Ignores a key until the physical key is released.
    /// </summary>
    /// <param name="key">The key to ignore.</param>
    public void IgnoreKeyUntilRelease(VirtualKeys key)
    {
        ignoredKeys.Add(key);
        KeyState.SetKeyState(key, down: false);
    }

    /// <summary>
    ///     Listen for any key or button press and notify the callback.
    /// </summary>
    /// <param name="callback">The callback to call.</param>
    public void ListenForAnyKeyOrButton(Action<VirtualKeys> callback)
    {
        callbackListForAnyPress.Add(callback);
    }

    /// <summary>
    ///     Called before the core game update.
    /// </summary>
    internal void PreLogicUpdate()
    {
        Mouse.LogicUpdate();

        InputUpdated?.Invoke(this, EventArgs.Empty);

        HandleAnyKeyCallbacks();
    }

    private void HandleAnyKeyCallbacks()
    {
        if (!KeyState.IsAnyKeyDown || callbackListForAnyPress.Count <= 0) return;

        VirtualKeys any = KeyState.Any;

        foreach (Action<VirtualKeys> callback in callbackListForAnyPress) callback(any);

        callbackListForAnyPress.Clear();
    }

    /// <summary>
    ///     Called after the core game update.
    /// </summary>
    internal void PostLogicUpdate()
    {
        KeyState.LogicUpdate();
    }

    internal void OnKeyDown(Byte key)
    {
        var virtualKey = (VirtualKeys) key;

        if (ignoredKeys.Contains(virtualKey)) return;

        KeyState.SetKeyState(virtualKey, down: true);
        HandleKey(virtualKey, down: true);
    }

    internal void OnKeyUp(Byte key)
    {
        var virtualKey = (VirtualKeys) key;

        if (ignoredKeys.Contains(virtualKey))
        {
            ignoredKeys.Remove(virtualKey);

            return;
        }

        KeyState.SetKeyState(virtualKey, down: false);
        HandleKey(virtualKey, down: false);
    }

    internal void OnChar(Char character)
    {
        TextInput?.Invoke(this,
            new TextInputEventArgs
            {
                Character = character
            });
    }

    internal void OnMouseMove(Int32 x, Int32 y)
    {
        Mouse.OnMouseMove((x, y));

        MouseMove?.Invoke(this,
            new MouseMoveEventArgs
            {
                Position = Mouse.Position
            });
    }

    internal void OnMouseWheel(Double delta)
    {
        MouseWheel?.Invoke(this,
            new MouseWheelEventArgs
            {
                Delta = delta
            });
    }

    private void HandleKey(VirtualKeys key, Boolean down)
    {
        if (mouseButtons.Contains(key))
        {
            MouseButton?.Invoke(this,
                new MouseButtonEventArgs
                {
                    Button = key,
                    IsPressed = down
                });
        }
        else
        {
            KeyboardKeyEventArgs args = new()
            {
                Key = key
            };

            if (down) KeyDown?.Invoke(this, args);
            else KeyUp?.Invoke(this, args);
        }
    }

    /// <summary>
    ///     Called once per frame, when the input system should update itself.
    /// </summary>
    internal event EventHandler? InputUpdated;

    /// <summary>
    ///     Called when a mouse button is pressed or released.
    /// </summary>
    public event EventHandler<MouseButtonEventArgs>? MouseButton;

    /// <summary>
    ///     Called when the mouse moves.
    /// </summary>
    public event EventHandler<MouseMoveEventArgs>? MouseMove;

    /// <summary>
    ///     Called when the mouse wheel is scrolled.
    /// </summary>
    public event EventHandler<MouseWheelEventArgs>? MouseWheel;

    /// <summary>
    ///     Called when a keyboard key is pressed.
    /// </summary>
    public event EventHandler<KeyboardKeyEventArgs>? KeyDown;

    /// <summary>
    ///     Called when a keyboard key is released.
    /// </summary>
    public event EventHandler<KeyboardKeyEventArgs>? KeyUp;

    /// <summary>
    ///     Called when a text input is received.
    /// </summary>
    public event EventHandler<TextInputEventArgs>? TextInput;
}
