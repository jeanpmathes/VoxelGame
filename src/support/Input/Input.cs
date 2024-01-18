// <copyright file="Input.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Support.Core;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Input.Devices;
using VoxelGame.Support.Input.Events;

namespace VoxelGame.Support.Input;

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
    internal void PreUpdate()
    {
        Mouse.Update();

        OnInputUpdate(this, EventArgs.Empty);

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
    internal void PostUpdate()
    {
        KeyState.Update();
    }

    internal void OnKeyDown(byte key)
    {
        var virtualKey = (VirtualKeys) key;

        if (ignoredKeys.Contains(virtualKey)) return;

        KeyState.SetKeyState(virtualKey, down: true);
        HandleKey(virtualKey, down: true);
    }

    internal void OnKeyUp(byte key)
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

    internal void OnChar(char character)
    {
        TextInput(this,
            new TextInputEventArgs
            {
                Character = character
            });
    }

    internal void OnMouseMove(int x, int y)
    {
        Mouse.OnMouseMove((x, y));

        MouseMove(this,
            new MouseMoveEventArgs
            {
                Position = Mouse.Position
            });
    }

    internal void OnMouseWheel(double delta)
    {
        MouseWheel(this,
            new MouseWheelEventArgs
            {
                Delta = delta
            });
    }

    private void HandleKey(VirtualKeys key, bool down)
    {
        if (mouseButtons.Contains(key))
        {
            MouseButton(this,
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

            if (down) KeyDown(this, args);
            else KeyUp(this, args);
        }
    }

    /// <summary>
    ///     Called once per frame, when the input system should update itself.
    /// </summary>
    internal event EventHandler OnInputUpdate = delegate {};

    /// <summary>
    ///     Called when a mouse button is pressed or released.
    /// </summary>
    public event EventHandler<MouseButtonEventArgs> MouseButton = delegate {};

    /// <summary>
    ///     Called when the mouse moves.
    /// </summary>
    public event EventHandler<MouseMoveEventArgs> MouseMove = delegate {};

    /// <summary>
    ///     Called when the mouse wheel is scrolled.
    /// </summary>
    public event EventHandler<MouseWheelEventArgs> MouseWheel = delegate {};

    /// <summary>
    ///     Called when a keyboard key is pressed.
    /// </summary>
    public event EventHandler<KeyboardKeyEventArgs> KeyDown = delegate {};

    /// <summary>
    ///     Called when a keyboard key is released.
    /// </summary>
    public event EventHandler<KeyboardKeyEventArgs> KeyUp = delegate {};

    /// <summary>
    ///     Called when a text input is received.
    /// </summary>
    public event EventHandler<TextInputEventArgs> TextInput = delegate {};
}
