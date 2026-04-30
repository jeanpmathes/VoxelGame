// <copyright file="Visual.cs" company="VoxelGame">
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
using System.Drawing;
using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Input;
using VoxelGame.GUI.Rendering;
using VoxelGame.GUI.Themes;
using VoxelGame.GUI.Utilities;
using Brush = VoxelGame.GUI.Graphics.Brush;
using Brushes = VoxelGame.GUI.Graphics.Brushes;

namespace VoxelGame.GUI.Visuals;

// todo: use less null suppression exclamation marks here, maybe add exceptions instead or other checks

/// <summary>
///     The abstract base class for all visuals in the visual tree.
///     A visual can be measured, arranged, and rendered.
/// </summary>
public abstract class Visual
{
    /// <summary>
    ///     Creates a new instance of the <see cref="Visual" /> class.
    /// </summary>
    protected Visual()
    {
        isAnchor = new Slot<Boolean>(value: false, this);
        templateOwner = new Slot<Control?>(value: null, this);

        isHovered = new Slot<Boolean>(value: false, this);
        isKeyboardFocused = new Slot<Boolean>(value: false, this);
        isPointerFocused = new Slot<Boolean>(value: false, this);

        InvalidateMeasure();

        MinimumWidth = VisualProperty.Create(this, BindToOwnerIfAnchor(o => o.MinimumWidth, defaultValue: 1f), Invalidation.Measure);
        MinimumHeight = VisualProperty.Create(this, BindToOwnerIfAnchor(o => o.MinimumHeight, defaultValue: 1f), Invalidation.Measure);

        MaximumWidth = VisualProperty.Create(this, BindToOwnerIfAnchor(o => o.MaximumWidth, Single.PositiveInfinity), Invalidation.Measure);
        MaximumHeight = VisualProperty.Create(this, BindToOwnerIfAnchor(o => o.MaximumHeight, Single.PositiveInfinity), Invalidation.Measure);

        Margin = VisualProperty.Create(this, BindToOwnerIfAnchor(o => o.Margin), Invalidation.Measure);
        Padding = VisualProperty.Create(this, BindToOwnerIfAnchor(o => o.Padding), Invalidation.Measure);

        HorizontalAlignment = VisualProperty.Create(this, BindToOwnerIfAnchor(o => o.HorizontalAlignment), Invalidation.Arrange);
        VerticalAlignment = VisualProperty.Create(this, BindToOwnerIfAnchor(o => o.VerticalAlignment), Invalidation.Arrange);

        Background = VisualProperty.Create(this, BindToOwnerBackground(), Invalidation.Render);

        Opacity = VisualProperty.Create(this, BindToOwnerIfAnchor(o => o.Opacity, defaultValue: 1f), Invalidation.Render);

        IsNavigable = VisualProperty.Create(this, BindToOwnerIfAnchor(o => o.IsNavigable, defaultValue: false), Invalidation.None);

        Visibility = VisualProperty.Create(this, BindToOwner(o => o.Visibility), Invalidation.Measure);
        Enablement = VisualProperty.Create(this, BindToOwner(o => o.Enablement), Invalidation.None);
    }

    #region PROPERTIES

    /// <summary>
    ///     The minimum width of this visual. Might not be respected by all layout containers.
    /// </summary>
    public VisualProperty<Single> MinimumWidth { get; }

    /// <summary>
    ///     The minimum height of this visual. Might not be respected by all layout containers.
    /// </summary>
    public VisualProperty<Single> MinimumHeight { get; }

    /// <summary>
    ///     The maximum width of this visual. Might not be respected by all layout containers.
    /// </summary>
    public VisualProperty<Single> MaximumWidth { get; }

    /// <summary>
    ///     The maximum height of this visual. Might not be respected by all layout containers.
    /// </summary>
    public VisualProperty<Single> MaximumHeight { get; }

    /// <summary>
    ///     The margin of this visual, which is space around the visual that the layout system should try to respect.
    /// </summary>
    public VisualProperty<ThicknessF> Margin { get; }

    /// <summary>
    ///     The padding of this visual, which is space inside the visual that the layout system should try to respect.
    ///     If a visual defines custom layout logic, it decides if and how to respect the padding.
    ///     As such, padding is less strictly enforced than margin.
    /// </summary>
    public VisualProperty<ThicknessF> Padding { get; }

