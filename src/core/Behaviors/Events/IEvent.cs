// <copyright file="IEvent.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Behaviors.Events;

/// <summary>
///     Core interface for events.
/// </summary>
/// <typeparam name="TEventMessage">The message type of the event.</typeparam>
public interface IEvent<in TEventMessage>
{
    /// <summary>
    ///     Get whether this event has any subscribers.
    /// </summary>
    Boolean HasSubscribers { get; }

    /// <summary>
    ///     Publish an event message to all subscribers of this event.
    /// </summary>
    /// <param name="message">The event message to publish.</param>
    void Publish(TEventMessage message);
}
