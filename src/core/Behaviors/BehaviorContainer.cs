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
using VoxelGame.Core.Behaviors.Events;

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
    private TBehavior?[]? baked;
    private TSelf Self => (TSelf) this;

    /// <inheritdoc />
    public IEnumerable<TBehavior> Behaviors => behaviors;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Boolean Has<TConcreteBehavior>() where TConcreteBehavior : class, TBehavior, IBehavior<TConcreteBehavior, TBehavior, TSelf>
    {
        return Get<TConcreteBehavior>() != null;
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TConcreteBehavior? Get<TConcreteBehavior>() where TConcreteBehavior : class, TBehavior, IBehavior<TConcreteBehavior, TBehavior, TSelf>
    {
        if (baked == null) return behaviors.OfType<TConcreteBehavior>().FirstOrDefault();

        Int32 id = IBehavior<TConcreteBehavior, TBehavior, TSelf>.ID;

        return id < baked.Length ? (TConcreteBehavior?) baked[id] : null;
    }

    /// <inheritdoc />
    public TConcreteBehavior Require<TConcreteBehavior>() where TConcreteBehavior : class, TBehavior, IBehavior<TConcreteBehavior, TBehavior, TSelf>
    {
        var behavior = Get<TConcreteBehavior>();

        if (behavior != null)
            return behavior;

        behavior = IBehavior<TConcreteBehavior, TBehavior, TSelf>.Create(Self);

        behaviors.Add(behavior);

        return behavior;
    }

    /// <inheritdoc />
    public virtual void DefineEvents(IEventRegistry registry) {}

    /// <inheritdoc />
    public virtual void SubscribeToEvents(IEventBus bus) {}

    /// <inheritdoc />
    public void Bake(TBehavior?[] array)
    {
        Debug.Assert(baked == null);
        baked = array;
    }

    /// <inheritdoc />
    public void Validate()
    {
        ValidateSelf();

        foreach (TBehavior behavior in behaviors) behavior.Validate();
    }

    /// <summary>
    ///     Override this method to perform additional validation on the container itself.
    /// </summary>
    protected virtual void ValidateSelf() {}
}