    /// <summary>
    ///     The horizontal alignment of this visual, which is used by layout containers to determine how to position this
    ///     visual within its layout slot.
    /// </summary>
    public VisualProperty<HorizontalAlignment> HorizontalAlignment { get; }

    /// <summary>
    ///     The vertical alignment of this visual, which is used by layout containers to determine how to position this visual
    ///     within its layout slot.
    /// </summary>
    public VisualProperty<VerticalAlignment> VerticalAlignment { get; }

    /// <summary>
    ///     The brush used to draw the background of this visual.
    /// </summary>
    public VisualProperty<Brush> Background { get; }

    /// <summary>
    ///     The opacity to render this element with, in the range from 0 to 1 inclusive.
    ///     The renderer multiplies this with all parent opacities and also the brush opacity.
    /// </summary>
    public VisualProperty<Single> Opacity { get; }

    /// <summary>
    ///     Whether this visual allows to navigate to it, which is used to move the keyboard focus using the keyboard.
    ///     A non-navigable visual might still receive keyboard focus through other means, but will be skipped during keyboard
    ///     navigation.
    ///     Children of non-navigable visuals can still be navigable and will not be skipped.
    /// </summary>
    public VisualProperty<Boolean> IsNavigable { get; }

    /// <summary>
    ///     The visibility of this visual, which determines whether it is visible and whether it takes up space in the layout.
    /// </summary>
    public VisualProperty<Visibility> Visibility { get; }

    /// <summary>
    ///     The enablement of this visual, which determines whether it can receive input and focus and how it should react to
    ///     input.
    /// </summary>
    public VisualProperty<Enablement> Enablement { get; }

    /// <summary>
    ///     Create a binding that binds to the foreground of the template owner.
    /// </summary>
    /// <returns>The created binding.</returns>
    protected Binding<Brush> BindToOwnerForeground()
    {
        return Binding.To(TemplateOwner).Select(o => o?.Foreground, Defaults.ForegroundBrush);
    }

    /// <summary>
    ///     Create a binding that binds to the background of the template owner.
    /// </summary>
    /// <returns>The created binding.</returns>
    protected Binding<Brush> BindToOwnerBackground()
    {
        return Binding.To(TemplateOwner).Select(o => o?.Background, Brushes.Transparent);
    }

    /// <summary>
    ///     Bind to a property of the template owner, returning a default value if this visual is not part of a control
    ///     template.
    /// </summary>
    /// <param name="selector">The selector function to select the property from the template owner.</param>
    /// <param name="defaultValue">The default value to use if this visual is not part of a control template.</param>
    /// <typeparam name="TValue">The type of the property to bind to.</typeparam>
    /// <returns>The created binding.</returns>
    protected Binding<TValue> BindToOwner<TValue>(Func<Control, IValueSource<TValue>> selector, TValue defaultValue = default!)
    {
        return Binding.To(TemplateOwner)
            .Select(owner => owner != null ? selector(owner) : null, defaultValue);
    }

    /// <summary>
    ///     Bind to a property of the template owner if this visual is an anchor, otherwise bind to a default value.
    /// </summary>
    /// <param name="selector">The selector function to select the property from the template owner.</param>
    /// <param name="defaultValue">The default value to use if this visual is not an anchor.</param>
    /// <typeparam name="TValue">The type of the property to bind to.</typeparam>
    /// <returns>The created binding.</returns>
    protected Binding<TValue> BindToOwnerIfAnchor<TValue>(Func<Control, IValueSource<TValue>> selector, TValue defaultValue = default!)
    {
        return Binding.To(TemplateOwner, IsAnchor)
            .Select((owner, anchor) => anchor && owner != null ? selector(owner) : null, defaultValue);
    }

    #endregion PROPERTIES

    #region HIERARCHY

    /// <summary>
    ///     Whether this visual is attached to the visual root.
    /// </summary>
    protected Boolean IsAttached { get; private set; }

    /// <summary>
    ///     Whether this visual is the root visual of the tree.
    /// </summary>
    private protected Boolean IsRoot { get; set; }

    /// <summary>
    ///     Set this visual as the root of a tree.
    ///     May only be called by the code within <see cref="Control" />.
    /// </summary>
    /// <param name="uiRenderer">The renderer to use for this visual and its subtree.</param>
    internal void SetAsRoot(IRenderer uiRenderer)
    {
        renderer = uiRenderer;

        IsRoot = true;

        Attach(TemplateOwner.GetValue()!);
    }

