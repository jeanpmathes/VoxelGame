// <copyright file="IEvent.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Collections;

namespace VoxelGame.Core.Behaviors.Events;

/// <summary>
///     Core interface for event messages.
/// </summary>
public interface IEventMessage
{
}

/// <summary>
///     Core interface for event messages, with pooling support.
/// </summary>
public interface IEventMessage<TSelf> : IEventMessage where TSelf : class, IEventMessage<TSelf>, new()
{
    /// <summary>
    ///    A simple pool for this event message type.
    /// </summary>
    public static SimpleObjectPool<TSelf> Pool { get; } = new();
}
