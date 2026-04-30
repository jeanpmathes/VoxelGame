// <copyright file="Binding.cs" company="VoxelGame">
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

namespace VoxelGame.GUI.Bindings;

/// <summary>
///     Utility class for defining bindings.
/// </summary>
public static class Binding
{
    /// <summary>
    ///     Create a read-only binding that binds to a constant value.
    /// </summary>
    /// <param name="value">The constant value.</param>
    /// <typeparam name="T">The type of value stored in the binding.</typeparam>
    /// <returns>The created binding.</returns>
    public static Binding<T> Constant<T>(T value)
    {
        return new Binding<T>(() => value, setter: null, []);
    }

    /// <summary>
    ///     Create a read-only parametrized binding that ignores its input and always returns a constant value.
    /// </summary>
    /// <param name="value">The constant value.</param>
    /// <typeparam name="TIn">The type of the input parameter (ignored).</typeparam>
    /// <typeparam name="TOut">The type of value stored in the binding.</typeparam>
    /// <returns>The created binding.</returns>
    public static Binding<TIn, TOut> Constant<TIn, TOut>(TOut value)
    {
        return new Binding<TIn, TOut>(_ => value, []);
    }

    /// <summary>
    ///     Create a read-only computed binding. Note that using other value sources inside the getter will not automatically
    ///     trigger change notifications.
    /// </summary>
    /// <param name="getter">The getter function.</param>
    /// <typeparam name="T">The type of value stored in the binding.</typeparam>
    /// <returns>The created binding.</returns>
    public static Binding<T> Computed<T>(Func<T> getter)
    {
        return new Binding<T>(getter, setter: null, []);
    }

    /// <summary>
    ///     Create a read-only parametrized computed binding. Note that using other value sources inside the getter will not
    ///     automatically trigger change notifications.
    /// </summary>
    /// <param name="getter">The getter function taking the input value and returning the output value.</param>
    /// <typeparam name="TIn">The type of the input parameter.</typeparam>
    /// <typeparam name="TOut">The type of value stored in the binding.</typeparam>
    /// <returns>The created binding.</returns>
    public static Binding<TIn, TOut> Computed<TIn, TOut>(Func<TIn, TOut> getter)
    {
        return new Binding<TIn, TOut>(getter, []);
    }

    /// <summary>
    ///     Bind directly to a parametrized value source, using a constant value as input.
    /// </summary>
    /// <param name="source">The parametrized value source.</param>
    /// <param name="input">The constant value to supply as input.</param>
    /// <typeparam name="TIn">The type of the input parameter of the value source.</typeparam>
    /// <typeparam name="TOut">The type of value stored in the binding, which must match the output type of the value source.</typeparam>
    /// <returns>The created binding.</returns>
    public static Binding<TOut> To<TIn, TOut>(IValueSource<TIn, TOut> source, TIn input)
    {
        return new Binding<TOut>(() => source.GetValue(input), setter: null, [source]);
    }

    /// <summary>
    ///     Bind directly to a parametrized value source, creating a parametrized binding.
    /// </summary>
    /// <param name="source">The parametrized value source.</param>
    /// <typeparam name="TIn">The type of the input parameter of the value source.</typeparam>
    /// <typeparam name="TOut">The type of value stored in the binding.</typeparam>
    /// <returns>The created parametrized binding.</returns>
    public static Binding<TIn, TOut> To<TIn, TOut>(IValueSource<TIn, TOut> source)
    {
        return new Binding<TIn, TOut>(source.GetValue, [source]);
    }

    /// <summary>
    ///     Bind directly to a value source.
    /// </summary>
    /// <param name="source">The value source.</param>
    /// <typeparam name="T">The type of value stored in the binding.</typeparam>
    /// <returns>The created binding.</returns>
    public static Binding<T> To<T>(IValueSource<T> source)
    {
        return new Binding<T>(source.GetValue, setter: null, [source]);
    }

    /// <summary>
    ///     Bind directly to two value sources, creating a tuple of their values.
    /// </summary>
    /// <param name="source1">The first value source.</param>
    /// <param name="source2">The second value source.</param>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <returns>The created binding.</returns>
    public static Binding<(T1, T2)> To<T1, T2>(IValueSource<T1> source1, IValueSource<T2> source2)
    {
        return new Binding<(T1, T2)>(() => (source1.GetValue(), source2.GetValue()), setter: null, [source1, source2]);
    }

