// <copyright file="Input.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Input.Devices;
using VoxelGame.Graphics.Input.Events;
using VoxelGame.GUI.Input;
using VoxelGame.Toolkit.Interop;

namespace VoxelGame.Graphics.Input;

/// <summary>
///     Receives raw input from the client, controls the mouse and keyboard, and is the entry point for input routing.
/// </summary>
public class Input
{
    private readonly InputRouter router = new();

    internal Input(Mouse mouse)
    {
        Mouse = mouse;
    }

    /// <summary>
    ///     Get the mouse device.
    /// </summary>
    public Mouse Mouse { get; }

    /// <summary>
    ///     Raised before the input system resets, which means that outstanding key and button releases will be issued.
    /// </summary>
    public event EventHandler? Reset;

    /// <summary>
    ///     Register a handler that receives input in its layer until the registration is disposed of.
    /// </summary>
    /// <param name="handler">The handler to register.</param>
    /// <param name="layer">The layer in which to register the handler.</param>
    /// <returns>A registration that removes and unregisters the handler when disposed.</returns>
    public IDisposable RegisterHandler(IInputHandler handler, InputHandlerLayer layer)
    {
        return router.RegisterHandler(handler, layer);
    }

    /// <summary>
    ///     Refresh input devices before application input-update consumers run.
    /// </summary>
    internal void Update()
    {
        Mouse.Update();
    }

    /// <summary>
    ///     Reset the current state.
    ///     Call this when the client may no longer receive input messages for some time, meaning physical release events might be missed.
    ///     This will cause synthetic key and button releases to be issued.
    /// </summary>
    internal void ResetState()
    {
        Reset?.Invoke(this, EventArgs.Empty);

        router.Reset();
    }

    /// <summary>
    ///     Receive a keyboard message from the native client and return whether a handler accepted it.
    /// </summary>
    internal Bool OnKey(Byte key, Bool isDown, Bool isRepeat, ModifierKeys modifiers)
    {
        VirtualKeys virtualKey = (VirtualKeys) key;
        Boolean pressed = isDown;

        return router.RouteKeyboardKey(new KeyboardKeyEventArgs
        {
            Key = virtualKey,
            IsPressed = pressed,
            IsRepeat = isRepeat,
            Modifiers = modifiers
        });
    }

    /// <summary>
    ///     Receive a text message from the native client and return whether a handler accepted it.
    /// </summary>
    internal Bool OnChar(Char character)
    {
        TextInputEventArgs args = new() {Character = character};

        return router.RouteText(args);
    }

    /// <summary>
    ///     Receive a mouse-button message from the native client and return whether a handler accepted it.
    /// </summary>
    internal Bool OnMouseButton(Byte button, Bool isDown, Int32 x, Int32 y, ModifierKeys modifiers)
    {
        VirtualKeys virtualKey = (VirtualKeys) button;
        Boolean pressed = isDown;

        return router.RouteMouseButton(new MouseButtonEventArgs
        {
            Button = virtualKey,
            IsPressed = pressed,
            Position = (x, y),
            Modifiers = modifiers
        });
    }

    /// <summary>
    ///     Receive a mouse-move message from the native client and return whether a handler accepted it.
    /// </summary>
    internal Bool OnMouseMove(Int32 x, Int32 y, Int32 deltaX, Int32 deltaY)
    {
        Mouse.OnMouseMove((x, y));

        MouseMoveEventArgs args = new()
        {
            Position = (x, y),
            Delta = (deltaX, deltaY)
        };

        return router.RouteMouseMove(args);
    }

    /// <summary>
    ///     Receive a mouse-wheel message from the native client and return whether a handler accepted it.
    /// </summary>
    internal Bool OnMouseWheel(Int32 x, Int32 y, Double scrollX, Double scrollY)
    {
        MouseWheelEventArgs args = new()
        {
            Position = (x, y),
            Delta = (scrollX, scrollY)
        };

        return router.RouteMouseWheel(args);
    }
}
