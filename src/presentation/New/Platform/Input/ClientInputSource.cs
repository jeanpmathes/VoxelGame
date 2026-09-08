// <copyright file="ClientInputSource.cs" company="Gwen.Net">
//     MIT License
// 
// </copyright>
// <author>Gwen.Net, jeanpmathes</author>

using System;
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Input;
using VoxelGame.Graphics.Input.Events;
using VoxelGame.GUI.Input;
using VoxelGame.GUI.Utilities;

namespace VoxelGame.Presentation.New.Platform.Input;

/// <summary>
///     Wraps a <see cref="Client" /> as an <see cref="InputSource" />.
/// </summary>
public sealed class ClientInputSource : InputSource, IInputHandler, IDisposable
{
    private readonly IDisposable registration;

    /// <summary>
    ///     Wrap a client to create an input source.
    /// </summary>
    /// <param name="client">The client to wrap.</param>
    public ClientInputSource(Client client)
    {
        registration = client.Input.RegisterHandler(this, InputHandlerLayer.UserInterface);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        registration.Dispose();
    }

    /// <inheritdoc />
    public Boolean HandleKeyboardKey(KeyboardKeyEventArgs args)
    {
        Key key = TranslateKeyCode(args.Key);

        return key != Key.Invalid && SendKeyEvent(key, args.IsPressed, args.IsRepeat, args.Modifiers, args.IsSynthetic);
    }

    /// <inheritdoc />
    public Boolean HandleText(TextInputEventArgs args)
    {
        return SendTextEvent(args.Character.ToString());
    }

    /// <inheritdoc />
    public Boolean HandleMouseButton(MouseButtonEventArgs args)
    {
        PointerButton button = TranslateMouseButton(args.Button);

        return button != PointerButton.Invalid && SendPointerButtonEvent(new Point(args.Position.X, args.Position.Y), button, args.IsPressed, args.Modifiers, args.IsSynthetic);
    }

    /// <inheritdoc />
    public Boolean HandleMouseMove(MouseMoveEventArgs args)
    {
        return SendPointerMoveEvent(new Point(args.Position.X, args.Position.Y), (Single) args.Delta.X, (Single) args.Delta.Y);
    }

    /// <inheritdoc />
    public Boolean HandleMouseWheel(MouseWheelEventArgs args)
    {
        return SendScrollEvent(new Point(args.Position.X, args.Position.Y), (Single) args.Delta.X, (Single) args.Delta.Y);
    }

    private static Key TranslateKeyCode(VirtualKeys key)
    {
        return key switch
        {
            VirtualKeys.Back => Key.Backspace,
            VirtualKeys.Return => Key.Enter,
            VirtualKeys.Escape => Key.Escape,
            VirtualKeys.Tab => Key.Tab,
            VirtualKeys.Space => Key.Space,
            VirtualKeys.Up => Key.Up,
            VirtualKeys.Down => Key.Down,
            VirtualKeys.Left => Key.Left,
            VirtualKeys.Right => Key.Right,
            VirtualKeys.Prior => Key.PageUp,
            VirtualKeys.Next => Key.PageDown,
            VirtualKeys.Home => Key.Home,
            VirtualKeys.End => Key.End,
            VirtualKeys.Delete => Key.Delete,
            VirtualKeys.Insert => Key.Insert,
            VirtualKeys.LeftControl or VirtualKeys.RightControl or VirtualKeys.Control => Key.Control,
            VirtualKeys.LeftMenu or VirtualKeys.RightMenu or VirtualKeys.Menu => Key.Alt,
            VirtualKeys.LeftShift or VirtualKeys.RightShift or VirtualKeys.Shift => Key.Shift,

            VirtualKeys.A => Key.A,
            VirtualKeys.B => Key.B,
            VirtualKeys.C => Key.C,
            VirtualKeys.D => Key.D,
            VirtualKeys.E => Key.E,
            VirtualKeys.F => Key.F,
            VirtualKeys.G => Key.G,
            VirtualKeys.H => Key.H,
            VirtualKeys.I => Key.I,
            VirtualKeys.J => Key.J,
            VirtualKeys.K => Key.K,
            VirtualKeys.L => Key.L,
            VirtualKeys.M => Key.M,
            VirtualKeys.N => Key.N,
            VirtualKeys.O => Key.O,
            VirtualKeys.P => Key.P,
            VirtualKeys.Q => Key.Q,
            VirtualKeys.R => Key.R,
            VirtualKeys.S => Key.S,
            VirtualKeys.T => Key.T,
            VirtualKeys.U => Key.U,
            VirtualKeys.V => Key.V,
            VirtualKeys.W => Key.W,
            VirtualKeys.X => Key.X,
            VirtualKeys.Y => Key.Y,
            VirtualKeys.Z => Key.Z,

            _ => Key.Invalid
        };
    }

    private static PointerButton TranslateMouseButton(VirtualKeys button)
    {
        return button switch
        {
            VirtualKeys.LeftButton => PointerButton.Left,
            VirtualKeys.RightButton => PointerButton.Right,
            VirtualKeys.MiddleButton => PointerButton.Middle,
            _ => PointerButton.Invalid
        };
    }
}
