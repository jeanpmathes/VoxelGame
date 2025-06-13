// <copyright file="IEvent.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Behaviors.Events;

/// <summary>
///     Core interface for events.
/// </summary>
/// <typeparam name="TEventMessage">The message type of the event.</typeparam>
public interface IEvent<in TEventMessage> where TEventMessage : IEventMessage
{
    /// <summary>
    ///     Publish an event message to all subscribers of this event.
    /// </summary>
    /// <param name="message">The event message to publish.</param>
    public void Publish(TEventMessage message);
}
