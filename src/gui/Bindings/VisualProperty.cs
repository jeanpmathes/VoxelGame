// <copyright file="VisualProperty.cs" company="VoxelGame">
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
using VoxelGame.GUI.Visuals;

namespace VoxelGame.GUI.Bindings;

/// <summary>
///     Abstract base class for visual properties.
/// </summary>
public class VisualProperty : IValueSource
{
    private readonly Visual owner;
    private readonly Invalidation invalidation;

    /// <summary>
    ///     Initializes a new visual property base with its owner and invalidation behavior.
    /// </summary>
    /// <param name="owner">The visual that owns this property.</param>
    /// <param name="invalidation">The invalidation to trigger when the property changes.</param>
    protected VisualProperty(Visual owner, Invalidation invalidation)
    {
        this.owner = owner;
        this.invalidation = invalidation;
    }

    /// <inheritdoc />
    public event EventHandler? ValueChanged;

    /// <summary>
    ///     Create a new property with the given default binding and invalidation behavior.
    /// </summary>
    /// <param name="owner">The visual that owns the property.</param>
    /// <param name="defaultBinding">The default binding for the property.</param>
    /// <param name="invalidation">The invalidation behavior when the property value changes.</param>
    /// <typeparam name="T">The type of value stored in the property.</typeparam>
    /// <returns>The created property.</returns>
    public static VisualProperty<T> Create<T>(Visual owner, Binding<T> defaultBinding, Invalidation invalidation)
    {
        return new VisualProperty<T>(owner, invalidation, defaultBinding);
    }

    /// <summary>
    ///     Create a new property with a constant default value and invalidation behavior.
    /// </summary>
    /// <param name="owner">The visual that owns the property.</param>
    /// <param name="defaultValue">The default value for the property.</param>
    /// <param name="invalidation">The invalidation behavior when the property value changes.</param>
    /// <typeparam name="T">The type of value stored in the property.</typeparam>
    /// <returns>The created property.</returns>
    public static VisualProperty<T> Create<T>(Visual owner, T defaultValue, Invalidation invalidation)
    {
        return Create(owner, Binding.Constant(defaultValue), invalidation);
    }

    /// <summary>
    ///     Create a new property with the given default binding, invalidation behavior and a callback for when the value
    ///     changes.
    /// </summary>
    /// <param name="owner">The visual that owns the property.</param>
    /// <param name="defaultBinding">The default binding for the property.</param>
    /// <param name="onChanged">
    ///     A callback that is invoked when the property value changes. The new value is passed as an
    ///     argument.
    /// </param>
    /// <param name="invalidation">The invalidation behavior when the property value changes.</param>
    /// <typeparam name="T">The type of value stored in the property.</typeparam>
    /// <returns>The created property.</returns>
    public static VisualProperty<T> Create<T>(Visual owner, Binding<T> defaultBinding, Action<T> onChanged, Invalidation invalidation = Invalidation.None)
    {
        VisualProperty<T> property = Create(owner, defaultBinding, invalidation);
        property.ValueChanged += (_, _) => onChanged(property.GetValue());
        return property;
    }

    /// <summary>
    ///     Create a new property with a constant default value, invalidation behavior and a callback for when the value
    ///     changes.
    /// </summary>
    /// <param name="owner">The visual that owns the property.</param>
    /// <param name="defaultValue">The default value for the property.</param>
    /// <param name="onChanged">
    ///     A callback that is invoked when the property value changes. The new value is passed as an
    ///     argument.
    /// </param>
    /// <param name="invalidation">The invalidation behavior when the property value changes.</param>
    /// <typeparam name="T">The type of value stored in the property.</typeparam>
    /// <returns>The created property.</returns>
    public static VisualProperty<T> Create<T>(Visual owner, T defaultValue, Action<T> onChanged, Invalidation invalidation = Invalidation.None)
    {
        VisualProperty<T> property = Create(owner, defaultValue, invalidation);
        property.ValueChanged += (_, _) => onChanged(property.GetValue());
        return property;
    }

