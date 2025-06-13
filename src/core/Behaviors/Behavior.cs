// <copyright file="Behavior.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors.Events;

namespace VoxelGame.Core.Behaviors;

/// <summary>
///     The base class for the behavior system.
///     Behaviors allow defining the functionality of subject instances modularly.
/// </summary>
/// <param name="subject">The subject that this behavior applies to.</param>
/// <typeparam name="TSelf">The type of the behavior itself.</typeparam>
/// <typeparam name="TSubject">The type of the subject that the behavior applies to.</typeparam>
public abstract class Behavior<TSelf, TSubject>(TSubject subject) : IBehavior<TSubject>
    where TSelf : Behavior<TSelf, TSubject>
    where TSubject : class, IHasBehaviors<TSubject, TSelf>
{
    /// <inheritdoc />
    public TSubject Subject { get; } = subject;


    /// <summary>
    ///     Override this method to define events that the behavior will publish.
    /// </summary>
    public virtual void DefineEvents(IEventRegistry registry) {}

    /// <summary>
    ///     Override this method to subscribe to events that the behavior will handle.
    /// </summary>
    public virtual void SubscribeToEvents(IEventBus bus) {}

    /// <summary>
    ///     Override this method to validate the behavior.
    /// </summary>
    public virtual void Validate() {}
}
