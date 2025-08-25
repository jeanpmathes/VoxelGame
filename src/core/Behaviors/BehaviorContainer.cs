// <copyright file="BehaviorContainer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Behaviors;

/// <summary>
///     An abstract base class for a subject that can contain behaviors.
/// </summary>
/// <typeparam name="TSelf">The type of this behavior container.</typeparam>
/// <typeparam name="TBehavior">The behavior base class type that this container holds.</typeparam>
public abstract class BehaviorContainer<TSelf, TBehavior> : IHasBehaviors<TSelf, TBehavior>
    where TSelf : BehaviorContainer<TSelf, TBehavior>
    where TBehavior : class, IBehavior<TSelf>
{
    private readonly List<TBehavior> behaviors = [];
    private readonly Dictionary<Type, List<Action<TBehavior>>> watchers = new();
    
    private TBehavior?[]? baked;
    
    private TSelf Self => (TSelf) this;

    /// <inheritdoc />
    public IEnumerable<TBehavior> Behaviors => behaviors;

    /// <inheritdoc /> 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Boolean Has<TConcreteBehavior>() where TConcreteBehavior : class, TBehavior, IBehavior<TConcreteBehavior, TBehavior, TSelf>
    {
        return Get<TConcreteBehavior>() != null; // todo: think renaming Has to Is
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TConcreteBehavior? Get<TConcreteBehavior>() where TConcreteBehavior : class, TBehavior, IBehavior<TConcreteBehavior, TBehavior, TSelf>
    {
        if (baked == null) return behaviors.OfType<TConcreteBehavior>().FirstOrDefault();

        Int32 id = IBehavior<TConcreteBehavior, TBehavior, TSelf>.ID;

        if (id == IBehavior.UnknownID)
            return null; // No block is actually using this behavior.

        return id < baked.Length ? (TConcreteBehavior?) baked[id] : null;
    }

    /// <inheritdoc />
    public TConcreteBehavior Require<TConcreteBehavior>() where TConcreteBehavior : class, TBehavior, IBehavior<TConcreteBehavior, TBehavior, TSelf>
    {
        // todo: ensure not called after baking
        
        var behavior = Get<TConcreteBehavior>();

        if (behavior != null)
            return behavior;

        behavior = IBehavior<TConcreteBehavior, TBehavior, TSelf>.Create(Self);

        behaviors.Add(behavior);

        if (!watchers.TryGetValue(typeof(TConcreteBehavior), out List<Action<TBehavior>>? actions)) 
            return behavior;

        foreach (Action<TBehavior> action in actions)
        {
            action(behavior);
        }
            
        watchers.Remove(typeof(TConcreteBehavior));

        return behavior;
    }

    /// <inheritdoc />
    public void RequireIfPresent<TConditionalConcreteBehavior, TConditionConcreteBehavior>(Action<TConditionalConcreteBehavior>? initializer = null) where TConditionalConcreteBehavior : class, TBehavior, IBehavior<TConditionalConcreteBehavior, TBehavior, TSelf> where TConditionConcreteBehavior : class, TBehavior, IBehavior<TConditionConcreteBehavior, TBehavior, TSelf>
    {
        if (Get<TConditionConcreteBehavior>() != null)
        {
            var behavior = Require<TConditionalConcreteBehavior>();
            initializer?.Invoke(behavior);
        }
        else
        {
            watchers.GetOrAdd(typeof(TConditionConcreteBehavior), []).Add(behavior =>
            {
                if (behavior is not TConditionConcreteBehavior) return;

                var conditionalBehavior = Require<TConditionalConcreteBehavior>();
                initializer?.Invoke(conditionalBehavior);
            });
        }
    }

    /// <inheritdoc />
    public virtual void DefineEvents(IEventRegistry registry) {}

    /// <inheritdoc />
    public virtual void SubscribeToEvents(IEventBus bus) {}

    /// <inheritdoc />
    public virtual void Validate(IResourceContext context)
    {
        Validation?.Invoke(this, new IAspectable.ValidationEventArgs { Context = context});
    }
    
    /// <inheritdoc />
    public event EventHandler<IAspectable.ValidationEventArgs>? Validation;

    /// <inheritdoc />
    public void Bake(TBehavior?[] array)
    {
        Debug.Assert(baked == null);
        baked = array;

        watchers.Clear();
        
        OnBake();
    }

    /// <summary>
    /// Is called after baking of this behavior container.
    /// Other containers might not be baked yet.
    /// </summary>
    protected virtual void OnBake()
    {
        
    }
}