    /// <summary>
    ///     Unset this visual as the root of a tree, if it is a root.
    ///     May only be called by the code within <see cref="Control" />.
    /// </summary>
    internal void UnsetAsRoot()
    {
        renderer = null;

        IsRoot = false;

        Detach(isReparenting: false);
    }

    /// <summary>
    ///     Set this visual as the anchor of a control template.
    /// </summary>
    /// <param name="control">The template owner control.</param>
    internal void SetAsAnchor(Control control)
    {
        isAnchor.SetValue(true);
        templateOwner.SetValue(control);
    }

    /// <summary>
    ///     Unset this visual as the anchor of a control template, if it is an anchor.
    /// </summary>
    internal void UnsetAsAnchor()
    {
        isAnchor.SetValue(false);
    }

    private readonly Slot<Control?> templateOwner;

    /// <summary>
    ///     The control owning the template this visual is part of, or <c>null</c> if this visual is not part of a control
    ///     template.
    /// </summary>
    protected ReadOnlySlot<Control?> TemplateOwner => templateOwner;

    private readonly Slot<Boolean> isAnchor;

    /// <summary>
    ///     Whether this visual is the anchor of a control template, i.e. the first visual in the template.
    /// </summary>
    protected ReadOnlySlot<Boolean> IsAnchor => isAnchor;

    private readonly List<Visual> children = [];

    /// <summary>
    ///     The parent of this visual.
    /// </summary>
    public Visual? Parent { get; private set; }

    /// <summary>
    ///     The children of this visual.
    /// </summary>
    public IReadOnlyList<Visual> Children => children;

    /// <summary>
    ///     Set the child of this visual.
    ///     Replaces any existing children.
    /// </summary>
    /// <param name="child">
    ///     The child to set. Will be removed from its previous parent if any.
    ///     If <c>null</c>, all existing children will be removed.
    /// </param>
    protected void SetChild(Visual? child)
    {
        if (child?.Parent == this && children.Count == 1) return;

        child?.Parent?.RemoveChild(child, isReparenting: true);

        if (children.Count > 0)
        {
            List<Visual> oldChildren = new(children);
            children.Clear();

            foreach (Visual oldChild in oldChildren)
            {
                oldChild.Parent = null;

                HandleChildRemoved(oldChild);
                oldChild.Detach(isReparenting: false);
            }
        }

        if (child == null) return;

        children.Add(child);
        child.Parent = this;

        if (IsAttached)
            child.Attach(TemplateOwner.GetValue()!);

        HandleChildAdded(child);
    }

    /// <summary>
    ///     Add a child to this visual.
    /// </summary>
    /// <param name="child">The child to add. Will be removed from its previous parent if any.</param>
    protected void AddChild(Visual child)
    {
        if (child.Parent == this) return;

        child.Parent?.RemoveChild(child, isReparenting: true);

        children.Add(child);
        child.Parent = this;

        if (IsAttached)
            child.Attach(TemplateOwner.GetValue()!);

        HandleChildAdded(child);
    }

    /// <summary>
    ///     Remove a child from this visual.
    ///     If the specified child is not a child of this visual, nothing happens.
    /// </summary>
    /// <param name="child">The child to remove.</param>
    protected void RemoveChild(Visual child)
    {
        RemoveChild(child, isReparenting: false);
    }

    /// <summary>
    ///     Remove a child visual from this visual, optionally preserving resources for reparenting.
    /// </summary>
    /// <param name="child">The child visual to remove.</param>
    /// <param name="isReparenting">Whether the child is being removed as part of reparenting.</param>
    private void RemoveChild(Visual child, Boolean isReparenting)
    {
        if (child.Parent != this) return;

        if (!children.Remove(child)) return;

        child.Parent = null;

        HandleChildRemoved(child);
        child.Detach(isReparenting);
    }

    /// <summary>
    ///     Handle common operations that should happen whenever a child visual is added.
    /// </summary>
    /// <param name="child">The child visual that was added.</param>
    private void HandleChildAdded(Visual child)
    {
        child.UpdateDrawDebugOutlinesEffectiveValue();

        InvalidateMeasure();
        OnChildAdded(child);
    }

