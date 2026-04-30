// <copyright file="Control.cs" company="VoxelGame">
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
using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Controls.Templates;
using VoxelGame.GUI.Graphics;
using VoxelGame.GUI.Input;
using VoxelGame.GUI.Rendering;
using VoxelGame.GUI.Styles;
using VoxelGame.GUI.Themes;
using VoxelGame.GUI.Utilities;
using VoxelGame.GUI.Visuals;

namespace VoxelGame.GUI.Controls;

/// <summary>
///     Non-generic contract for shared control properties.
/// </summary>
public interface IControl
{
    /// <inheritdoc cref="Control.Foreground" />
    public Property<Brush> Foreground { get; }

    /// <inheritdoc cref="Control.Background" />
    public Property<Brush> Background { get; }

    /// <inheritdoc cref="Control.Opacity" />
    public Property<Single> Opacity { get; }

    /// <inheritdoc cref="Control.MinimumWidth" />
    public Property<Single> MinimumWidth { get; }

    /// <inheritdoc cref="Control.MinimumHeight" />
    public Property<Single> MinimumHeight { get; }

    /// <inheritdoc cref="Control.MaximumWidth" />
    public Property<Single> MaximumWidth { get; }

    /// <inheritdoc cref="Control.MaximumHeight" />
    public Property<Single> MaximumHeight { get; }

    /// <inheritdoc cref="Control.Margin" />
    public Property<ThicknessF> Margin { get; }

    /// <inheritdoc cref="Control.Padding" />
    public Property<ThicknessF> Padding { get; }

    /// <inheritdoc cref="Control.HorizontalAlignment" />
    public Property<HorizontalAlignment> HorizontalAlignment { get; }

    /// <inheritdoc cref="Control.VerticalAlignment" />
    public Property<VerticalAlignment> VerticalAlignment { get; }

    /// <inheritdoc cref="Control.IsNavigable" />
    public Property<Boolean> IsNavigable { get; }

    /// <inheritdoc cref="Control.Visibility" />
    public Property<Visibility> Visibility { get; }

    /// <inheritdoc cref="Control.Enablement" />
    public Property<Enablement> Enablement { get; }

    /// <inheritdoc cref="Control.Parent" />
    public ReadOnlySlot<Control?> Parent { get; }

    /// <inheritdoc cref="Control.Children" />
    public ReadOnlyListSlot<Control> Children { get; }

    /// <inheritdoc cref="Control.Context" />
    public Context Context { get; }

    /// <inheritdoc cref="Control.Visualization" />
    public ReadOnlySlot<Visual?> Visualization { get; }

    /// <inheritdoc cref="Control.IsHovered" />
    public IValueSource<Boolean> IsHovered { get; }

    /// <inheritdoc cref="Control.IsKeyboardFocused" />
    public IValueSource<Boolean> IsKeyboardFocused { get; }

    /// <inheritdoc cref="Control.IsPointerFocused" />
    public IValueSource<Boolean> IsPointerFocused { get; }
}

/// <summary>
///     The base class of all controls, meaning logical controls.
/// </summary>
public abstract class Control : IControl
{
    /// <summary>
    ///     Creates a new instance of the <see cref="Control" /> class.
    /// </summary>
    protected Control()
    {
        parent = new Slot<Control?>(value: null, this);

        Foreground = Property.Create(this, BindToParent(p => p.Foreground, Defaults.ForegroundBrush));
        Background = Property.Create(this, Brushes.Transparent);

        Opacity = Property.Create(this, defaultValue: 1f);

        MinimumWidth = Property.Create(this, defaultValue: 1f);
        MinimumHeight = Property.Create(this, defaultValue: 1f);

        MaximumWidth = Property.Create(this, Single.PositiveInfinity);
        MaximumHeight = Property.Create(this, Single.PositiveInfinity);

        Margin = Property.Create(this, ThicknessF.Zero);
        Padding = Property.Create(this, ThicknessF.Zero);

        HorizontalAlignment = Property.Create(this, GUI.HorizontalAlignment.Stretch);
        VerticalAlignment = Property.Create(this, GUI.VerticalAlignment.Stretch);

        IsNavigable = Property.Create(this, defaultValue: false);

        Visibility = Property.Create(this,
            GUI.Visibility.Visible,
            Binding.To(Parent).Select(p => p?.Visibility, GUI.Visibility.Visible).Parametrize<Visibility>(Visibilities.Lower));

        {
            LocalEnablement = Property.Create(this, GUI.Enablement.Enabled);

            Enablement = Property.Create(this,
                GUI.Enablement.Enabled,
                Binding.To(Parent).Select(p => p?.Enablement, GUI.Enablement.Enabled).Combine(LocalEnablement).Compute(Enablements.Lower).Parametrize<Enablement>(Enablements.Lower));
        }
    }