    /// <summary>
    ///     Bind directly to three value sources, creating a tuple of their values.
    /// </summary>
    /// <param name="source1">The first value source.</param>
    /// <param name="source2">The second value source.</param>
    /// <param name="source3">The third value source.</param>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <typeparam name="T3">The type of the third value.</typeparam>
    /// <returns>The created binding.</returns>
    public static Binding<(T1, T2, T3)> To<T1, T2, T3>(IValueSource<T1> source1, IValueSource<T2> source2, IValueSource<T3> source3)
    {
        return new Binding<(T1, T2, T3)>(() => (source1.GetValue(), source2.GetValue(), source3.GetValue()), setter: null, [source1, source2, source3]);
    }

    extension<T1, T2>(Binding<(T1, T2)> binding)
    {
        /// <summary>
        ///     Deconstructing overload of <see cref="Binding{T}.Select{TSelected}(Func{T,IValueSource{TSelected}})" /> for
        ///     two-source tuple bindings.
        /// </summary>
        public Binding<TResult> Select<TResult>(Func<T1, T2, IValueSource<TResult>> selector)
        {
            return binding.Select(t => selector(t.Item1, t.Item2));
        }

        /// <summary>
        ///     Deconstructing overload of <see cref="Binding{T}.Select{TSelected}(Func{T,IValueSource{TSelected}?},TSelected)" />
        ///     for two-source tuple bindings.
        /// </summary>
        public Binding<TResult> Select<TResult>(Func<T1, T2, IValueSource<TResult>?> selector,
            TResult defaultValue)
        {
            return binding.Select(t => selector(t.Item1, t.Item2), defaultValue);
        }

        /// <summary>
        ///     Deconstructing overload of <see cref="Binding{T}.Compute{TSelected}" /> for two-source tuple bindings.
        /// </summary>
        public Binding<TResult> Compute<TResult>(Func<T1, T2, TResult> computer)
        {
            return binding.Compute(t => computer(t.Item1, t.Item2));
        }
    }

    extension<T1, T2, T3>(Binding<(T1, T2, T3)> binding)
    {
        /// <summary>
        ///     Deconstructing overload of <see cref="Binding{T}.Select{TSelected}(Func{T,IValueSource{TSelected}})" /> for
        ///     three-source tuple bindings.
        /// </summary>
        public Binding<TResult> Select<TResult>(Func<T1, T2, T3, IValueSource<TResult>> selector)
        {
            return binding.Select(t => selector(t.Item1, t.Item2, t.Item3));
        }

        /// <summary>
        ///     Deconstructing overload of <see cref="Binding{T}.Select{TSelected}(Func{T,IValueSource{TSelected}?},TSelected)" />
        ///     for three-source tuple bindings.
        /// </summary>
        public Binding<TResult> Select<TResult>(Func<T1, T2, T3, IValueSource<TResult>?> selector,
            TResult defaultValue)
        {
            return binding.Select(t => selector(t.Item1, t.Item2, t.Item3), defaultValue);
        }

        /// <summary>
        ///     Deconstructing overload of <see cref="Binding{T}.Compute{TSelected}" /> for three-source tuple bindings.
        /// </summary>
        public Binding<TResult> Compute<TResult>(Func<T1, T2, T3, TResult> computer)
        {
            return binding.Compute(t => computer(t.Item1, t.Item2, t.Item3));
        }
    }

    extension<T1, T2, T3, T4>(Binding<(T1, T2, T3, T4)> binding)
    {
        /// <summary>
        ///     Deconstructing overload of <see cref="Binding{T}.Select{TSelected}(Func{T,IValueSource{TSelected}})" /> for
        ///     four-source tuple bindings.
        /// </summary>
        public Binding<TResult> Select<TResult>(Func<T1, T2, T3, T4, IValueSource<TResult>> selector)
        {
            return binding.Select(t => selector(t.Item1, t.Item2, t.Item3, t.Item4));
        }