    /// <summary>
    ///     Handle common operations that should happen whenever a child visual is removed.
    /// </summary>
    /// <param name="child">The child visual that was removed.</param>
    private void HandleChildRemoved(Visual child)
    {
        child.UpdateDrawDebugOutlinesEffectiveValue();

        InvalidateMeasure();
        OnChildRemoved(child);
    }

    /// <summary>
    ///     Called after a child visual has been added to this visual.
    /// </summary>
    /// <param name="child">The child visual that was added.</param>
    private protected virtual void OnChildAdded(Visual child) {}

    /// <summary>
    ///     Called after a child visual has been removed from this visual.
    /// </summary>
    /// <param name="child">The child visual that was removed.</param>
    private protected virtual void OnChildRemoved(Visual child) {}

    /// <summary>
    ///     Attach this visual and its subtree to a template owner.
    /// </summary>
    /// <param name="newOwner">The control that owns the template for this visual subtree.</param>
    private void Attach(Control newOwner)
    {
        if (IsAttached) return;
        IsAttached = true;

        if (IsAnchor.GetValue())
            newOwner = TemplateOwner.GetValue()!;

        templateOwner.SetValue(newOwner);
        renderer ??= Parent?.renderer;

        OnAttach();
        AttachedToRoot?.Invoke(this, EventArgs.Empty);

        foreach (Visual child in children)
        {
            child.Attach(newOwner);
        }
    }

    /// <summary>
    ///     Called when the visual is attached to a tree with a root visual.
    ///     Note that for example giving this visual a parent does not necessarily
    ///     attach it to a root visual, as the parent itself may not be attached to a root.
    /// </summary>
    public virtual void OnAttach() {}

    /// <summary>
    ///     Invoked when this visual is attached to a tree with a root visual.
    /// </summary>
    public event EventHandler? AttachedToRoot;

    /// <summary>
    ///     Detach this visual and its subtree from the current template owner.
    /// </summary>
    /// <param name="isReparenting">Whether detaching happens as part of reparenting.</param>
    private void Detach(Boolean isReparenting)
    {
        if (!IsAttached) return;
        IsAttached = IsRoot;
        if (IsAttached) return;

        OnDetach(isReparenting);
        DetachedFromRoot?.Invoke(this, EventArgs.Empty);

        templateOwner.SetValue(null);
        renderer = null;

        foreach (Visual child in children)
        {
            child.Detach(isReparenting);
        }
    }

    /// <summary>
    ///     Called when the visual is detached from a tree with a root visual.
    ///     Note that being detached in most cases does not mean losing the parent,
    ///     as it may simply be that the parent or one of its ancestors was detached.
    /// </summary>
    /// <remarks>
    ///     Generally, disposable resources must be disposed when being detached,
    ///     unless the visual is being reparented.
    /// </remarks>
    /// <param name="isReparenting">
    ///     Indicates whether the visual is being detached because it is being reparented.
    /// </param>
    public virtual void OnDetach(Boolean isReparenting) {}

    /// <summary>
    ///     Invoked when this visual is detached from a tree with a root visual.
    /// </summary>
    public event EventHandler? DetachedFromRoot;

    /// <summary>
    ///     Get the child visual after the specified child visual, or <c>null</c> if no such child exists.
    /// </summary>
    /// <param name="child">
    ///     The child visual to get the next sibling of. If this is not a child of this visual, the result is
    ///     also <c>null</c>.
    /// </param>
    /// <returns>The child visual after the specified child visual, or <c>null</c> if no such child exists.</returns>
    public Visual? GetChildAfter(Visual child)
    {
        Int32 index = children.IndexOf(child);
        if (index == -1 || index == children.Count - 1) return null;
        return children[index + 1];
    }

    /// <summary>
    ///     Get the child visual before the specified child visual, or <c>null</c> if no such child exists.
    /// </summary>
    /// <param name="child">
    ///     The child visual to get the previous sibling of. If this is not a child of this visual, the result
    ///     is also <c>null</c>.
    /// </param>
    /// <returns>The child visual before the specified child visual, or <c>null</c> if no such child exists.</returns>
    public Visual? GetChildBefore(Visual child)
    {
        Int32 index = children.IndexOf(child);
        if (index <= 0) return null;
        return children[index - 1];
    }

