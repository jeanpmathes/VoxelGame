// <copyright file="IEventRegistry.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

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
    /// <typeparam name="TEventMessage">The type of the event message.</typeparam>
    /// <returns>The registered event.</returns>
    public IEvent<TEventMessage> RegisterEvent<TEventMessage>(Boolean single) ;

    /// <summary>
    ///     Register an event with the registry, allowing multiple subscribers.
    /// </summary>
    /// <typeparam name="TEventMessage">The type of the event message.</typeparam>
    /// <returns>The registered event.</returns>
    public IEvent<TEventMessage> RegisterEvent<TEventMessage>() 
    {
        return RegisterEvent<TEventMessage>(single: false);
    }
}
