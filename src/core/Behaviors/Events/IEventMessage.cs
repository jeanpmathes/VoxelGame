// <copyright file="IEvent.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Behaviors.Events;

/// <summary>
///     Core interface for event messages.
/// </summary>
public interface IEventMessage
{
    /// <summary>
    ///     Get the sender of the event message.
    /// </summary>
    public Object Sender { get; }
}