    #region STYLE

    /// <summary>
    ///     Invalidate styling of this control, causing it to be reapplied.
    /// </summary>
    protected abstract void InvalidateStyling();

    #endregion

    #region PROPERTIES

    /// <summary>
    ///     The preferred foreground brush of the control.
    /// </summary>
    public Property<Brush> Foreground { get; }

    /// <summary>
    ///     The preferred background brush of the control.
    /// </summary>
    public Property<Brush> Background { get; }

    /// <summary>
    ///     The opacity of the control. Opacity is applied in a multiplicative fashion when traversing the tree and also
    ///     applied to any used brushes.
    /// </summary>
    public Property<Single> Opacity { get; }

    /// <summary>
    ///     The minimum width of this control. Might not be respected by all layout containers.
    /// </summary>
    public Property<Single> MinimumWidth { get; }

    /// <summary>
    ///     The minimum height of this control. Might not be respected by all layout containers.
    /// </summary>
    public Property<Single> MinimumHeight { get; }

    /// <summary>
    ///     The maximum width of this control. Might not be respected by all layout containers.
    /// </summary>
    public Property<Single> MaximumWidth { get; }

    /// <summary>
    ///     The maximum height of this control. Might not be respected by all layout containers.
    /// </summary>
    public Property<Single> MaximumHeight { get; }

    /// <summary>
    ///     The margin of this control, which is space around the control that the layout system should try to respect.
    /// </summary>
    public Property<ThicknessF> Margin { get; }

    /// <summary>
    ///     The padding of this control, which is space inside the control that the layout system should try to respect.
    ///     If a control defines custom layout logic, it decides if and how to respect the padding.
    ///     As such, padding is less strictly enforced than margin.
    /// </summary>
    public Property<ThicknessF> Padding { get; }

    /// <summary>
    ///     The horizontal alignment of this control within its parent. Might not be respected by all layout containers.
    /// </summary>
    public Property<HorizontalAlignment> HorizontalAlignment { get; }

    /// <summary>
    ///     The vertical alignment of this control within its parent. Might not be respected by all layout containers.
    /// </summary>
    public Property<VerticalAlignment> VerticalAlignment { get; }

    /// <summary>
    ///     Whether this control allows to navigate to it, which is used to move the keyboard focus using the keyboard.
    /// </summary>
    public Property<Boolean> IsNavigable { get; }

    /// <summary>
    ///     The visibility of the control and its visualization.
    ///     Controls cannot be more visible than their parents.
    /// </summary>
    public Property<Visibility> Visibility { get; }

    /// <summary>
    ///     The enablement of the control.
    ///     Controls cannot be more enabled than their parents.
    /// </summary>
    public Property<Enablement> Enablement { get; }

    /// <summary>
    ///     Create a property bound to a property of the parent control.
    /// </summary>
    /// <param name="selector">The selector function to select the property from the parent control.</param>
    /// <param name="defaultValue">The default value to use if there is no parent.</param>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <returns>>The created property.</returns>
    protected Binding<T> BindToParent<T>(Func<Control, IValueSource<T>> selector, T defaultValue)
    {
        return Binding.To(Parent).Select(p => p != null ? selector(p) : null, defaultValue);
    }

    #endregion PROPERTIES

    #region HIERARCHY

    /// <summary>
    ///     Whether this control is attached to a tree with a root control.
    /// </summary>
    protected Boolean IsAttached { get; private set; }

    /// <summary>
    ///     Whether this control is the root control of the tree.
    /// </summary>
    private protected Boolean IsRoot { get; set; }

    /// <summary>
    ///     Set this control as the root of the tree.
    ///     May only be called by the canvas controls.
    /// </summary>
    /// <param name="uiRenderer">The renderer that will be used to render the visuals of this control and its children.</param>
    private protected void SetAsRoot(IRenderer uiRenderer)
    {
        renderer = uiRenderer;

        IsRoot = true;

        Attach();
    }

    private readonly Slot<Control?> parent;
    private readonly ListSlot<Control> children = [];

    /// <summary>
    ///     The parent of this control.
    /// </summary>
    public ReadOnlySlot<Control?> Parent => parent;

    /// <summary>
    ///     The children of this control.
    /// </summary>
    public ReadOnlyListSlot<Control> Children => children;

