﻿// <copyright file="Bindable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Application;

/// <summary>
///     Wraps a value source that allows to bind actions to it.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class Bindable<T>
{
    private readonly Func<T> get;
    private readonly Action<T> set;

    /// <summary>
    ///     Create a new instance of the <see cref="Bindable{T}" /> class.
    /// </summary>
    /// <param name="get">The getter of the wrapped value.</param>
    /// <param name="set">The setter of the wrapped value.</param>
    public Bindable(Func<T> get, Action<T> set)
    {
        this.get = get;
        this.set = set;
    }

    /// <summary>
    ///     Get the accessors of the value.
    /// </summary>
    public (Func<T> Get, Action<T> Set) Accessors => (Get, Set);

    /// <summary>
    ///     Implicitly get the value.
    /// </summary>
    /// <param name="bindable">The bindable to get the value from.</param>
    /// <returns>The value.</returns>
    public static implicit operator T(Bindable<T> bindable)
    {
        return bindable.Get();
    }

    private T Get()
    {
        return get();
    }

    /// <summary>
    ///     Set the value.
    /// </summary>
    /// <param name="newValue">The new value.</param>
    public void Set(T newValue)
    {
        T oldValue = get();

        set(newValue);

        Changed(this, new BindingChangedArgs<T>(oldValue, newValue));
    }

    /// <summary>
    ///     Bind an action to the value.
    /// </summary>
    /// <param name="action">The action to bind. The action will be called on every change and once immediately.</param>
    /// <returns>An <see cref="IDisposable" /> that can be used to unbind the action.</returns>
    public IDisposable Bind(Action<BindingChangedArgs<T>> action)
    {
        EventHandler<BindingChangedArgs<T>> handler = (_, args) => action(args);
        Changed += handler;

        handler(this, new BindingChangedArgs<T>(get(), get()));

        return new Disposer(() => Changed -= handler);
    }

    private event EventHandler<BindingChangedArgs<T>> Changed = delegate {};
}

/// <summary>
///     Event args for when a binding changes.
/// </summary>
/// <param name="OldValue">The old value.</param>
/// <param name="NewValue">The new value.</param>
/// <typeparam name="T">The type of the value.</typeparam>
public record BindingChangedArgs<T>(T OldValue, T NewValue);
