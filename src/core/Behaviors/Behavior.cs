// <copyright file="Behavior.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Toolkit.Utilities;

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
    /// Perform any validation required by the behavior.
    /// </summary>
    public void Validate(IResourceContext context)
    {
        OnValidate(context);
        
        Validation?.Invoke(this, new IAspectable.ValidationEventArgs
        {
            Context = context
        });
    }
    
    /// <summary>
    ///     Override this method to validate the behavior.
    /// </summary>
    protected virtual void OnValidate(IResourceContext context)
    {
        
    }

    /// <inheritdoc />
    public event EventHandler<IAspectable.ValidationEventArgs>? Validation;

    /// <inheritdoc />
    public override String ToString()
    {
        return Reflections.GetDecoratedName<TSelf>(Subject.ToString() ?? "unknown", instance: null); // todo: using TSelf here does not work as TSelf is always block behavior
    }
}
