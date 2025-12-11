// <copyright file="ResolvedProperty.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Toolkit.Utilities;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Behaviors.Aspects;

/// <summary>
///     A property which is initialized from outside a behavior or other aspectable context using an initializer
///     <see cref="Aspect{TValue,TContext}" />.
/// </summary>
/// <typeparam name="TValue">The value type of the property.</typeparam>
public class ResolvedProperty<TValue>
{
    private readonly AspectableProxy proxy;

    private TValue value;
    private Boolean isInitialized;

    private ResolvedProperty(TValue initial, Aspect<TValue, Void> initializer, AspectableProxy proxy)
    {
        this.proxy = proxy;

        value = initial;
        Initializer = initializer;
    }

    /// <summary>
    ///     Aspect used to initialize the property.
    /// </summary>
    public Aspect<TValue, Void> Initializer { get; }

    /// <summary>
    ///     Creates a new resolved property with the given name and optional initial value.
    /// </summary>
    /// <param name="name">The name of the property. Use <c>nameof(...)</c> to get the correct name.</param>
    /// <param name="initial">The initial value of the property before initialization.</param>
    /// <typeparam name="TStrategy">
    ///     The contribution strategy used to initialize the property. Use <see cref="Void" /> as the
    ///     context type.
    /// </typeparam>
    /// <returns>>The created resolved property.</returns>
    public static ResolvedProperty<TValue> New<TStrategy>(String name, TValue initial = default!) where TStrategy : IContributionStrategy<TValue, Void>, new()
    {
        AspectableProxy proxy = new();
        Aspect<TValue, Void> initializer = Aspect<TValue, Void>.New<TStrategy>($"{name}Initializer", proxy);

        return new ResolvedProperty<TValue>(initial, initializer, proxy);
    }

    /// <summary>
    ///     Initializes the resolved property with the given owner.
    ///     Must be called before accessing the property.
    /// </summary>
    /// <param name="owner">The owner aspectable.</param>
    public void Initialize(IAspectable owner)
    {
        if (isInitialized) return;

        proxy.SetOwner(owner);

        value = Initializer.GetValue(value, Void.Instance);
        isInitialized = true;
    }

    /// <summary>
    ///     Get the value of the resolved property. Make sure to call <see cref="Initialize(IAspectable)" /> before accessing
    ///     the property.
    /// </summary>
    /// <returns>>The value of the resolved property.</returns>
    public TValue Get()
    {
        if (!isInitialized) throw Exceptions.InvalidOperation($"Attempted to access uninitialized resolved property of type {typeof(TValue)}.");

        return value;
    }

    /// <summary>
    ///     Overrides the value of the resolved property.
    ///     Must be initialized before calling this method.
    /// </summary>
    /// <param name="newValue">The new value to set.</param>
    public void Override(TValue newValue)
    {
        if (!isInitialized) throw Exceptions.InvalidOperation($"Attempted to override uninitialized resolved property of type {typeof(TValue)}.");

        value = newValue;
    }

    private sealed class AspectableProxy : IAspectable
    {
        public event EventHandler<IAspectable.ValidationEventArgs>? Validation;

        public void SetOwner(IAspectable owner)
        {
            owner.Validation += (s, e) => Validation?.Invoke(s, e);
        }
    }
}
