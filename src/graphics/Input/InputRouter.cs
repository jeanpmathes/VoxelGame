// <copyright file="InputRouter.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Input.Events;
using VoxelGame.GUI.Input;

namespace VoxelGame.Graphics.Input;

/// <summary>
///     Routes input to the appropriate <see cref="IInputHandler" />, respecting order of registration and the
///     <see cref="InputHandlerLayer" />.
///     If a handler handled a press, the handler will also receive the corresponding release and prevent other handlers
///     from intercepting it.
/// </summary>
internal sealed class InputRouter
{
    private readonly InputHandlerRegistry registry;

    /// <summary>
    ///     When a handler handles a down-event, we want to ensure it also receives the corresponding up-event.
    ///     As such, we store all handlers that are waiting for the corresponding up-event here.
    ///     If the handler gets cleaned up or deactivated, we instead suppress the corresponding up-event.
    /// </summary>
    private readonly KeyDictionary<(InputHandlerRegistry.Registration registration, Boolean isKey)> waitingHandlers = new();

    private readonly KeySet state = new();
    private readonly KeySet suppressions = new();

    private Vector2 mousePosition;

    internal InputRouter()
    {
        registry = new InputHandlerRegistry(OnRegister, OnUnregister);
    }

    private static void OnRegister(InputHandlerRegistry.Registration registration)
    {
        // Nothing to do.
    }

    private void OnUnregister(InputHandlerRegistry.Registration registration)
    {
        using PooledList<VirtualKeys> waiting = [];

        foreach ((VirtualKeys key, (InputHandlerRegistry.Registration waitingRegistration, Boolean _)) in waitingHandlers)
            if (waitingRegistration == registration)
                waiting.Add(key);

        foreach (VirtualKeys keyOrButton in waiting)
        {
            if (IsKeyOrButtonDown(keyOrButton))
                suppressions.Add(keyOrButton);

            CleanUpWaitingHandlerOfKeyOrButton(keyOrButton);
        }
    }

    /// <summary>
    ///     Register a handler, inserted to the back of the layer.
    /// </summary>
    /// <param name="handler">The handler that to register.</param>
    /// <param name="layer">The layer in which the handler will be registered.</param>
    /// <returns>A registration that removes the handler when disposed. Dispose this before the handler is destroyed.</returns>
    public IDisposable RegisterHandler(IInputHandler handler, InputHandlerLayer layer)
    {
        return registry.Register(handler, layer);
    }

    /// <summary>
    ///     Route a keyboard key event to the appropriate handler.
    /// </summary>
    /// <param name="args">The key event received from the client.</param>
    /// <returns><see langword="true" /> if the event was handled by a handler, otherwise <see langword="false" />.</returns>
    internal Boolean RouteKeyboardKey(KeyboardKeyEventArgs args)
    {
        VirtualKeys key = args.Key;
        ModifierKeys modifiersBefore = state.GetModifiers();

        state.Set(key, args.IsPressed);

        ModifierKeys lostModifiers = modifiersBefore & ~state.GetModifiers();

        Boolean handled = args.IsPressed
            ? RouteKeyboardKeyDown(args, key)
            : RouteKeyboardKeyUp(args, key);

        registry.BroadcastModifiersLost(lostModifiers);

        return handled;
    }

    private Boolean RouteKeyboardKeyDown(KeyboardKeyEventArgs args, VirtualKeys key)
    {
        if (suppressions.Contains(key)) return true;

        if (waitingHandlers.TryGetValue(key, out (InputHandlerRegistry.Registration registration, Boolean isKey) waiting))
        {
            if (waiting.registration.IsActive)
                waiting.registration.Handler.HandleKeyboardKey(args);

            return true;
        }

        InputHandlerRegistry.Registration? handler = registry.DispatchKey(args);

        SetHandlerWaitingForReleaseOfKeyOrButton(handler, key, isKey: true);

        return handler != null;
    }

    private Boolean RouteKeyboardKeyUp(KeyboardKeyEventArgs args, VirtualKeys key)
    {
        if (suppressions.Remove(key)) return true;

        if (waitingHandlers.Remove(key, out (InputHandlerRegistry.Registration registration, Boolean isKey) waiting))
        {
            if (waiting.registration.IsActive) waiting.registration.Handler.HandleKeyboardKey(args);

            return true;
        }

        return registry.DispatchKey(args) != null;
    }

    /// <summary>
    ///     Routes a mouse button event to the appropriate handler.
    /// </summary>
    /// <param name="args">The mouse button event received from the client.</param>
    /// <returns><see langword="true" /> if the event was handled by a handler; otherwise, <see langword="false" />.</returns>
    internal Boolean RouteMouseButton(MouseButtonEventArgs args)
    {
        VirtualKeys button = args.Button;

        mousePosition = args.Position;
        state.Set(button, args.IsPressed);

        return args.IsPressed
            ? RouteMouseButtonDown(args, button)
            : RouteMouseButtonUp(args, button);
    }

