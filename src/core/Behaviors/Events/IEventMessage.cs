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
    // todo: allow simple pooling of event messages, add to analyzer note that there should be an attribute for pooled messages for which analysis exists so they are not kept, do same for SectionChangedEventArgs
    // todo: the pooling storage should use the pool class so it is thread-safe instead of a single static instance
    // todo: go through all publish calls and use pooling there
{
    /// <summary>
    ///     Get the sender of the event message.
    /// </summary>
    public Object Sender { get; }
}
