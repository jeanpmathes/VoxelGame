// <copyright file="Actor.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Logic;
using VoxelGame.Toolkit.Components;

namespace VoxelGame.Core.Actors;

/// <summary>
///     An actor is anything that can be added to a world, but is not the world itself.
/// </summary>
[ComponentSubject(typeof(ActorComponent))]
public abstract partial class Actor : Composed<Actor, ActorComponent>
{
    private World? world;

    /// <summary>
    ///     Gets the world in which this actor is located.
    ///     Using an actor without a world is not valid.
    /// </summary>
    public World World => world!;

    /// <summary>
    ///     The head of the actor, which allows to determine where the actor is looking at.
    ///     If an actor has no head or the concept of looking does not make sense, this will try to return the transform of the
    ///     actor itself, or <c>null</c> if no transform is present.
    /// </summary>
    public virtual Transform? Head => GetComponent<Transform>();

    /// <summary>
    ///     Called when this actor is added to a world.
    ///     Before adding an actor to a world, it must be removed from any previous world.
    /// </summary>
    /// <param name="newWorld">The world to which this actor was added.</param>
    public void OnAdd(World newWorld)
    {
        Debug.Assert(world == null);

        world = newWorld;

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
        Debug.Assert(world != null);

        OnRemoveComponents();

        world = null;
    }

    /// <inheritdoc cref="Actor.OnRemove" />
    [ComponentEvent(nameof(ActorComponent.OnRemove))]
    private partial void OnRemoveComponents();

    /// <summary>
    ///     Call to activate this actor.
    /// </summary>
    public void Activate()
    {
        Debug.Assert(world != null);

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
        Debug.Assert(world != null);

        OnDeactivateComponents();
    }

    /// <inheritdoc cref="Actor.Deactivate" />
    [ComponentEvent(nameof(ActorComponent.OnDeactivate))]
    private partial void OnDeactivateComponents();

    /// <summary>
    ///     Update this actor. Not all actors are always updated, e.g. when in they are in an inactive chunk.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    public void LogicUpdate(Double deltaTime)
    {
        Debug.Assert(world != null);

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