        /// <summary>
        ///     Deconstructing overload of <see cref="Binding{T}.Select{TSelected}(Func{T,IValueSource{TSelected}?},TSelected)" />
        ///     for four-source tuple bindings.
        /// </summary>
        public Binding<TResult> Select<TResult>(Func<T1, T2, T3, T4, IValueSource<TResult>?> selector,
            TResult defaultValue)
        {
            return binding.Select(t => selector(t.Item1, t.Item2, t.Item3, t.Item4), defaultValue);
        }

        /// <summary>
        ///     Deconstructing overload of <see cref="Binding{T}.Compute{TSelected}" /> for four-source tuple bindings.
        /// </summary>
        public Binding<TResult> Compute<TResult>(Func<T1, T2, T3, T4, TResult> computer)
        {
            return binding.Compute(t => computer(t.Item1, t.Item2, t.Item3, t.Item4));
        }
    }
}

/// <summary>
///     Binds a property to a slot.
/// </summary>
/// <typeparam name="T">The type of value stored in the binding.</typeparam>
public sealed class Binding<T> : IValueSource<T>
{
    private readonly Func<T> getter;
    private readonly Action<T>? setter;
    private T? cachedValue;
    private Boolean isCacheValid;

    internal Binding(Func<T> getter, Action<T>? setter, IValueSource[] dependencies)
    {
        this.getter = getter;
        this.setter = setter;

        foreach (IValueSource dependency in dependencies)
            dependency.ValueChanged += OnDependencyValueChanged;
    }

    /// <inheritdoc />
    public T GetValue()
    {
        if (!isCacheValid)
        {
            cachedValue = getter();
            isCacheValid = true;
        }

        return cachedValue!;
    }

    /// <inheritdoc />
    public event EventHandler? ValueChanged;

    private void OnDependencyValueChanged(Object? sender, EventArgs e)
    {
        T? oldValue = cachedValue;
        T newValue = getter();

        if (isCacheValid && EqualityComparer<T>.Default.Equals(oldValue, newValue))
            return;

        cachedValue = newValue;
        isCacheValid = true;

        ValueChanged?.Invoke(sender, EventArgs.Empty);
    }

    /// <summary>
    ///     Create a new binding that selects a value from this binding's value using the provided selector function.
    ///     This will correctly handle subscription changes if the selector returns a different inner source when the outer
    ///     value changes.
    /// </summary>
    /// <param name="selector">
    ///     A function that selects an inner value source from the current value of this binding.
    ///     Must not contain any calls to <see cref="IValueSource{T}.GetValue" />.
    /// </param>
    /// <typeparam name="TSelected">The type of the selected value.</typeparam>
    /// <returns>>The created binding.</returns>
    public Binding<TSelected> Select<TSelected>(Func<T, IValueSource<TSelected>> selector)
    {
        Relay<TSelected> relay = new(default!);

        ValueChanged += (_, _) => relay.SetInner(selector(GetValue()));
        relay.SetInner(selector(GetValue()));

        return Binding.To(relay);
    }

    /// <summary>
    ///     Contrary to the non-nullable overload, this variant allows the selector to return <c>null</c>.
    ///     It creates a new binding that selects a value from this binding's value using the provided selector function,
    ///     returning <paramref name="defaultValue" /> when the selector returns <c>null</c>.
    /// </summary>
    /// <param name="selector">
    ///     A function that selects an inner value source from the current value of this binding.
    ///     May return <c>null</c>, in which case <paramref name="defaultValue" /> is used.
    ///     Must not contain any calls to <see cref="IValueSource{T}.GetValue" />.
    /// </param>
    /// <param name="defaultValue">The value to return when the selected inner source is <c>null</c>.</param>
    /// <typeparam name="TSelected">The type of the selected value.</typeparam>
    /// <returns>The created binding.</returns>
    public Binding<TSelected> Select<TSelected>(Func<T, IValueSource<TSelected>?> selector, TSelected defaultValue)
    {
        Relay<TSelected> relay = new(defaultValue);

        ValueChanged += (_, _) => relay.SetInner(selector(GetValue()));
        relay.SetInner(selector(GetValue()));

        return Binding.To(relay);
    }

