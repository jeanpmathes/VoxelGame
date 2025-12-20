// <copyright file="EventSystem.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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

namespace VoxelGame.Core.Behaviors.Events;

/// <summary>
///     The event system is responsible for managing events and their handlers.
/// </summary>
public class EventSystem(IValidator validator) : IEventRegistry, IEventBus
{
    private readonly Dictionary<Type, Event> events = new();

    /// <inheritdoc />
    public void Subscribe<TEventMessage>(Action<TEventMessage> handler)
    {
        if (events.TryGetValue(typeof(TEventMessage), out Event? @event) && @event is Event<TEventMessage> specific) specific.Subscribe(handler, validator);

        // If the event is not defined, just ignore the subscription.
    }


    /// <inheritdoc />
    public IEvent<TEventMessage> RegisterEvent<TEventMessage>(Boolean single)
    {
        if (events.TryGetValue(typeof(TEventMessage), out Event? existingEvent))
        {
            var typedEvent = (Event<TEventMessage>) existingEvent;

            if (typedEvent.IsSingle != single)
                validator.ReportWarning($"Event {typeof(TEventMessage)} is already registered with single={typedEvent.IsSingle}, but tried to register with single={single}");

            return typedEvent;
        }

        Event<TEventMessage> @event = new(single);
        events.Add(typeof(TEventMessage), @event);

        return @event;
    }

    private class Event;

    private sealed class Event<TEventMessage>(Boolean single) : Event, IEvent<TEventMessage>
    {
        private readonly List<Action<TEventMessage>> handlers = [];

        public Boolean IsSingle => single;

        public void Publish(TEventMessage message)
        {
            foreach (Action<TEventMessage> handler in handlers)
                handler(message);
        }

        public Boolean HasSubscribers => handlers.Count > 0;

        public void Subscribe(Action<TEventMessage> handler, IValidator validator)
        {
            if (single && handlers.Count >= 1)
                validator.ReportWarning($"Event {typeof(TEventMessage)} is single but has multiple subscriptions");
            else
                handlers.Add(handler);
        }
    }
}
