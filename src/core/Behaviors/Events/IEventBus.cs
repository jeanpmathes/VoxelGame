// <copyright file="IEventBus.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Behaviors.Events;

/// <summary>
///     An event bus allows subscribing to events.
/// </summary>
public interface IEventBus
{
    /// <summary>
    ///     Subscribe to an event with a handler.
    /// </summary>
    /// <param name="handler">The event handler.</param>
    /// <typeparam name="TEventMessage">The type of event message to subscribe to.</typeparam>
    public void Subscribe<TEventMessage>(Action<TEventMessage> handler) where TEventMessage : IEventMessage;
}
