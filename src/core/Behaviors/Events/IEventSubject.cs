// <copyright file="IEventSubject.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Behaviors.Events;

/// <summary>
///     An event subject is a class that can define and subscribe to events.
/// </summary>
public interface IEventSubject
{
    /// <summary>
    ///     Let the subject define its events in the given registry.
    /// </summary>
    public void DefineEvents(IEventRegistry registry);

    /// <summary>
    ///     Let the subject subscribe to events in the given event bus.
    /// </summary>
    public void SubscribeToEvents(IEventBus bus);
}
