// <copyright file="Composed.cs" company="VoxelGame">
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
using System.Collections.Generic;
using System.Diagnostics;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Toolkit.Components;

/// <summary>
///     Abstract base class for non-generic methods of <see cref="Composed{TSelf,TComponent}" />.
/// </summary>
public abstract class Composed : IDisposable
{
    /// <summary>
    ///     Remove a specified component by providing the component instance.
    ///     This should generally not be used directly.
    ///     If the component is not attached to this container, nothing happens.
    /// </summary>
    /// <param name="component">The component to remove.</param>
    public abstract void RemoveComponent(Object component);

    #region DISPOSABLE

    /// <summary>
    ///     Called by the finalizer or Dispose method.
    /// </summary>
    /// <param name="disposing"><c>true</c> if called from Dispose, <c>false</c> if called from the finalizer.</param>
    protected virtual void Dispose(Boolean disposing) {}

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~Composed()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}

/// <summary>
///     Base class for objects that can hold components.
///     <para>
///         Components are simple objects that extend the functionality of their
///         subject. They can be attached and removed at any time, which makes
///         them ideal for features that should appear or disappear while an
///         object is alive. Because components are stored in a list and queried
///         dynamically, access is flexible but not optimized for extremely
///         frequent checks.
///     </para>
///     <para>
///         The behavior system provides a similar concept but bakes
///         behaviors into arrays for very fast access. Behaviors therefore
///         become immutable after baking. Use the component system when
///         functionality needs to remain mutable at runtime. Use the behavior
///         system when the set of features of an object is fixed after
///         creation and when queries about the presence of a feature are
///         performance-critical. The behavior system also only makes sense
///         when all objects are created in advance, e.g., with the flyweight
///         pattern.
///     </para>
/// </summary>
/// <typeparam name="TSelf">The type of the container itself.</typeparam>
/// <typeparam name="TComponent">The base type of components stored in this container.</typeparam>
public abstract class Composed<TSelf, TComponent> : Composed
    where TSelf : Composed<TSelf, TComponent>
    where TComponent : Component<TSelf>
{
    private readonly OrderedDictionary<Type, TComponent> components = new();

    /// <summary>
    ///     Get the container itself.
    /// </summary>
    protected abstract TSelf Self { get; }

    /// <summary>
    ///     Get all components attached to this container.
    /// </summary>
    protected IEnumerable<TComponent> Components => components.Values;

    /// <summary>
    ///     Add a component of the given type.
    ///     If there already exists a component of the same type, the existing component is returned.
    /// </summary>
    public TConcrete AddComponent<TConcrete>() where TConcrete : TComponent, IConstructible<TSelf, TConcrete>
    {
        if (GetComponent<TConcrete>() is {} existing) return existing;

        var component = TConcrete.Construct(Self);

        components.Add(typeof(TConcrete), component);

        OnComponentAdded(component);

        return component;
    }

    /// <summary>
    ///     Add a component of the given type.
    ///     If there already exists a component of the same type, the existing component is returned.
    /// </summary>
    public TConcrete AddComponent<TConcrete, TConcreteSelf>() where TConcrete : TComponent, IConstructible<TConcreteSelf, TConcrete> where TConcreteSelf : TSelf
    {
        if (GetComponent<TConcrete>() is {} existing) return existing;

        var component = TConcrete.Construct((TConcreteSelf) Self);

        components.Add(typeof(TConcrete), component);

        OnComponentAdded(component);

        return component;
    }

    /// <summary>
    ///     Add a component of the given type.
    ///     If there already exists a component of the same type, the existing component is returned.
    /// </summary>
    public TConcrete AddComponent<TConcrete, TArgument>(TArgument argument) where TConcrete : TComponent, IConstructible<TSelf, TArgument, TConcrete>
    {
        if (GetComponent<TConcrete>() is {} existing) return existing;

        var component = TConcrete.Construct(Self, argument);

        components.Add(typeof(TConcrete), component);

        OnComponentAdded(component);

        return component;
    }

    /// <summary>
    ///     Add a component of the given type.
    ///     If there already exists a component of the same type, the existing component is returned.
    /// </summary>
    public TConcrete AddComponent<TConcrete, TArgument, TConcreteSelf>(TArgument argument) where TConcrete : TComponent, IConstructible<TConcreteSelf, TArgument, TConcrete> where TConcreteSelf : TSelf
    {
        if (GetComponent<TConcrete>() is {} existing) return existing;

        var component = TConcrete.Construct((TConcreteSelf) Self, argument);

        components.Add(typeof(TConcrete), component);

        OnComponentAdded(component);

        return component;
    }

    /// <summary>
    ///     Remove the specified component.
    /// </summary>
    public Boolean RemoveComponent<TConcrete>() where TConcrete : TComponent
    {
        if (!components.TryGetValue(typeof(TConcrete), out TComponent? component)) return false;

        RemoveComponent(typeof(TConcrete), component);

        return true;
    }

    /// <inheritdoc />
    public override void RemoveComponent(Object component)
    {
        Type type = component.GetType();

        if (!components.TryGetValue(type, out TComponent? existing) || !ReferenceEquals(existing, component))
            return;

        RemoveComponent(type, existing);
    }

    private void RemoveComponent(Type type, TComponent component)
    {
        components.Remove(type);

        OnComponentRemoved(component);

        component.Dispose();
    }

    /// <summary>
    ///     Get the component of the specified type or <c>null</c> if it does not exist.
    /// </summary>
    public TConcrete? GetComponent<TConcrete>() where TConcrete : TComponent
    {
        if (!components.TryGetValue(typeof(TConcrete), out TComponent? component))
            return null;

        Debug.Assert(component is TConcrete);

        return (TConcrete) component;
    }

    /// <summary>
    ///     Get the component of the specified type or add it if it does not exist.
    /// </summary>
    public TConcrete GetRequiredComponent<TConcrete>() where TConcrete : TComponent, IConstructible<TSelf, TConcrete>
    {
        if (GetComponent<TConcrete>() is {} component) return component;

        return AddComponent<TConcrete>();
    }

    /// <summary>
    ///     Get the component of the specified type or add it if it does not exist.
    /// </summary>
    public TConcrete GetRequiredComponent<TConcrete, TConcreteSelf>() where TConcrete : TComponent, IConstructible<TConcreteSelf, TConcrete> where TConcreteSelf : TSelf
    {
        if (GetComponent<TConcrete>() is {} component) return component;

        return AddComponent<TConcrete, TConcreteSelf>();
    }

    /// <summary>
    ///     Get the component of the specified type or throw an exception if it does not exist.
    ///     This is useful for components that take parameters in their constructor and can thus not be added on demand.
    /// </summary>
    public TConcrete GetComponentOrThrow<TConcrete>() where TConcrete : TComponent
    {
        if (GetComponent<TConcrete>() is {} component) return component;

        throw Exceptions.InvalidOperation($"Component of type {typeof(TConcrete).Name} does not exist on {Self}.");
    }

    /// <summary>
    ///     Check whether a component of the specified type exists.
    /// </summary>
    public Boolean HasComponent<TConcrete>() where TConcrete : TComponent
    {
        return components.ContainsKey(typeof(TConcrete));
    }

    /// <summary>
    ///     Called when a component has been added to this container.
    /// </summary>
    /// <param name="component">The component that was added.</param>
    protected virtual void OnComponentAdded(TComponent component) {}

    /// <summary>
    ///     Called when a component has been removed from this container.
    /// </summary>
    /// <param name="component">The component that was removed.</param>
    protected virtual void OnComponentRemoved(TComponent component) {}

    #region DISPOSABLE

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        foreach (TComponent component in components.Values)
        {
            OnComponentRemoved(component);
            component.Dispose();
        }

        components.Clear();
    }

    #endregion DISPOSABLE
}
