// <copyright file="EventSystem.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
    public void Subscribe<TEventMessage>(Action<TEventMessage> handler) where TEventMessage : IEventMessage
    {
        if (events.TryGetValue(typeof(TEventMessage), out Event? @event) && @event is Event<TEventMessage> specific) specific.Subscribe(handler, validator);

        // If the event is not defined, just ignore the subscription.
    }


    /// <inheritdoc />
    public IEvent<TEventMessage> RegisterEvent<TEventMessage>(Boolean single) where TEventMessage : IEventMessage
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

    private class Event {}

    private sealed class Event<TEventMessage>(Boolean single) : Event, IEvent<TEventMessage> where TEventMessage : IEventMessage
    {
        private readonly List<Action<TEventMessage>> handlers = [];

        public void Publish(TEventMessage message)
        {
            foreach (Action<TEventMessage> handler in handlers)
                handler(message);
        }

        public void Subscribe(Action<TEventMessage> handler, IValidator validator)
        {
            if (single && handlers.Count >= 1)
                validator.ReportWarning($"Event {typeof(TEventMessage)} is single but has multiple subscriptions");
            else
                handlers.Add(handler);
        }
        
        public Boolean HasSubscribers => handlers.Count > 0;
        
        public Boolean IsSingle => single;
    }
}
