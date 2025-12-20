// <copyright file="ResolvedProperty.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