    #endregion HIERARCHY

    #region LAYOUTING

    private SizeF MinimumSize => new(MinimumWidth.GetValue(), MinimumHeight.GetValue());
    private SizeF MaximumSize => new(MaximumWidth.GetValue(), MaximumHeight.GetValue());

    private Boolean isMeasureValid;
    private Boolean isArrangeValid;

    /// <summary>
    ///     The size this visual measured itself to be during the last measure pass, including its margins.
    ///     This represents the total space this visual requires for layout, i.e. the minimum area that
    ///     must be reserved by its parent so it can render itself properly.
    /// </summary>
    public SizeF MeasuredSize { get; private set; } = SizeF.Empty;

    private SizeF lastAvailableSize = SizeF.Empty;

    /// <summary>
    ///     The bounds of this visual, excluding margins, relative to the parent visual.
    /// </summary>
    public RectangleF Bounds { get; private set; }

    /// <summary>
    ///     The bounds with negative offset applied, zeroing them to (0,0).
    /// </summary>
    protected RectangleF RenderBounds => new(PointF.Empty, Bounds.Size);

    private RectangleF lastFinalRectangle = RectangleF.Empty;

    /// <summary>
    ///     Offset from this visual to the root visual.
    ///     Applying this to a root position transforms it into the child coordinate space of this visual, not the local
    ///     coordinate space.
    /// </summary>
    private PointF offsetToRoot = PointF.Empty;

    /// <summary>
    ///     Measure the desired size of this visual given the available size.
    /// </summary>
    /// <param name="availableSize">The available size. The visual does not have to use all of this size.</param>
    /// <returns>The desired size required by this visual, might be larger than the available size.</returns>
    public SizeF Measure(SizeF availableSize)
    {
        if (isMeasureValid && lastAvailableSize == availableSize)
            return MeasuredSize;

        lastAvailableSize = availableSize;

        if (Visibility.GetValue().IsLayouted)
        {
            SizeF usableSize = availableSize - Margin.GetValue();

            usableSize = Sizes.Max(SizeF.Empty, usableSize);

            SizeF measuredSize = OnMeasure(usableSize);

            measuredSize = Sizes.Clamp(measuredSize, MinimumSize, MaximumSize);

            MeasuredSize = measuredSize + Margin.GetValue();
        }
        else
        {
            MeasuredSize = SizeF.Empty;
        }

        isMeasureValid = true;
        isArrangeValid = false;

        return MeasuredSize;
    }

    /// <summary>
    ///     Override to define custom measuring logic. See <see cref="Measure(SizeF)" />.
    ///     If you override this, you will likely need to override <see cref="OnArrange(RectangleF)" /> as well and vice versa.
    /// </summary>
    /// <param name="availableSize">The available size. The visual does not have to use all of this size.</param>
    /// <returns>
    ///     The desired size required by this visual, might be larger than the available size.
    ///     Must never return an infinite size.
    /// </returns>
    public virtual SizeF OnMeasure(SizeF availableSize)
    {
        if (children.Count == 0)
            return SizeF.Empty;

        SizeF desiredSize = SizeF.Empty;

        SizeF usableSize = availableSize;

        usableSize -= Padding.GetValue();

        foreach (Visual child in children)
        {
            SizeF childDesiredSize = child.Measure(usableSize);

            desiredSize.Width = Math.Max(desiredSize.Width, childDesiredSize.Width);
            desiredSize.Height = Math.Max(desiredSize.Height, childDesiredSize.Height);
        }

        desiredSize += Padding.GetValue();

        return desiredSize;
    }

