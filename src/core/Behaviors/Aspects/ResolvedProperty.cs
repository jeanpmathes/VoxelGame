// <copyright file = "ResolvedProperty.cs" company = "VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using VoxelGame.Toolkit.Utilities;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Behaviors.Aspects;

/// <summary>
/// A property which is initialized from outside a behavior or other aspectable context using an initializer <see cref="Aspect{TValue,TContext}"/>.
/// </summary>
/// <typeparam name="TValue">The value type of the property.</typeparam>
public class ResolvedProperty<TValue>
{
    private readonly AspectableProxy proxy;
    
    private TValue value;
    private Boolean isInitialized;
    
    public static ResolvedProperty<TValue> New<TStrategy>(String name, TValue initial = default!) where TStrategy : IContributionStrategy<TValue, Void>, new()
    {
        AspectableProxy proxy = new();
        Aspect<TValue, Void> initializer = Aspect<TValue, Void>.New<TStrategy>($"{name}Initializer", proxy);
        
        return new ResolvedProperty<TValue>(initial, initializer, proxy);
    }

    private class AspectableProxy : IAspectable
    {
        public event EventHandler<IAspectable.ValidationEventArgs>? Validation;
        
        public void SetOwner(IAspectable owner)
        {
            owner.Validation += (s, e) => Validation?.Invoke(s, e);
        }
    }
    
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
    
    public void Initialize(IAspectable owner)
    {
        proxy.SetOwner(owner);
        
        value = Initializer.GetValue(value, Void.Instance);
        isInitialized = true;
    }

    public TValue Get()
    {
        if (!isInitialized)
        {
            throw Exceptions.InvalidOperation($"Attempted to access uninitialized resolved property of type {typeof(TValue)}.");
        }
            
        return value;
    }
    
    public void Override(TValue newValue)
    {
        if (!isInitialized)
        {
            throw Exceptions.InvalidOperation($"Attempted to override uninitialized resolved property of type {typeof(TValue)}.");
        }
        
        value = newValue;
    }
}