    /// <summary>
    ///     Create a new binding that selects a parametrized value source from this binding's value using the provided selector
    ///     function.
    ///     This will correctly handle subscription changes if the selector returns a different inner source when the outer
    ///     value changes.
    /// </summary>
    /// <param name="selector">
    ///     A function that selects an inner parametrized value source from the current value of this
    ///     binding. Must not contain any calls to <see cref="IValueSource{T}.GetValue" />.
    /// </param>
    /// <typeparam name="TIn">The type of the input parameter of the selected value source.</typeparam>
    /// <typeparam name="TOut">The type of the value stored in the selected value source.</typeparam>
    /// <returns>The created binding.</returns>
    public Binding<TIn, TOut> Select<TIn, TOut>(Func<T, IValueSource<TIn, TOut>> selector)
    {
        Relay<TIn, TOut> relay = new(default!);

        ValueChanged += (_, _) => relay.SetInner(selector(GetValue()));
        relay.SetInner(selector(GetValue()));

        return Binding.To(relay);
    }

    /// <summary>
    ///     Contrary to the non-nullable overload, this variant allows the selector to return <c>null</c>.
    ///     It creates a new binding that selects a parametrized value source from this binding's value using the provided
    ///     selector function, returning a constant source of <paramref name="defaultValue" /> when the selector returns
    ///     <c>null</c>.
    /// </summary>
    /// <param name="selector">
    ///     A function that selects an inner parametrized value source from the current value of this
    ///     binding. May return <c>null</c>, in which case a constant source of <paramref name="defaultValue" /> is used. Must
    ///     not contain any calls to <see cref="IValueSource{T}.GetValue" />.
    /// </param>
    /// <param name="defaultValue">The value to return when the selected inner source is <c>null</c>.</param>
    /// <typeparam name="TIn">The type of the input parameter of the selected value source.</typeparam>
    /// <typeparam name="TOut">The type of the value stored in the selected value source.</typeparam>
    /// <returns>The created binding.</returns>
    public Binding<TIn, TOut> Select<TIn, TOut>(Func<T, IValueSource<TIn, TOut>?> selector, TOut defaultValue)
    {
        Relay<TIn, TOut> relay = new(defaultValue);

        ValueChanged += (_, _) => relay.SetInner(selector(GetValue()));
        relay.SetInner(selector(GetValue()));

        return Binding.To(relay);
    }

    /// <summary>
    ///     Create a new binding that computes a value from this binding's value using the provided selector function.
    ///     Note that using other value sources inside the selector will not automatically trigger change notifications.
    /// </summary>
    /// <param name="computer">
    ///     A function that computes a value from the current value of this binding.
    ///     Must not contain any calls to <see cref="IValueSource{T}.GetValue" />.
    /// </param>
    /// <typeparam name="TSelected">The type of the selected value.</typeparam>
    /// <returns>The created binding.</returns>
    public Binding<TSelected> Compute<TSelected>(Func<T, TSelected> computer)
    {
        return new Binding<TSelected>(() => computer(GetValue()), setter: null, [this]);
    }

    /// <summary>
    ///     Create a new binding that safely casts a value using <c>as</c>, potentially returning null.
    /// </summary>
    /// <typeparam name="TTarget">The target type to cast to.</typeparam>
    /// <returns>The casting binding.</returns>
    public Binding<TTarget?> Cast<TTarget>() where TTarget : class
    {
        return new Binding<TTarget?>(() => GetValue() as TTarget, setter: null, [this]);
    }

    /// <summary>
    ///     Create a new binding that combines this binding with another value source, creating a tuple of their values.
    /// </summary>
    /// <param name="other">The other value source to combine with this binding.</param>
    /// <typeparam name="TOther">The type of the value in the other source.</typeparam>
    /// <returns>The created binding.</returns>
    public Binding<(T, TOther)> Combine<TOther>(IValueSource<TOther> other)
    {
        return new Binding<(T, TOther)>(() => (GetValue(), other.GetValue()), setter: null, [this, other]);
    }