    /// <summary>
    ///     Set the child of this control.
    ///     Replaces any existing children.
    /// </summary>
    /// <param name="child">
    ///     The child to set. Will be removed from its previous parent if any.
    ///     If <c>null</c>, all existing children will be removed.
    /// </param>
    protected void SetChild(Control? child)
    {
        if (child?.Parent.GetValue() == this && children.Count.GetValue() == 1) return;

        child?.Parent.GetValue()?.RemoveChild(child, isReparenting: true);

        if (children.Count.GetValue() > 0)
        {
            List<Control> oldChildren = new(children);
            children.Clear();

            foreach (Control oldChild in oldChildren)
            {
                oldChild.parent.SetValue(null);

                OnChildRemoved(oldChild);
                oldChild.Detach(isReparenting: false);
            }
        }

        if (child == null) return;

        children.Add(child);
        child.parent.SetValue(this);

        if (IsAttached)
            child.Attach();

        OnChildAdded(child);
    }

    /// <summary>
    ///     Add a  child to this control.
    /// </summary>
    /// <param name="child">The child to add. Will be removed from its previous parent if any.</param>
    protected void AddChild(Control child)
    {
        if (child.Parent.GetValue() == this) return;

        child.Parent.GetValue()?.RemoveChild(child, isReparenting: true);

        children.Add(child);
        child.parent.SetValue(this);

        if (IsAttached)
            child.Attach();

        OnChildAdded(child);
    }

    /// <summary>
    ///     Remove a child from this control.
    ///     If the specified child is not a child of this control, nothing happens.
    /// </summary>
    /// <param name="child">The child to remove.</param>
    protected void RemoveChild(Control child)
    {
        RemoveChild(child, isReparenting: false);
    }

    private void RemoveChild(Control child, Boolean isReparenting)
    {
        if (child.Parent.GetValue() != this) return;

        if (!children.Remove(child)) return;

        child.parent.SetValue(null);

        OnChildRemoved(child);
        child.Detach(isReparenting);
    }

    private void OnChildAdded(Control child)
    {
        ChildAdded?.Invoke(this, new ChildAddedEventArgs(child));
    }

    /// <summary>
    ///     Invoked when a child is added to this control.
    /// </summary>
    public event EventHandler<ChildAddedEventArgs>? ChildAdded;

    private void OnChildRemoved(Control child)
    {
        ChildRemoved?.Invoke(this, new ChildRemovedEventArgs(child));
    }

    /// <summary>
    ///     Invoked when a child is removed from this control.
    /// </summary>
    public event EventHandler<ChildRemovedEventArgs>? ChildRemoved;

    private void Attach()
    {
        if (IsAttached) return;
        IsAttached = true;

        InvalidateContext();
        OnAttach();
        AttachedToRoot?.Invoke(this, EventArgs.Empty);

        foreach (Control child in children)
        {
            child.Attach();
        }
    }

    /// <summary>
    ///     Called when the control is attached to a tree with a root control.
    ///     Note that for example giving this control a parent does not necessarily
    ///     attach it to a root control, as the parent itself may not be attached to a root.
    /// </summary>
    public virtual void OnAttach() {}

    /// <summary>
    ///     Invoked when this control is attached to a tree with a root control.
    /// </summary>
    public event EventHandler? AttachedToRoot;

    private void Detach(Boolean isReparenting)
    {
        if (!IsAttached) return;
        IsAttached = IsRoot;
        if (IsAttached) return;

        InvalidateContext();
        OnDetach(isReparenting);
        DetachedFromRoot?.Invoke(this, EventArgs.Empty);

        foreach (Control child in children)
        {
            child.Detach(isReparenting);
        }
    }

    /// <summary>
    ///     Called when the control is detached from a tree with a root control.
    ///     Note that being detached in most cases does not mean losing the parent,
    ///     as it may simply be that the parent or one of its ancestors was detached.
    /// </summary>
    /// <remarks>
    ///     Generally, disposable resources must be disposed when being detached,
    ///     unless the control is being reparented.
    /// </remarks>
    /// <param name="isReparenting">
    ///     Indicates whether the control is being detached because it is being reparented.
    /// </param>
    public virtual void OnDetach(Boolean isReparenting) {}

    /// <summary>
    ///     Invoked when this control is detached from a tree with a root control.
    /// </summary>
    public event EventHandler? DetachedFromRoot;

    #endregion HIERARCHY

    #region CONTEXT

    private readonly Context? localContext;
    private Context? cachedContext;