    /// <summary>
    ///     Arrange this visual within the given final rectangle.
    ///     This will set the <see cref="Bounds" /> of this visual.
    /// </summary>
    /// <param name="finalRectangle">
    ///     The final rectangle to arrange this visual in, in the coordinate space of the parent
    ///     visual.
    /// </param>
    public void Arrange(RectangleF finalRectangle)
    {
        if (isArrangeValid && lastFinalRectangle == finalRectangle)
            return;

        lastFinalRectangle = finalRectangle;

        if (!isMeasureValid)
            Measure(finalRectangle.Size);

        finalRectangle -= Margin.GetValue();

        if (!Visibility.GetValue().IsLayouted)
        {
            SetBounds(Rectangle.Empty);
        }
        else if (finalRectangle.IsEmpty)
        {
            SetBounds(finalRectangle);
        }
        else
        {
            SizeF size = finalRectangle.Size;

            if (HorizontalAlignment.GetValue() != GUI.HorizontalAlignment.Stretch)
                size.Width = Math.Min(finalRectangle.Width, MeasuredSize.Width - Margin.GetValue().Width);

            if (VerticalAlignment.GetValue() != GUI.VerticalAlignment.Stretch)
                size.Height = Math.Min(finalRectangle.Height, MeasuredSize.Height - Margin.GetValue().Height);

            size = Sizes.Clamp(size, MinimumSize, MaximumSize);

            PointF location = finalRectangle.Location;

            if (HorizontalAlignment.GetValue() == GUI.HorizontalAlignment.Center)
                location.X += (finalRectangle.Width - size.Width) / 2;
            else if (HorizontalAlignment.GetValue() == GUI.HorizontalAlignment.Right)
                location.X += finalRectangle.Width - size.Width;

            if (VerticalAlignment.GetValue() == GUI.VerticalAlignment.Center)
                location.Y += (finalRectangle.Height - size.Height) / 2;
            else if (VerticalAlignment.GetValue() == GUI.VerticalAlignment.Bottom)
                location.Y += finalRectangle.Height - size.Height;

            SetBounds(new RectangleF(location, size));

            OnArrange(new RectangleF(PointF.Empty, size));
        }

        // SetBounds might invalidate something, so we set everything to valid again.

        isArrangeValid = true;
        isMeasureValid = true;
    }

    /// <summary>
    ///     Override to define custom arranging logic. See <see cref="Arrange(RectangleF)" />.
    ///     If you have overriden <see cref="OnMeasure(SizeF)" />, you will likely need to override this as well and vice
    ///     versa.
    /// </summary>
    /// <param name="finalRectangle">The final rectangle to arrange this visual in, in the coordinate space of this visual.</param>
    public virtual void OnArrange(RectangleF finalRectangle)
    {
        if (children.Count == 0)
            return;

        finalRectangle -= Padding.GetValue();

        if (finalRectangle.IsEmpty)
            return;

        foreach (Visual child in children)
            child.Arrange(finalRectangle);
    }

    /// <summary>
    ///     Invalidate the measurement of this visual, causing a re-measure and re-arrange.
    /// </summary>
    public void InvalidateMeasure()
    {
        if (!isMeasureValid) return;

        isMeasureValid = false;
        isArrangeValid = false;

        Parent?.InvalidateMeasure();
    }

    /// <summary>
    ///     Invalidate the arrangement of this visual, causing a re-arrange.
    /// </summary>
    public void InvalidateArrange()
    {
        if (!isArrangeValid) return;

        isArrangeValid = false;

        Parent?.InvalidateArrange();
    }

    /// <summary>
    ///     Sets the size of the visual.
    /// </summary>
    /// <param name="size">New size.</param>
    /// <returns>True if bounds changed.</returns>
    /// <remarks>Bounds are reset after the next layout pass.</remarks>
    public Boolean SetSize(SizeF size)
    {
        return SetBounds(Bounds with {Size = size});
    }

    /// <summary>
    ///     Sets the visual bounds, relative to the parent visual.
    /// </summary>
    /// <param name="newBounds">New bounds.</param>
    /// <returns>True if bounds changed.</returns>
    /// <remarks>Bounds are reset after the next layout pass.</remarks>
    public virtual Boolean SetBounds(RectangleF newBounds)
    {
        if (Bounds == newBounds) return false;

        RectangleF oldBounds = Bounds;
        Bounds = newBounds;

        UpdateOffsetToRoot();

        OnBoundsChanged(oldBounds, newBounds);

        InvalidateRender();

        return true;
    }

    /// <summary>
    ///     Called when the bounds of this visual have changed.
    /// </summary>
    /// <param name="oldBounds">The old bounds.</param>
    /// <param name="newBounds">The new bounds.</param>
    public virtual void OnBoundsChanged(RectangleF oldBounds, RectangleF newBounds) {}

