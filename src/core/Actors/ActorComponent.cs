// <copyright file="ActorComponent.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Toolkit.Components;

namespace VoxelGame.Core.Actors;

/// <summary>
/// Base class for all components used in the <see cref="Actor"/> class.
/// </summary>
public class ActorComponent(Actor subject) : Component<Actor>(subject)
{
    /// <summary>
    /// Called when the actor is added to a world.
    /// </summary>
    public virtual void OnAdd() {}

    /// <summary>
    /// Called when the actor is removed from a world.
    /// </summary>
    public virtual void OnRemove() {}
    
    /// <summary>
    /// Called when the actor is activated.
    /// If an actor is added to an inactive world, it will be activated when the world is activated.
    /// </summary>
    public virtual void OnActivate() { }
    
    /// <summary>
    /// Called when the actor is deactivated.
    /// If the world is deactivated, all actors in it will be deactivated.
    /// </summary>
    public virtual void OnDeactivate() { }
    
    /// <summary>
    /// Called when the actor receives a logic update.
    /// </summary>
    public virtual void OnLogicUpdate(Double deltaTime) { }
}