    /// <summary>
    ///     Rebuild the cached context for this control and reapply styling.
    /// </summary>
    private void InvalidateContext()
    {
        UpdateCachedContext();

        InvalidateStyling();

        OnInvalidateContext();
    }

    /// <summary>
    ///     Override to react to context changes.
    /// </summary>
    protected virtual void OnInvalidateContext() {}

    [MemberNotNull(nameof(cachedContext))]
    private void UpdateCachedContext()
    {
        Context parentContext = Parent.GetValue()?.Context ?? Context.Default;

        cachedContext = localContext == null
            ? parentContext
            : new Context(localContext, parentContext);
    }

    /// <summary>
    ///     The context of this control, which is used for example to determine styling.
    /// </summary>
    public Context Context
    {
        get
        {
            if (cachedContext != null)
                return cachedContext;

            UpdateCachedContext();

            return cachedContext;
        }

        init
        {
            localContext = value;

            UpdateCachedContext();
        }
    }

    #endregion CONTEXT

    #region VISUALIZATION / TEMPLATING

    private protected IRenderer? renderer;

    /// <summary>
    ///     Build or refresh the visual tree that represents this control.
    /// </summary>
    /// <returns>The root visual used to render this control.</returns>
    internal abstract Visual Visualize();

    /// <summary>
    ///     The current root visual that represents this control if it has been visualized.
    /// </summary>
    public abstract ReadOnlySlot<Visual?> Visualization { get; }

    #endregion VISUALIZATION / TEMPLATING

    #region INPUT

    /// <summary>
    ///     Whether the pointer (mouse) is currently hovering over the anchor visual of this control.
    /// </summary>
    public abstract IValueSource<Boolean> IsHovered { get; }

    /// <summary>
    ///     Whether this control is currently focused for keyboard input, meaning that its anchor visual is the target of the
    ///     keyboard focus.
    /// </summary>
    public abstract IValueSource<Boolean> IsKeyboardFocused { get; }

    /// <summary>
    ///     Whether this control is currently focused for pointer input, meaning that its anchor visual is the target of the
    ///     pointer focus.
    /// </summary>
    public abstract IValueSource<Boolean> IsPointerFocused { get; }

    /// <summary>
    ///     Called when an input event tunnels down the visual tree towards the target visual and reaches the template anchor
    ///     of this control.
    ///     Use this to intercept input events before they reach the target visual.
    /// </summary>
    /// <param name="inputEvent">The input event.</param>
    public virtual void OnInputPreview(InputEvent inputEvent) {}

    /// <summary>
    ///     Called when an input event bubbles up the visual tree from the target visual and reaches the template anchor of
    ///     this control.
    ///     Use this to handle inputs.
    /// </summary>
    /// <param name="inputEvent">The input event.</param>
    public virtual void OnInput(InputEvent inputEvent) {}

    #endregion INPUT

    #region ENABLEMENT

    private Property<Enablement> LocalEnablement { get; }

    /// <summary>
    ///     Add a constraint to the (effective) enablement of this control.
    ///     The constraint will be combined with the existing enablement using <see cref="Enablements.Lower" />.
    ///     Therefore, additional constraints will not allow the control to be more enabled, only less enabled.
    /// </summary>
    /// <param name="constraint">The enablement constraint to add.</param>
    protected void AddEnablementConstraint(Binding<Enablement> constraint)
    {
        LocalEnablement.OverrideDefault(original => Binding.To(original, constraint).Compute(Enablements.Lower));
    }

    #endregion ENABLEMENT
}

/// <summary>
///     The generic base class of all controls, meaning logical controls.
///     The generic variant is needed for templating.
/// </summary>
/// <typeparam name="TSelf">The type of the control itself.</typeparam>
public abstract class Control<TSelf> : Control where TSelf : Control<TSelf>
{
    /// <summary>
    ///     Creates a new instance of the <see cref="Control{TSelf}" /> class.
    /// </summary>
    protected Control()
    {
        visualization = new Slot<Visual?>(value: null, this);

        Template = Property.Create(this, Binding.Computed(CreateDefaultTemplate));
        Template.ValueChanged += OnTemplateChanged;

        Style = Property.Create(this, (Style<TSelf>?) null);
        Style.ValueChanged += OnStyleChanged;

        IsHovered = Binding.To(visualization).Select(v => v?.IsHovered, defaultValue: false);

        IsKeyboardFocused = Binding.To(visualization).Select(v => v?.IsKeyboardFocused, defaultValue: false);
        IsPointerFocused = Binding.To(visualization).Select(v => v?.IsPointerFocused, defaultValue: false);
    }

