// <copyright file="Actor.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Logic;
using VoxelGame.Toolkit.Components;
using VoxelGame.Annotations.Attributes;

namespace VoxelGame.Core.Actors;

/// <summary>
///     An actor is anything that can be added to a world, but is not the world itself.
/// </summary>
[ComponentSubject(typeof(ActorComponent))]
public abstract partial class Actor : Composed<Actor, ActorComponent>
{

    /// <summary>
    ///     Gets the world in which this actor is located.
    ///     Using an actor without a world is not valid.
    /// </summary>
    public World World { get; private set; } = null!;

    /// <summary>
    ///     The head of the actor, which allows to determine where the actor is looking at.
    ///     If an actor has no head or the concept of looking does not make sense, this will try to return the transform of the
    ///     actor itself.
    /// </summary>
    public virtual IOrientable? Head => GetComponent<Transform>();

    /// <summary>
    ///     Called when this actor is added to a world.
    /// </summary>
    /// <param name="world">The world to which this actor was added.</param>
    public void OnAdd(World world)
    {
        World = world;

        OnAddComponents();
    }

    /// <inheritdoc cref="Actor.OnAdd" />
    [ComponentEvent(nameof(ActorComponent.OnAdd))]
    private partial void OnAddComponents();

    /// <summary>
    ///     Called when this actor is removed from a world.
    /// </summary>
    public void OnRemove()
    {
        OnRemoveComponents();

        World = null!;
    }

    /// <inheritdoc cref="Actor.OnRemove" />
    [ComponentEvent(nameof(ActorComponent.OnRemove))]
    private partial void OnRemoveComponents();

    /// <summary>
    ///     Call to activate this actor.
    /// </summary>
    public void Activate()
    {
        OnActivateComponents();
    }

    /// <inheritdoc cref="Actor.Activate" />
    [ComponentEvent(nameof(ActorComponent.OnActivate))]
    private partial void OnActivateComponents();

    /// <summary>
    ///     Call to deactivate this actor.
    /// </summary>
    public void Deactivate()
    {
        OnDeactivateComponents();
    }

    /// <inheritdoc cref="Actor.Deactivate" />
    [ComponentEvent(nameof(ActorComponent.OnDeactivate))]
    private partial void OnDeactivateComponents();

    /// <summary>
    ///     Update this actor.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    public void LogicUpdate(Double deltaTime)
    {
        OnLogicUpdate(deltaTime);

        OnLogicUpdateComponents(deltaTime);
    }

    /// <inheritdoc cref="Actor.LogicUpdate" />
    [ComponentEvent(nameof(ActorComponent.OnLogicUpdate))]
    private partial void OnLogicUpdateComponents(Double deltaTime);

    /// <summary>
    ///     Called when the actor receives a logic update.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    protected virtual void OnLogicUpdate(Double deltaTime) {}
}
