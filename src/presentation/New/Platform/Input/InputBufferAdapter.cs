// <copyright file="InputBufferAdapter.cs" company="VoxelGame">
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
using System.Drawing;
using VoxelGame.GUI.Input;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Presentation.New.Platform.Input;

/// <summary>
///     Buffers input events until signaled, then passes them to the input receiver in order of arrival.
/// </summary>
/// <param name="receiver">The receiver of the input events.</param>
public class InputBufferAdapter(IInputReceiver receiver) : IInputReceiver
{
    private readonly List<Event> events = [];

    /// <inheritdoc />
    public void ReceiveKeyEvent(Key key, Boolean isDown, Boolean isRepeat, ModifierKeys modifiers)
    {
        events.Add(new Event {type = EventType.KeyEvent, key = key, isDown = isDown, isRepeat = isRepeat, modifiers = modifiers});
    }

    /// <inheritdoc />
    public void ReceiveTextEvent(String text)
    {
        events.Add(new Event {type = EventType.TextEvent, text = text});
    }

    /// <inheritdoc />
    public void ReceivePointerButtonEvent(PointF position, PointerButton button, Boolean isDown, ModifierKeys modifiers)
    {
        events.Add(new Event {type = EventType.PointerButton, position = position, pointerButton = button, isDown = isDown, modifiers = modifiers});
    }

    /// <inheritdoc />
    public void ReceivePointerMoveEvent(PointF position, Single deltaX, Single deltaY)
    {
        events.Add(new Event {type = EventType.PointerMove, position = position, deltaX = deltaX, deltaY = deltaY});
    }

    /// <inheritdoc />
    public void ReceiveScrollEvent(PointF position, Single deltaX, Single deltaY)
    {
        events.Add(new Event {type = EventType.ScrollEvent, position = position, deltaX = deltaX, deltaY = deltaY});
    }

    /// <summary>
    ///     Sends all buffered events to the input receiver.
    /// </summary>
    public void Send()
    {
        foreach (Event @event in events)
        {
            switch (@event.type)
            {
                case EventType.KeyEvent:
                    receiver.ReceiveKeyEvent(@event.key, @event.isDown, @event.isRepeat, @event.modifiers);
                    break;
                case EventType.TextEvent:
                    receiver.ReceiveTextEvent(@event.text);
                    break;
                case EventType.PointerButton:
                    receiver.ReceivePointerButtonEvent(@event.position, @event.pointerButton, @event.isDown, @event.modifiers);
                    break;
                case EventType.PointerMove:
                    receiver.ReceivePointerMoveEvent(@event.position, @event.deltaX, @event.deltaY);
                    break;
                case EventType.ScrollEvent:
                    receiver.ReceiveScrollEvent(@event.position, @event.deltaX, @event.deltaY);
                    break;

                default:
                    throw Exceptions.UnsupportedEnumValue(@event.type);
            }
        }

        events.Clear();
    }

    private enum EventType
    {
        KeyEvent,
        TextEvent,
        PointerButton,
        PointerMove,
        ScrollEvent
    }

    private struct Event()
    {
        public EventType type = EventType.KeyEvent;

        public Key key = Key.Invalid;
        public PointerButton pointerButton = PointerButton.Left;

        public Boolean isDown = false;
        public Boolean isRepeat = false;
        public ModifierKeys modifiers = ModifierKeys.None;

        public PointF position = default;
        public Single deltaX = 0;
        public Single deltaY = 0;

        public String text = "";
    }
}
