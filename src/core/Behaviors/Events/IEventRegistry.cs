// <copyright file="IEventRegistry.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Behaviors.Events;

/// <summary>
///     An event registry allows defining events which one intends to publish to.
/// </summary>
public interface IEventRegistry
{
    /// <summary>
    ///     Register an event with the registry.
    /// </summary>
    /// <param name="single">Whether there can only be one subscriber to this event.</param>
    /// <param name="context">The resource context in which the event is registered.</param>
    /// <typeparam name="TEventMessage">The type of the event message.</typeparam>
    /// <returns>The registered event.</returns>
    public IEvent<TEventMessage> RegisterEvent<TEventMessage>(Boolean single, IResourceContext context) where TEventMessage : IEventMessage;

    /// <summary>
    ///     Register an event with the registry, allowing multiple subscribers.
    /// </summary>
    /// <param name="context">The resource context in which the event is registered.</param>
    /// <typeparam name="TEventMessage">The type of the event message.</typeparam>
    /// <returns>The registered event.</returns>
    public IEvent<TEventMessage> RegisterEvent<TEventMessage>(IResourceContext context) where TEventMessage : IEventMessage
    {
        return RegisterEvent<TEventMessage>(single: false, context);
    }
}