    private Boolean RouteMouseButtonUp(MouseButtonEventArgs args, VirtualKeys button)
    {
        if (suppressions.Remove(button)) return true;

        if (waitingHandlers.Remove(button, out (InputHandlerRegistry.Registration registration, Boolean isKey) waiting))
        {
            if (waiting.registration.IsActive) waiting.registration.Handler.HandleMouseButton(args);

            return true;
        }

        return registry.DispatchMouseButton(args) != null;
    }

    private Boolean RouteMouseButtonDown(MouseButtonEventArgs args, VirtualKeys button)
    {
        if (suppressions.Contains(button)) return true;

        if (waitingHandlers.TryGetValue(button, out (InputHandlerRegistry.Registration registration, Boolean isKey) waiting))
        {
            if (waiting.registration.IsActive)
                waiting.registration.Handler.HandleMouseButton(args);

            return true;
        }

        InputHandlerRegistry.Registration? handledBy = registry.DispatchMouseButton(args);
        SetHandlerWaitingForReleaseOfKeyOrButton(handledBy, button, isKey: false);

        return handledBy != null;
    }

    /// <summary>
    ///     Routes a text input event to the appropriate <see cref="IInputHandler" />.
    /// </summary>
    /// <param name="args">The arguments containing information about the text input event.</param>
    /// <returns>
    ///     <see langword="true" /> if an input handler processed the event; otherwise, <see langword="false" />.
    /// </returns>
    internal Boolean RouteText(TextInputEventArgs args)
    {
        return registry.DispatchText(args) != null;
    }

    internal Boolean RouteMouseMove(MouseMoveEventArgs args)
    {
        mousePosition = args.Position;

        return registry.DispatchMouseMove(args) != null;
    }

    internal Boolean RouteMouseWheel(MouseWheelEventArgs args)
    {
        mousePosition = args.Position;

        return registry.DispatchMouseWheel(args) != null;
    }

    /// <summary>
    ///     Synthetically release all keys and buttons currently waiting for a release.
    ///     Call this before release events might get lost, for example, when the window stops receiving events because of
    ///     focus loss.
    /// </summary>
    internal void Reset()
    {
        ModifierKeys lostModifiers = state.GetModifiers();

        using PooledList<VirtualKeys> ordinaryKeys = [];
        using PooledList<VirtualKeys> mouseButtons = [];
        using PooledList<VirtualKeys> modifierKeys = [];

        foreach ((VirtualKeys key, (InputHandlerRegistry.Registration registration, Boolean isKey) value) in waitingHandlers)
            if (!value.isKey)
                mouseButtons.Add(key);
            else if (key.IsModifier())
                modifierKeys.Add(key);
            else
                ordinaryKeys.Add(key);

        foreach (VirtualKeys key in ordinaryKeys)
        {
            CleanUpWaitingHandlerOfKeyOrButton(key);
            state.Remove(key);
        }

        foreach (VirtualKeys button in mouseButtons)
        {
            CleanUpWaitingHandlerOfKeyOrButton(button);
            state.Remove(button);
        }

        foreach (VirtualKeys key in modifierKeys)
        {
            CleanUpWaitingHandlerOfKeyOrButton(key);
            state.Remove(key);
        }

        waitingHandlers.Clear();

        suppressions.Reset();

        state.Reset();

        registry.BroadcastModifiersLost(lostModifiers);
    }

    private void SetHandlerWaitingForReleaseOfKeyOrButton(InputHandlerRegistry.Registration? handler, VirtualKeys keyOrButton, Boolean isKey)
    {
        if (handler == null || IsKeyOrButtonUp(keyOrButton)) return;

        if (handler.IsActive)
            waitingHandlers.Add(keyOrButton, (handler, isKey));
        else if (IsKeyOrButtonDown(keyOrButton))
            suppressions.Add(keyOrButton);
    }

    private Boolean IsKeyOrButtonDown(VirtualKeys keyOrButton)
    {
        return state.Contains(keyOrButton);
    }

    private Boolean IsKeyOrButtonUp(VirtualKeys keyOrButton)
    {
        return !state.Contains(keyOrButton);
    }

    private void CleanUpWaitingHandlerOfKeyOrButton(VirtualKeys keyOrButton)
    {
        if (!waitingHandlers.Remove(keyOrButton, out (InputHandlerRegistry.Registration registration, Boolean isKey) waiting)) return;

        ModifierKeys modifiers = state.GetModifiers(keyOrButton);

        if (waiting.isKey)
        {
            waiting.registration.Handler.HandleKeyboardKey(new KeyboardKeyEventArgs
            {
                Key = keyOrButton,
                IsPressed = false,
                IsRepeat = false,
                IsSynthetic = true,
                Modifiers = modifiers
            });
        }
        else
        {
            waiting.registration.Handler.HandleMouseButton(new MouseButtonEventArgs
            {
                Button = keyOrButton,
                IsPressed = false,
                IsSynthetic = true,
                Position = mousePosition,
                Modifiers = modifiers
            });
        }
    }
}