    private void UpdateOffsetToRoot()
    {
        PointF newOffset = Parent != null
            ? new PointF(Parent.offsetToRoot.X + Bounds.X, Parent.offsetToRoot.Y + Bounds.Y)
            : Bounds.Location;

        if (offsetToRoot == newOffset) return;

        offsetToRoot = newOffset;

        foreach (Visual child in children)
            child.UpdateOffsetToRoot();
    }

    /// <summary>
    ///     Transform a point from the local coordinate space of this visual to the coordinate space of the root visual.
    /// </summary>
    /// <param name="localPoint">The point in the local coordinate space of this visual.</param>
    /// <returns>The point transformed to the coordinate space of the root visual.</returns>
    public PointF LocalPointToRoot(PointF localPoint)
    {
        return new PointF(localPoint.X + offsetToRoot.X - Bounds.X, localPoint.Y + offsetToRoot.Y - Bounds.Y);
    }

    /// <summary>
    ///     Transform a point from the coordinate space of the root visual to the local coordinate space of this visual.
    /// </summary>
    /// <param name="rootPoint">The point in the coordinate space of the root visual.</param>
    /// <returns>The point transformed to the local coordinate space of this visual.</returns>
    public PointF RootPointToLocal(PointF rootPoint)
    {
        return new PointF(rootPoint.X - offsetToRoot.X + Bounds.X, rootPoint.Y - offsetToRoot.Y + Bounds.Y);
    }

    #endregion LAYOUTING

    #region RENDERING

    private IRenderer? renderer;

    private Boolean isRenderValid;

    /// <summary>
    ///     Determines whether the visual should be clipped to its bounds while rendering.
    /// </summary>
    protected virtual Boolean ShouldClip => true;

    /// <summary>
    ///     Render this visual using the specified renderer.
    ///     For custom rendering, generally override <see cref="OnRender()" /> instead of this method, as this method handles
    ///     important setup and teardown logic.
    /// </summary>
    public virtual void Render()
    {
        if (renderer == null) return;

        PrepareRender();

        if (!Visibility.GetValue().IsVisible)
            return;

        // todo: unify all push/pop operations into a single Push(offset, clip?, opacity) with nullable clip

        renderer.PushOffset(Bounds.Location);
        renderer.PushOpacity(Opacity.GetValue());

        DoRender();

        if (ShouldClip)
            renderer.PopClip();

        renderer.PopOpacity();
        renderer.PopOffset();

        isRenderValid = true;

        if (drawDebugOutlinesEffective)
        {
            ThicknessF margin = Margin.GetValue();
            ThicknessF padding = Padding.GetValue();

            if (margin != ThicknessF.Zero)
                renderer.DrawLinedRectangle(Bounds + margin, Brushes.DebugMargin);

            renderer.DrawLinedRectangle(Bounds, Brushes.DebugBounds);

            if (padding != ThicknessF.Zero)
                renderer.DrawLinedRectangle(Bounds - padding, Brushes.DebugPadding);
        }

        void DoRender()
        {
            if (ShouldClip)
            {
                renderer.PushClip(RenderBounds);

                if (renderer.IsClipEmpty()) return;

                renderer.BeginClip();
            }

            OnRender();

            foreach (Visual child in children)
            {
                child.Render();
            }
        }
    }

    /// <summary>
    ///     Ensure measure and arrange are valid before rendering this visual.
    /// </summary>
    private void PrepareRender()
    {
        // Generally, measure and arrange bubble up to the root, which means the measure and arrange
        // is performed at most once for the entire visual tree.
        // However, layout boundaries would prevent bubbling up beyond them, so they must also ensure
        // that measure and arrange are valid before rendering.

        if (!isMeasureValid)
            Measure(Bounds.Size);

        if (!isArrangeValid)
            Arrange(Bounds);
    }

    /// <summary>
    ///     Get the renderer to render this visual with.
    ///     Is guaranteed to be non-null in <see cref="OnRender" /> and when attached.
    /// </summary>
    protected IRenderer Renderer => renderer!;

    /// <summary>
    ///     Called when this visual should render itself.
    ///     Override this to implement custom rendering logic.
    /// </summary>
    protected virtual void OnRender()
    {
        Renderer.DrawFilledRectangle(RenderBounds, Background.GetValue());
    }

    /// <summary>
    ///     Invalidate the rendering of this visual, causing a re-render.
    /// </summary>
    public void InvalidateRender()
    {
        if (!isRenderValid) return;

        Parent?.InvalidateRender();
    }

