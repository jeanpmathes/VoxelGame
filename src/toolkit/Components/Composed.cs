// <copyright file="Composed.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Toolkit.Components;

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
public abstract class Composed<TSelf, TComponent> : IDisposable
    where TSelf : Composed<TSelf, TComponent>
    where TComponent : Component<TSelf>
{
    private readonly Dictionary<Type, TComponent> components = [];

    /// <summary>
    ///     Get the container itself.
    /// </summary>
    protected abstract TSelf Self { get; }

    /// <summary>
    ///     Get all components attached to this container.
    /// </summary>
    public IEnumerable<TComponent> Components => components.Values;

    /// <summary>
    ///     Add a component of the given type.
    /// </summary>
    public TConcrete AddComponent<TConcrete>() where TConcrete : TComponent, IConstructible<TSelf, TConcrete>
    {
        var component = TConcrete.Construct(Self);

        components.Add(typeof(TConcrete), component);

        return component;
    }

    /// <summary>
    ///     Add a component of the given type.
    /// </summary>
    public TConcrete AddComponent<TConcrete, TConcreteSelf>() where TConcrete : TComponent, IConstructible<TConcreteSelf, TConcrete> where TConcreteSelf : TSelf
    {
        var component = TConcrete.Construct((TConcreteSelf) Self);

        components.Add(typeof(TConcrete), component);

        return component;
    }

    /// <summary>
    ///     Add a component of the given type.
    /// </summary>
    public TConcrete AddComponent<TConcrete, TArgument>(TArgument argument) where TConcrete : TComponent, IConstructible<TSelf, TArgument, TConcrete>
    {
        var component = TConcrete.Construct(Self, argument);

        components.Add(typeof(TConcrete), component);

        return component;
    }

    /// <summary>
    ///     Remove the specified component.
    /// </summary>
    public Boolean RemoveComponent<TConcrete>() where TConcrete : TComponent
    {
        if (!components.TryGetValue(typeof(TConcrete), out TComponent? component)) return false;

        components.Remove(typeof(TConcrete));

        component.Dispose();

        return true;
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
    /// <typeparam name="TConcrete">The type of the component to get or add.</typeparam>
    /// <returns>The component of the specified type.</returns>
    public TConcrete GetRequiredComponent<TConcrete>() where TConcrete : TComponent, IConstructible<TSelf, TConcrete>
    {
        if (GetComponent<TConcrete>() is {} component) return component;

        return AddComponent<TConcrete>();
    }

    /// <summary>
    ///     Check whether a component of the specified type exists.
    /// </summary>
    public Boolean HasComponent<TConcrete>() where TConcrete : TComponent
    {
        return components.ContainsKey(typeof(TConcrete));
    }

    #region DISPOSING

    /// <summary>
    ///     Called by the finalizer or Dispose method.
    /// </summary>
    /// <param name="disposing"><c>true</c> if called from Dispose, <c>false</c> if called from the finalizer.</param>
    protected virtual void Dispose(Boolean disposing)
    {
        if (!disposing)
            return;

        foreach (TComponent component in components.Values) component.Dispose();

        components.Clear();
    }

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

    #endregion DISPOSING
}
