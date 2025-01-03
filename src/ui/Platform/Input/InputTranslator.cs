﻿// <copyright file="InputTranslator.cs" company="Gwen.Net">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>Gwen.Net, jeanpmathes</author>

using System;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Input;
using OpenTK.Mathematics;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Input.Events;

namespace VoxelGame.UI.Platform.Input;

/// <summary>
///     Translates input from the platform to Gwen.Net.
/// </summary>
public class InputTranslator
{
    private readonly Canvas canvas;

    private Boolean controlPressed;
    private Vector2 lastMousePosition;

    /// <summary>
    ///     Creates a new instance of <see cref="InputTranslator" />.
    /// </summary>
    /// <param name="canvas"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public InputTranslator(Canvas canvas)
    {
        this.canvas = canvas;
    }

    private GwenMappedKey TranslateKeyCode(VirtualKeys key)
    {
        switch (key)
        {
            case VirtualKeys.Back: return GwenMappedKey.Backspace;
            case VirtualKeys.Return: return GwenMappedKey.Return;
            case VirtualKeys.Escape: return GwenMappedKey.Escape;
            case VirtualKeys.Tab: return GwenMappedKey.Tab;
            case VirtualKeys.Space: return GwenMappedKey.Space;
            case VirtualKeys.Up: return GwenMappedKey.Up;
            case VirtualKeys.Down: return GwenMappedKey.Down;
            case VirtualKeys.Left: return GwenMappedKey.Left;
            case VirtualKeys.Right: return GwenMappedKey.Right;
            case VirtualKeys.Home: return GwenMappedKey.Home;
            case VirtualKeys.End: return GwenMappedKey.End;
            case VirtualKeys.Delete: return GwenMappedKey.Delete;

            case VirtualKeys.LeftControl:
                controlPressed = true;

                return GwenMappedKey.Control;
            case VirtualKeys.LeftMenu: return GwenMappedKey.Alt;
            case VirtualKeys.LeftShift: return GwenMappedKey.Shift;

            case VirtualKeys.RightControl: return GwenMappedKey.Control;
            case VirtualKeys.RightMenu:
                if (controlPressed) canvas.Input_Key(GwenMappedKey.Control, down: false);

                return GwenMappedKey.Alt;
            case VirtualKeys.RightShift: return GwenMappedKey.Shift;

            default:
                return GwenMappedKey.Invalid;
        }
    }

    private static Char TranslateChar(VirtualKeys key)
    {
        if (key is >= VirtualKeys.A and <= VirtualKeys.Z) return (Char) ('a' + ((Int32) key - (Int32) VirtualKeys.A));

        return ' ';
    }

    /// <summary>
    ///     Processes a mouse button press.
    /// </summary>
    public void ProcessMouseButton(MouseButtonEventArgs args)
    {
        if (args.Button == VirtualKeys.LeftButton) canvas.Input_MouseButton(button: 0, args.IsPressed);
        else if (args.Button == VirtualKeys.RightButton) canvas.Input_MouseButton(button: 1, args.IsPressed);
    }

    /// <summary>
    ///     Processes a mouse move.
    /// </summary>
    public void ProcessMouseMove(MouseMoveEventArgs args)
    {
        Vector2 deltaPosition = args.Position - lastMousePosition;
        lastMousePosition = args.Position;

        canvas.Input_MouseMoved(
            (Int32) lastMousePosition.X,
            (Int32) lastMousePosition.Y,
            (Int32) deltaPosition.X,
            (Int32) deltaPosition.Y);
    }

    /// <summary>
    ///     Processes a mouse wheel event.
    /// </summary>
    public void ProcessMouseWheel(MouseWheelEventArgs args)
    {
        Double delta = args.Delta * 60;
        canvas.Input_MouseWheel((Int32) delta);
    }

    /// <summary>
    ///     Processes a key down event.
    /// </summary>
    public Boolean ProcessKeyDown(KeyboardKeyEventArgs args)
    {
        Char ch = TranslateChar(args.Key);

        if (InputHandler.DoSpecialKeys(canvas, ch)) return false;

        GwenMappedKey iKey = TranslateKeyCode(args.Key);

        if (iKey == GwenMappedKey.Invalid) return false;

        return canvas.Input_Key(iKey, down: true);
    }

    /// <summary>
    ///     Processes a text input event.
    /// </summary>
    /// <param name="args"></param>
    public void ProcessTextInput(TextInputEventArgs args)
    {
        canvas.Input_Character(args.Character);
    }

    /// <summary>
    ///     Processes a key up event.
    /// </summary>
    public Boolean ProcessKeyUp(KeyboardKeyEventArgs args)
    {
        GwenMappedKey key = TranslateKeyCode(args.Key);

        return canvas.Input_Key(key, down: false);
    }
}