    /// <summary>
    ///     Notifies subscribers that the value of the property has changed.
    /// </summary>
    protected void NotifyValueChanged()
    {
        switch (invalidation)
        {
            case Invalidation.Measure:
                owner.InvalidateMeasure();
                break;

            case Invalidation.Arrange:
                owner.InvalidateArrange();
                break;

            case Invalidation.Render:
                owner.InvalidateRender();
                break;
        }

        ValueChanged?.Invoke(owner, EventArgs.Empty);
    }
}

/// <summary>
///     A visual property is a value of a <see cref="Visual" /> that can be set and bound locally and internally.
///     The local level is the level that should be used in templates and has the highest priority.
///     The internal level is the level that should be used by the visual itself and has the second-highest priority.
/// </summary>
/// <typeparam name="T">The type of value stored in the property.</typeparam>
public sealed class VisualProperty<T> : VisualProperty, IValueSource<T>
{
    private readonly Binding<T> defaultBinding;

    private Binding<T>? internalBinding;
    private Binding<T>? localBinding;

    private Binding<T> targetBinding;
    private Boolean isActive;

    private T? cachedValue;
    private Boolean isCacheValid;

    /// <summary>
    ///     Initializes a new visual property with default binding and invalidation behavior.
    /// </summary>
    /// <param name="owner">The visual that owns this property.</param>
    /// <param name="invalidation">The invalidation to trigger when the property changes.</param>
    /// <param name="defaultBinding">The default value binding.</param>
    internal VisualProperty(Visual owner, Invalidation invalidation, Binding<T> defaultBinding) : base(owner, invalidation)
    {
        this.defaultBinding = defaultBinding;

        targetBinding = defaultBinding;

        owner.AttachedToRoot += (_, _) => Activate();
        owner.DetachedFromRoot += (_, _) => Deactivate();
    }

    /// <inheritdoc />
    public T GetValue()
    {
        if (!isActive)
            return targetBinding.GetValue();

        if (!isCacheValid)
            UpdateCachedValue(notify: false);

        return cachedValue!;
    }

    /// <summary>
    ///     Activates the property, causing it to subscribe to changes in its active binding.
    /// </summary>
    public void Activate()
    {
        if (isActive) return;
        isActive = true;

        AttachTargetBinding();
        UpdateCachedValue(notify: true);
    }

    /// <summary>
    ///     Deactivates the property, causing it to unsubscribe from changes in its active binding.
    /// </summary>
    public void Deactivate()
    {
        if (!isActive) return;
        isActive = false;

        DetachTargetBinding();
    }

    private void RecomputeTargetBinding()
    {
        Binding<T> binding = localBinding ?? internalBinding ?? defaultBinding;

        if (ReferenceEquals(binding, targetBinding))
            return;

        if (isActive)
        {
            DetachTargetBinding();
        }

        targetBinding = binding;
        isCacheValid = false;

        if (isActive)
        {
            AttachTargetBinding();
            UpdateCachedValue(notify: true);
        }
    }

    private void AttachTargetBinding()
    {
        targetBinding.ValueChanged += OnTargetBindingValueChanged;
    }

    private void DetachTargetBinding()
    {
        targetBinding.ValueChanged -= OnTargetBindingValueChanged;
    }

    private void OnTargetBindingValueChanged(Object? sender, EventArgs e)
    {
        UpdateCachedValue(notify: true);
    }

    private void UpdateCachedValue(Boolean notify)
    {
        T? oldValue = cachedValue;
        T newValue = targetBinding.GetValue();

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

    #region INTERNAL

    /// <summary>
    ///     Bind the property internally.
    /// </summary>
    /// <param name="newInternalBinding">The new internal binding for the property.</param>
    public void Set(Binding<T> newInternalBinding)
    {
        internalBinding = newInternalBinding;
        RecomputeTargetBinding();
    }

    /// <summary>
    ///     Bind the property to a constant value internally.
    /// </summary>
    /// <param name="newValue">The new constant value for the property.</param>
    public void Set(T newValue)
    {
        Set(Bindings.Binding.Constant(newValue));
    }

    /// <summary>
    ///     Clears the internal binding of the property, causing it to fall back to the local binding or default value.
    /// </summary>
    public void Clear()
    {
        internalBinding = null;
        RecomputeTargetBinding();
    }

    #endregion INTERNAL

    #region LOCAL

    private void SetLocal(Binding<T> newLocalBinding)
    {
        localBinding = newLocalBinding;
        RecomputeTargetBinding();
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
}