    /// <summary>
    ///     Combine this binding with two other value sources, creating a tuple of their values.
    /// </summary>
    /// <param name="other1">The first other value source to combine with this binding.</param>
    /// <param name="other2">The second other value source to combine with this binding.</param>
    /// <typeparam name="TOther1">The type of the value in the first other source.</typeparam>
    /// <typeparam name="TOther2">The type of the value in the second other source.</typeparam>
    /// <returns>The created binding.</returns>
    public Binding<(T, TOther1, TOther2)> Combine<TOther1, TOther2>(IValueSource<TOther1> other1, IValueSource<TOther2> other2)
    {
        return new Binding<(T, TOther1, TOther2)>(() => (GetValue(), other1.GetValue(), other2.GetValue()), setter: null, [this, other1, other2]);
    }

    /// <summary>
    ///     Combine this binding with three other value sources, creating a tuple of their values.
    /// </summary>
    /// <param name="other1">The first other value source to combine with this binding.</param>
    /// <param name="other2">The second other value source to combine with this binding.</param>
    /// <param name="other3">The third other value source to combine with this binding.</param>
    /// <typeparam name="TOther1">The type of the value in the first other source.</typeparam>
    /// <typeparam name="TOther2">The type of the value in the second other source.</typeparam>
    /// <typeparam name="TOther3">The type of the value in the third other source.</typeparam>
    /// <returns>The created binding.</returns>
    public Binding<(T, TOther1, TOther2, TOther3)> Combine<TOther1, TOther2, TOther3>(IValueSource<TOther1> other1, IValueSource<TOther2> other2, IValueSource<TOther3> other3)
    {
        return new Binding<(T, TOther1, TOther2, TOther3)>(() => (GetValue(), other1.GetValue(), other2.GetValue(), other3.GetValue()), setter: null, [this, other1, other2, other3]);
    }

    /// <summary>
    ///     Introduce an input parameter to this binding, creating a new parametrized binding.
    /// </summary>
    /// <param name="operation">The operation to perform using the input value and the current value of this binding.</param>
    /// <typeparam name="TIn">The type of the input parameter.</typeparam>
    /// <typeparam name="TOut">The type of value stored in the created binding.</typeparam>
    /// <returns>The created binding.</returns>
    public Binding<TIn, TOut> Parametrize<TIn, TOut>(Func<TIn, T, TOut> operation)
    {
        return new Binding<TIn, TOut>(input => operation(input, GetValue()), [this]);
    }

    /// <summary>
    ///     Introduce an input parameter to this binding, creating a new parametrized binding.
    /// </summary>
    /// <param name="operation">The operation to perform using the input value and the current value of this binding.</param>
    /// <typeparam name="TInAndOut">
    ///     The type of the input parameter, which is also the type of value stored in the created
    ///     binding.
    /// </typeparam>
    /// <returns>The created binding.</returns>
    public Binding<TInAndOut, TInAndOut> Parametrize<TInAndOut>(Func<TInAndOut, T, TInAndOut> operation)
    {
        return new Binding<TInAndOut, TInAndOut>(input => operation(input, GetValue()), [this]);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return $"{{{GetValue()?.ToString()}}}";
    }
}

internal sealed class Relay<T>(T defaultValue) : IValueSource<T>
{
    private T? cachedValue;
    private Boolean isCacheValid;

    private IValueSource<T>? Inner { get; set; }

    T IValueSource<T>.GetValue()
    {
        if (!isCacheValid)
        {
            cachedValue = ComputeValue();
            isCacheValid = true;
        }

        return cachedValue!;
    }

    public event EventHandler? ValueChanged;

    private T ComputeValue()
    {
        return Inner != null ? Inner.GetValue() : defaultValue;
    }

    internal void SetInner(IValueSource<T>? newInner)
    {
        if (ReferenceEquals(Inner, newInner)) return;

        if (Inner != null)
            Inner.ValueChanged -= OnInnerValueChanged;

        Inner = newInner;

        if (Inner != null)
            Inner.ValueChanged += OnInnerValueChanged;

        OnInnerValueChanged(null);
    }

    private void OnInnerValueChanged(Object? sender, EventArgs e)
    {
        OnInnerValueChanged(sender);
    }

    private void OnInnerValueChanged(Object? sender)
    {
        T? oldValue = cachedValue;
        T newValue = ComputeValue();

        if (isCacheValid && EqualityComparer<T>.Default.Equals(oldValue, newValue))
            return;

        cachedValue = newValue;
        isCacheValid = true;

        ValueChanged?.Invoke(sender, EventArgs.Empty);
    }
}

