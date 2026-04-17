// <copyright file="ClientInputSource.cs" company="Gwen.Net">
//     MIT License
// 
// </copyright>
// <author>Gwen.Net, jeanpmathes</author>

using System;
using System.Drawing;
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Input.Events;
using VoxelGame.GUI.Input;

namespace VoxelGame.Presentation.New.Platform.Input;

/// <summary>
///     Wraps a <see cref="Client" /> as an <see cref="InputSource" />.
/// </summary>
public sealed class ClientInputSource : InputSource, IDisposable
{
    private readonly VoxelGame.Graphics.Input.Input input;

    private PointF lastMousePosition;

    /// <summary>
    ///     Wrap a client to create an input source.
    /// </summary>
    /// <param name="client">The client to wrap.</param>
    public ClientInputSource(Client client)
    {
        input = client.Input;

        input.KeyUp += OnKeyUp;
        input.KeyDown += OnKeyDown;
        input.TextInput += OnTextInput;
        input.MouseButton += OnMouseButton;
        input.MouseMove += OnMouseMove;
        input.MouseWheel += OnMouseWheel;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        input.KeyUp -= OnKeyUp;
        input.KeyDown -= OnKeyDown;
        input.TextInput -= OnTextInput;
        input.MouseButton -= OnMouseButton;
        input.MouseMove -= OnMouseMove;
        input.MouseWheel -= OnMouseWheel;
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
            VirtualKeys.LeftControl or VirtualKeys.RightControl => Key.Control,
            VirtualKeys.LeftMenu or VirtualKeys.RightMenu => Key.Alt,
            VirtualKeys.LeftShift or VirtualKeys.RightShift => Key.Shift,

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

    // todo: bring back modifiers and this translation
    /*private static ModifierKeys TranslateModifierKeys(KeyModifiers modifiers)
    {
        ModifierKeys modifierKeys = ModifierKeys.None;

        if (modifiers.HasFlag(KeyModifiers.Alt))
            modifierKeys |= ModifierKeys.Alt;

        if (modifiers.HasFlag(KeyModifiers.Control))
            modifierKeys |= ModifierKeys.Control;

        if (modifiers.HasFlag(KeyModifiers.Shift))
            modifierKeys |= ModifierKeys.Shift;

        return modifierKeys;
    }*/

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

    private void OnKeyDown(Object? sender, KeyboardKeyEventArgs args)
    {
        Key key = TranslateKeyCode(args.Key);

        if (key == Key.Invalid)
            return;

        // todo: bring back modifiers and IsRepeat
        SendKeyEvent(key, isDown: true, /*args.IsRepeat, TranslateModifierKeys(args.Modifiers)*/ isRepeat: false, ModifierKeys.None);
    }

    private void OnKeyUp(Object? sender, KeyboardKeyEventArgs args)
    {
        Key key = TranslateKeyCode(args.Key);

        if (key == Key.Invalid)
            return;

        // todo: bring back modifiers and IsRepeat
        SendKeyEvent(key, isDown: false, /*args.IsRepeat, TranslateModifierKeys(args.Modifiers)*/ isRepeat: false, ModifierKeys.None);
    }

    private void OnTextInput(Object? sender, TextInputEventArgs args)
    {
        SendTextEvent(args.Character.ToString()); // todo: maybe the event also has to be a string earlier
    }

    private void OnMouseButton(Object? sender, MouseButtonEventArgs args)
    {
        // todo: bring back modifiers
        SendPointerButtonEvent(lastMousePosition, TranslateMouseButton(args.Button), args.IsPressed, /*TranslateModifierKeys(args.Modifiers)*/ModifierKeys.None);
    }

    private void OnMouseMove(Object? sender, MouseMoveEventArgs args)
    {
        lastMousePosition = new PointF(args.Position.X, args.Position.Y);

        SendPointerMoveEvent(lastMousePosition, /*args.DeltaX, args.DeltaY*/deltaX: 0.0f, deltaY: 0.0f); // todo: bring back delta to event if easy or calculate it here and pass correctly
    }

    private void OnMouseWheel(Object? sender, MouseWheelEventArgs args)
    {
        SendScrollEvent(lastMousePosition, (Single) args.Delta, /*args.OffsetY*/deltaY: 0.0f); // todo: bring back different deltas and correctly pass here
    }
}
