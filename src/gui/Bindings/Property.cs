// <copyright file="Property.cs" company="VoxelGame">
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
using System.Diagnostics.CodeAnalysis;
using VoxelGame.GUI.Controls;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.GUI.Bindings;

/// <summary>
///     Non-generic abstract base class for properties.
/// </summary>
public abstract class Property : IValueSource
{
    private readonly Control owner;

    internal Property(Control owner)
    {
        this.owner = owner;
    }

    /// <inheritdoc />
    public event EventHandler? ValueChanged;

    /// <summary>
    ///     Create a new property with the given default binding.
    /// </summary>
    /// <param name="owner">The owner element of the property.</param>
    /// <param name="defaultBinding">The default binding for the property.</param>
    /// <param name="coercionBinding">An optional coercion binding to coerce the bound value.</param>
    /// <typeparam name="T">The type of value stored in the property.</typeparam>
    /// <returns>The created property.</returns>
    public static Property<T> Create<T>(Control owner, Binding<T> defaultBinding, Binding<T, T>? coercionBinding = null)
    {
        return new Property<T>(owner, defaultBinding, coercionBinding);
    }

    /// <summary>
    ///     Create a new property with a constant default value.
    /// </summary>
    /// <param name="owner">The owner element of the property.</param>
    /// <param name="defaultValue">The default value for the property.</param>
    /// <param name="coercionBinding">An optional coercion binding to coerce the bound value.</param>
    /// <typeparam name="T">The type of value stored in the property.</typeparam>
    /// <returns>The created property.</returns>
    public static Property<T> Create<T>(Control owner, T defaultValue, Binding<T, T>? coercionBinding = null)
    {
        return Create(owner, Binding.Constant(defaultValue), coercionBinding);
    }

    /// <summary>
    ///     Style the property with a value or binding.
    /// </summary>
    /// <param name="value">The value or binding, must fit the type of the property.</param>
    internal abstract void Style(Object value);

    /// <summary>
    ///     Clear the styling of the property.
    /// </summary>
    internal abstract void ClearStyle();

    /// <summary>
    ///     Notifies subscribers that the value of the property has changed.
    /// </summary>
    protected void NotifyValueChanged()
    {
        ValueChanged?.Invoke(owner, EventArgs.Empty);
    }
}

/// <summary>
///     A property is a value of a <see cref="Control" /> that can be set, styled and bound to a slot.
/// </summary>
/// <typeparam name="T">The type of value stored in the property.</typeparam>
public sealed class Property<T> : Property, IValueSource<T>
{
    private readonly Binding<T, T>? coercionBinding;
    private Binding<T> defaultBinding;

    private Binding<T>? styleBinding;
    private Binding<T>? localBinding;

    private Binding<T> selectedBinding;
    private Binding<T> effectiveBinding;
    private Boolean isActive;

    private T? cachedValue;
    private Boolean isCacheValid;

    internal Property(Control owner, Binding<T> defaultBinding, Binding<T, T>? coercionBinding) : base(owner)
    {
        this.defaultBinding = defaultBinding;
        this.coercionBinding = coercionBinding;

        selectedBinding = defaultBinding;
        SetEffectiveBinding();

        owner.AttachedToRoot += (_, _) => Activate();
        owner.DetachedFromRoot += (_, _) => Deactivate();
    }

    /// <inheritdoc />
    public T GetValue()
    {
        if (!isActive)
            return effectiveBinding.GetValue();

        if (!isCacheValid)
            UpdateCachedValue(notify: false);

        return cachedValue!;
    }

    [MemberNotNull(nameof(effectiveBinding))]
    private void SetEffectiveBinding()
    {
        effectiveBinding = coercionBinding != null
            ? coercionBinding.Apply(selectedBinding)
            : selectedBinding;
    }

    /// <summary>
    ///     Activates the property, causing it to subscribe to changes in its active binding.
    /// </summary>
    public void Activate()
    {
        if (isActive) return;
        isActive = true;

        AttachEffectiveBinding();
        UpdateCachedValue(notify: true);
    }

    /// <summary>
    ///     Deactivates the property, causing it to unsubscribe from changes in its active binding.
    /// </summary>
    public void Deactivate()
    {
        if (!isActive) return;
        isActive = false;

        DetachEffectiveBinding();
    }