    /// <summary>
    ///     Get this control as its own type.
    /// </summary>
    protected TSelf Self => (TSelf) this;

    #region VISUALIZATION / TEMPLATING

    /// <summary>
    ///     The template used to visualize this control.
    /// </summary>
    public Property<ControlTemplate<TSelf>> Template { get; }

    private readonly Slot<Visual?> visualization;

    /// <summary>
    ///     The current root visual that represents this control if it has been visualized.
    /// </summary>
    public override ReadOnlySlot<Visual?> Visualization => visualization;

    /// <summary>
    ///     Build or refresh the visual tree for this control and apply current styling.
    /// </summary>
    /// <returns>The root visual used to render this control.</returns>
    internal override Visual Visualize()
    {
        UnanchorVisualization();

        ApplyStyling();

        Visual currentVisualization = Template.GetValue().Apply(Self);
        visualization.SetValue(currentVisualization);

        AnchorVisualization();

        return currentVisualization;
    }

    private void AnchorVisualization()
    {
        Visualization.GetValue()?.SetAsAnchor(this);

        if (IsRoot)
            Visualization.GetValue()?.SetAsRoot(renderer!);
    }

    private void UnanchorVisualization()
    {
        if (Visualization.GetValue() is not {} currentVisualization) return;

        if (IsRoot)
            currentVisualization.UnsetAsRoot();

        currentVisualization.UnsetAsAnchor();

        visualization.SetValue(null);
    }

    /// <summary>
    ///     Create a default template for this control, which is used if no style or local template is set.
    /// </summary>
    /// <returns>The default control template.</returns>
    protected abstract ControlTemplate<TSelf> CreateDefaultTemplate();

    private void OnTemplateChanged(Object? sender, EventArgs e)
    {
        InvalidateVisualization();
    }

    private void InvalidateVisualization()
    {
        if (Visualization.GetValue() == null) return;

        Visualize();
    }

    #endregion VISUALIZATION / TEMPLATING

    #region STYLE

    private IReadOnlyList<IStyle<TSelf>>? usedOuterStyles;
    private Style<TSelf>? usedLocalStyle;

    private Boolean IsStyled => usedOuterStyles != null || usedLocalStyle != null;

    /// <summary>
    ///     Set a specific style just for this control, which overrides any styling from the context.
    ///     This style does not affect any other controls.
    /// </summary>
    public Property<Style<TSelf>?> Style { get; }

    private void OnStyleChanged(Object? sender, EventArgs e)
    {
        InvalidateStyling();
    }

    /// <inheritdoc />
    protected sealed override void InvalidateStyling()
    {
        if (!IsStyled) return;

        ApplyStyling();
    }

    private void ApplyStyling()
    {
        ClearStyling();

        usedOuterStyles = Context.GetStyling<TSelf>();

        if (usedOuterStyles.Count > 0)
        {
            foreach (IStyle<TSelf> style in usedOuterStyles)
            {
                style.Apply(Self);
            }
        }

        usedLocalStyle = Style.GetValue();
        usedLocalStyle?.Apply(Self);
    }

    private void ClearStyling()
    {
        if (!IsStyled) return;

        usedLocalStyle?.Clear(Self);
        usedLocalStyle = null;

        if (usedOuterStyles == null) return;

        for (Int32 index = usedOuterStyles.Count - 1; index >= 0; index--)
        {
            usedOuterStyles[index].Clear(Self);
        }

        usedOuterStyles = null;
    }

    #endregion STYLE

    #region INPUT

    /// <inheritdoc />
    public sealed override IValueSource<Boolean> IsHovered { get; }

    /// <inheritdoc />
    public sealed override IValueSource<Boolean> IsKeyboardFocused { get; }

    /// <inheritdoc />
    public sealed override IValueSource<Boolean> IsPointerFocused { get; }

    #endregion INPUT
}

/// <summary>
///     Event arguments for the <see cref="Control.ChildAdded" /> event.
/// </summary>
/// <param name="child">The child that was added.</param>
public class ChildAddedEventArgs(Control child) : EventArgs
{
    /// <summary>
    ///     The child that was added to this control.
    /// </summary>
    public Control Child { get; } = child;
}

/// <summary>
///     Event arguments for the <see cref="Control.ChildRemoved" /> event.
/// </summary>
/// <param name="child">The child that was removed.</param>
public class ChildRemovedEventArgs(Control child) : EventArgs
{
    /// <summary>
    ///     The child that was removed from this control.
    /// </summary>
    public Control Child { get; } = child;
}