internal sealed class Relay<TIn, TOut>(TOut defaultValue) : IValueSource<TIn, TOut>
{
    private IValueSource<TIn, TOut>? Inner { get; set; }

    TOut IValueSource<TIn, TOut>.GetValue(TIn input)
    {
        return Inner != null ? Inner.GetValue(input) : defaultValue;
    }

    public event EventHandler? ValueChanged;

    internal void SetInner(IValueSource<TIn, TOut>? newInner)
    {
        if (ReferenceEquals(Inner, newInner)) return;

        if (Inner != null)
            Inner.ValueChanged -= OnInnerValueChanged;

        Inner = newInner;

        if (Inner != null)
            Inner.ValueChanged += OnInnerValueChanged;

        ValueChanged?.Invoke(sender: null, EventArgs.Empty);
    }

    private void OnInnerValueChanged(Object? sender, EventArgs e)
    {
        ValueChanged?.Invoke(sender, EventArgs.Empty);
    }
}

/// <summary>
///     A special parametrized binding that can only provide a value given an input value.
/// </summary>
/// <typeparam name="TIn">The type of the input value.</typeparam>
/// <typeparam name="TOut">The type of the output value.</typeparam>
public sealed class Binding<TIn, TOut> : IValueSource<TIn, TOut>
{
    private readonly Func<TIn, TOut> getter;

    internal Binding(Func<TIn, TOut> getter, IValueSource[] dependencies)
    {
        this.getter = getter;

        foreach (IValueSource dependency in dependencies)
            dependency.ValueChanged += OnDependencyValueChanged;
    }

    /// <inheritdoc />
    public TOut GetValue(TIn input)
    {
        return getter(input);
    }

    /// <inheritdoc />
    public event EventHandler? ValueChanged;

    /// <summary>
    ///     Create a new binding by applying this parametrized binding to an input value source. The created binding will
    ///     update whenever either this binding or the input source updates.
    /// </summary>
    /// <param name="input">The input value source to apply to this binding.</param>
    /// <returns>The created binding.</returns>
    public Binding<TOut> Apply(IValueSource<TIn> input)
    {
        return new Binding<TOut>(() => getter(input.GetValue()), setter: null, [this, input]);
    }

    /// <summary>
    ///     Create a new binding by applying this parametrized binding to a nullable input value source, using a default value
    ///     when the input source returns <c>null</c>. The created binding will update whenever either this binding or the
    ///     input source updates.
    /// </summary>
    /// <param name="input">The nullable input value source to apply to this binding.</param>
    /// <param name="defaultInput">The default value to use when the input source returns <c>null</c>.</param>
    /// <returns>The created binding.</returns>
    public Binding<TOut> Apply(IValueSource<TIn?> input, TIn defaultInput)
    {
        return new Binding<TOut>(() => getter(input.GetValue() ?? defaultInput), setter: null, [this, input]);
    }

    /// <summary>
    ///     Create a new binding by applying this parametrized binding to a nullable input value source, using a default output
    ///     value when the input source returns <c>null</c>. The created binding will update whenever either this binding or
    ///     the input source updates.
    /// </summary>
    /// <param name="input">The nullable input value source to apply to this binding.</param>
    /// <param name="defaultOutput">The default value to return when the input source returns <c>null</c>.</param>
    /// <returns>The created binding.</returns>
    public Binding<TOut> ApplyOr(IValueSource<TIn?> input, TOut defaultOutput)
    {
        return new Binding<TOut>(() =>
            {
                TIn? inputValue = input.GetValue();
                return inputValue != null ? getter(inputValue) : defaultOutput;
            },
            setter: null,
            [this, input]);
    }

    /// <summary>
    ///     Create a new binding by applying this parametrized binding to a constant input value. The created binding will
    ///     update whenever this binding updates.
    /// </summary>
    /// <param name="input">The constant input value to apply to this binding.</param>
    /// <returns>The created binding.</returns>
    public Binding<TOut> Apply(TIn input)
    {
        return new Binding<TOut>(() => getter(input), setter: null, [this]);
    }

    private void OnDependencyValueChanged(Object? sender, EventArgs e)
    {
        ValueChanged?.Invoke(sender, EventArgs.Empty);
    }
}
