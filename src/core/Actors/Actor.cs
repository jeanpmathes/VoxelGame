// <copyright file="Actor.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Logic;
using VoxelGame.Toolkit.Components;

namespace VoxelGame.Core.Actors;

/// <summary>
///     An actor is anything that can be added to a world, but is not the world itself.
/// </summary>
public abstract class Actor : Composed<Actor, ActorComponent>
{
    /// <inheritdoc />
    protected override Actor Self => this;

    /// <summary>
    ///     Gets the world in which this actor is located.
    ///     Using an actor without a world is not valid.
    /// </summary>
    public World World { get; private set; } = null!;

    /// <summary>
    ///     Called when this actor is added to a world.
    /// </summary>
    /// <param name="world">The world to which this actor was added.</param>
    public void OnAdd(World world)
    {
        World = world;

        foreach (ActorComponent component in Components)
        {
            component.OnAdd();
        }
    }

    /// <summary>
    ///     Called when this actor is removed from a world.
    /// </summary>
    public void OnRemove()
    {
        foreach (ActorComponent component in Components)
        {
            component.OnRemove();
        }

        World = null!;
    }

    /// <summary>
    /// Call to activate this actor.
    /// </summary>
    public void Activate()
    {
        foreach (ActorComponent component in Components)
        {
            component.OnActivate();
        }
    }

    /// <summary>
    /// Call to deactivate this actor.
    /// </summary>
    public void Deactivate()
    {
        foreach (ActorComponent component in Components)
        {
            component.OnDeactivate();
        }
    }

    /// <summary>
    ///     Update this actor.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    public void LogicUpdate(Double deltaTime)
    {
        OnLogicUpdate(deltaTime);

        foreach (ActorComponent component in Components)
        {
            component.OnLogicUpdate(deltaTime);
        }
    }
    
    /// <summary>
    /// Called when the actor receives a logic update.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    protected virtual void OnLogicUpdate(Double deltaTime) { }
    
    /// <summary>
    ///     The head of the actor, which allows to determine where the actor is looking at.
    ///     If an actor has no head or the concept of looking does not make sense, this will try to return the transform of the actor itself.
    /// </summary>
    public virtual IOrientable? Head => GetComponent<Transform>();
}
