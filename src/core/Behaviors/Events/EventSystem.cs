// <copyright file="EventSystem.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Behaviors.Events;

/// <summary>
///     The event system is responsible for managing events and their handlers.
/// </summary>
public class EventSystem(IResourceContext context) : IEventRegistry, IEventBus
{
    private readonly Dictionary<Type, Event> events = new();
 
    /// <inheritdoc />
    public void Subscribe<TEventMessage>(Action<TEventMessage> handler) where TEventMessage : IEventMessage
    {
        if (events.TryGetValue(typeof(TEventMessage), out Event? @event) && @event is Event<TEventMessage> specific) specific.Subscribe(handler, context);

        // If the event is not defined, just ignore the subscription.
    }


    /// <inheritdoc />
    public IEvent<TEventMessage> RegisterEvent<TEventMessage>(Boolean single) where TEventMessage : IEventMessage
    {
        if (events.TryGetValue(typeof(TEventMessage), out Event? existingEvent))
        {
            context.ReportWarning(this, $"Event {typeof(TEventMessage)} already defined");

            return (IEvent<TEventMessage>) existingEvent;
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

        public void Subscribe(Action<TEventMessage> handler, IResourceContext context)
        {
            if (single && handlers.Count >= 1)
                context.ReportWarning(this,
                    $"Event {typeof(TEventMessage)} is single but has multiple subscriptions");
            else
                handlers.Add(handler);
        }
        
        public Boolean HasSubscribers => handlers.Count > 0;
    }
}
