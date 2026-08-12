// <copyright file="InputHandlerRegistry.cs" company="VoxelGame">
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
using System.Collections.Generic;
using VoxelGame.Core.Collections;
using VoxelGame.Graphics.Input.Events;
using VoxelGame.GUI.Input;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Graphics.Input;

/// <summary>
///     Manages registrations of <see cref="IInputHandler" />s to specific <see cref="InputHandlerLayer" />s.
/// </summary>
internal class InputHandlerRegistry
{
    private static readonly InputHandlerLayer[] layers = Enum.GetValues<InputHandlerLayer>();

    private readonly Dictionary<InputHandlerLayer, List<Registration>> handlers = new()
    {
        [InputHandlerLayer.Application] = [],
        [InputHandlerLayer.UserInterface] = [],
        [InputHandlerLayer.Game] = []
    };

    private readonly Action<Registration> onRegister;
    private readonly Action<Registration> onUnregister;

    internal InputHandlerRegistry(Action<Registration> onRegister, Action<Registration> onUnregister)
    {
        this.onRegister = onRegister;
        this.onUnregister = onUnregister;
    }

    internal IDisposable Register(IInputHandler handler, InputHandlerLayer layer)
    {
        if (!handlers.TryGetValue(layer, out List<Registration>? registrations))
            throw Exceptions.UnsupportedEnumValue(layer);

        Registration registration = new(this, handler, layer);
        registrations.Add(registration);

        onRegister(registration);

        return registration;
    }

    private void Unregister(Registration registration)
    {
        if (!registration.IsActive) return;
        registration.Deactivate();

        onUnregister(registration);

        handlers[registration.Layer].Remove(registration);
    }

    internal Registration? DispatchKey(KeyboardKeyEventArgs args)
    {
        return Dispatch(args, static (handler, eventArgs) => handler.HandleKeyboardKey(eventArgs));
    }

    internal Registration? DispatchText(TextInputEventArgs args)
    {
        return Dispatch(args, static (handler, eventArgs) => handler.HandleText(eventArgs));
    }

    internal Registration? DispatchMouseButton(MouseButtonEventArgs args)
    {
        return Dispatch(args, static (handler, eventArgs) => handler.HandleMouseButton(eventArgs));
    }

    internal Registration? DispatchMouseMove(MouseMoveEventArgs args)
    {
        return Dispatch(args, static (handler, eventArgs) => handler.HandleMouseMove(eventArgs));
    }

    internal Registration? DispatchMouseWheel(MouseWheelEventArgs args)
    {
        return Dispatch(args, static (handler, eventArgs) => handler.HandleMouseWheel(eventArgs));
    }

    /// <summary>
    ///     Notify every active handler that one or more logical modifiers are no longer held.
    /// </summary>
    /// <param name="modifiers">The modifiers that are no longer held.</param>
    internal void BroadcastModifiersLost(ModifierKeys modifiers)
    {
        if (modifiers == ModifierKeys.None) return;

        Broadcast(modifiers, static (handler, lostModifiers) => handler.HandleModifiersLost(lostModifiers));
    }

    private Registration? Dispatch<T>(T eventArgs, Func<IInputHandler, T, Boolean> handle)
    {
        using PooledList<Registration> snapshot = new();

        foreach (InputHandlerLayer layer in layers)
            snapshot.AddRange(handlers[layer]);

        foreach (Registration registration in snapshot)
        {
            if (!registration.IsActive) continue;
            if (handle(registration.Handler, eventArgs)) return registration;
        }

        return null;
    }

    private void Broadcast<T>(T value, Action<IInputHandler, T> notify)
    {
        using PooledList<Registration> snapshot = new();

        foreach (InputHandlerLayer layer in layers)
            snapshot.AddRange(handlers[layer]);

        foreach (Registration registration in snapshot)
            if (registration.IsActive)
                notify(registration.Handler, value);
    }

    internal sealed class Registration(InputHandlerRegistry registry, IInputHandler handler, InputHandlerLayer layer) : IDisposable
    {
        internal IInputHandler Handler { get; } = handler;
        internal InputHandlerLayer Layer { get; } = layer;
        internal Boolean IsActive { get; private set; } = true;

        public void Dispose()
        {
            registry.Unregister(this);
        }

        internal void Deactivate()
        {
            IsActive = false;
        }
    }
}