    private void RecomputeSelectedBinding()
    {
        Binding<T> newSelectedBinding = localBinding ?? styleBinding ?? defaultBinding;

        if (ReferenceEquals(newSelectedBinding, selectedBinding))
            return;

        if (isActive)
        {
            DetachEffectiveBinding();
        }

        selectedBinding = newSelectedBinding;
        SetEffectiveBinding();

        if (isActive)
        {
            AttachEffectiveBinding();
            UpdateCachedValue(notify: true);
        }
        else
        {
            isCacheValid = false;
        }
    }

    private void AttachEffectiveBinding()
    {
        effectiveBinding.ValueChanged += OnEffectiveBindingValueChanged;
    }

    private void DetachEffectiveBinding()
    {
        effectiveBinding.ValueChanged -= OnEffectiveBindingValueChanged;
    }

    private void OnEffectiveBindingValueChanged(Object? sender, EventArgs e)
    {
        UpdateCachedValue(notify: true);
    }

    private void UpdateCachedValue(Boolean notify)
    {
        T? oldValue = cachedValue;
        T newValue = effectiveBinding.GetValue();

        if (isCacheValid && EqualityComparer<T>.Default.Equals(oldValue, newValue))
            return;

        cachedValue = newValue;
        isCacheValid = true;

        if (notify)
            NotifyValueChanged();
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return $"{{{GetValue()?.ToString()}}}";
    }

    #region DEFAULT

    /// <summary>
    ///     Overrides the default binding of the property.
    ///     This is used for child controls to override the default bindings of their parent controls.
    /// </summary>
    /// <param name="builder">A function that takes the current default binding and returns the new default binding.</param>
    internal void OverrideDefault(Func<Binding<T>, Binding<T>> builder)
    {
        defaultBinding = builder(defaultBinding);
        RecomputeSelectedBinding();
    }

    /// <summary>
    ///     Overrides the default binding of the property with a constant value.
    ///     This is a convenience method for <see cref="OverrideDefault(Func{Binding{T}, Binding{T}})" /> when the new default
    ///     binding is a constant value.
    /// </summary>
    /// <param name="defaultValue">The new default value.</param>
    internal void OverrideDefault(T defaultValue)
    {
        OverrideDefault(_ => Bindings.Binding.Constant(defaultValue));
    }

    #endregion DEFAULT

    #region LOCAL

    private void SetLocal(Binding<T> newLocalBinding)
    {
        localBinding = newLocalBinding;
        RecomputeSelectedBinding();
    }

    /// <summary>
    ///     Binds the property to a constant value locally.
    /// </summary>
    public T Value
    {
        set => SetLocal(Bindings.Binding.Constant(value));
    }

    /// <summary>
    ///     Binds the property locally.
    /// </summary>
    public Binding<T> Binding
    {
        set => SetLocal(value);
    }

    #endregion LOCAL

    #region STYLE

    private void SetStyle(Binding<T>? newStyleBinding)
    {
        styleBinding = newStyleBinding;
        RecomputeSelectedBinding();
    }

    /// <summary>
    ///     Style the property with a constant value.
    /// </summary>
    /// <param name="value">The constant value.</param>
    public void Style(T value)
    {
        SetStyle(Bindings.Binding.Constant(value));
    }

    /// <summary>
    ///     Style the property with a binding.
    /// </summary>
    /// <param name="binding">The binding.</param>
    public void Style(Binding<T> binding)
    {
        SetStyle(binding);
    }

    internal override void Style(Object value)
    {
        switch (value)
        {
            case T typedValue:
                Style(typedValue);
                break;
            case Binding<T> binding:
                Style(binding);
                break;
            case Binding<T, T> trigger:
                // The coercion will be applied to the value provided by the trigger, so we do not need to pass the coerced binding.
                Style(trigger.Apply(selectedBinding));
                break;
            default:
                throw Exceptions.UnsupportedValue(value);
        }
    }

    /// <summary>
    ///     Clear the styling of the property.
    /// </summary>
    internal override void ClearStyle()
    {
        SetStyle(null);
    }

    #endregion STYLE
}