    #endregion RENDERING

    #region INPUT

    private readonly Slot<Boolean> isHovered;

    /// <summary>
    ///     Whether the pointer (mouse) is currently hovering over this visual.
    /// </summary>
    public ReadOnlySlot<Boolean> IsHovered => isHovered;

    private readonly Slot<Boolean> isKeyboardFocused;

    /// <summary>
    ///     Whether this visual currently has keyboard focus.
    /// </summary>
    public ReadOnlySlot<Boolean> IsKeyboardFocused => isKeyboardFocused;

    private readonly Slot<Boolean> isPointerFocused;

    /// <summary>
    ///     Whether this visual currently has pointer (mouse) focus.
    /// </summary>
    public ReadOnlySlot<Boolean> IsPointerFocused => isPointerFocused;

    /// <summary>
    ///     Handle an input event that is tunneling.
    /// </summary>
    /// <param name="inputEvent">The input event.</param>
    internal void HandleInputPreview(InputEvent inputEvent)
    {
        OnInputPreview(inputEvent);

        if (isAnchor.GetValue())
            templateOwner.GetValue()?.OnInputPreview(inputEvent);
    }

    /// <summary>
    ///     Called when an input event tunnels down the visual tree towards the target visual.
    ///     Use this to intercept input events before they reach the target visual.
    /// </summary>
    /// <param name="inputEvent">The input event.</param>
    public virtual void OnInputPreview(InputEvent inputEvent) {}

    /// <summary>
    ///     Handle an input event that is bubbling.
    /// </summary>
    /// <param name="inputEvent">The input event.</param>
    internal void HandleInput(InputEvent inputEvent)
    {
        OnInput(inputEvent);

        if (isAnchor.GetValue())
            templateOwner.GetValue()?.OnInput(inputEvent);
    }

    /// <summary>
    ///     Called when an input event bubbles up the visual tree from the target visual.
    ///     Use this to handle inputs.
    /// </summary>
    /// <param name="inputEvent">The input event.</param>
    public virtual void OnInput(InputEvent inputEvent) {}

    internal void HandlePointerEnter()
    {
        isHovered.SetValue(true);

        OnPointerEnter();
    }

    /// <summary>
    ///     Called when the pointer enters the bounds of this visual.
    /// </summary>
    public virtual void OnPointerEnter() {}

    internal void HandlePointerLeave()
    {
        isHovered.SetValue(false);

        OnPointerLeave();
    }

    /// <summary>
    ///     Called when the pointer leaves the bounds of this visual.
    /// </summary>
    public virtual void OnPointerLeave() {}

    internal void HandleKeyboardFocusGained()
    {
        isKeyboardFocused.SetValue(true);
    }

    internal void HandleKeyboardFocusLost()
    {
        isKeyboardFocused.SetValue(false);
    }

    internal void HandlePointerFocusGained()
    {
        isPointerFocused.SetValue(true);
    }

    internal void HandlePointerFocusLost()
    {
        isPointerFocused.SetValue(false);
    }

    #endregion INPUT

    #region DEBUG

    private Boolean drawDebugOutlinesLocal;
    private Boolean drawDebugOutlinesEffective;

    /// <summary>
    ///     Whether this visual should render debug outlines.
    ///     The value is inherited downwards the visual tree, i.e. if this is set to true, all descendants of this visual will
    ///     also render debug outlines.
    /// </summary>
    internal Boolean DrawDebugOutlines
    {
        get => drawDebugOutlinesEffective;
        set
        {
            if (drawDebugOutlinesLocal == value) return;

            drawDebugOutlinesLocal = value;
            UpdateDrawDebugOutlinesEffectiveValue();
        }
    }

    /// <summary>
    ///     Update the effective debug-outline flag and propagate to children if needed.
    /// </summary>
    private void UpdateDrawDebugOutlinesEffectiveValue()
    {
        Boolean newValue = drawDebugOutlinesLocal || Parent?.drawDebugOutlinesEffective == true;
        if (drawDebugOutlinesEffective == newValue) return;

        drawDebugOutlinesEffective = newValue;

        InvalidateRender();

        foreach (Visual child in children)
        {
            child.UpdateDrawDebugOutlinesEffectiveValue();
        }
    }

    #endregion DEBUG
}
