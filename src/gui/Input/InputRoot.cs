// <copyright file="InputRoot.cs" company="VoxelGame">
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
using System.Drawing;
using VoxelGame.GUI.Visuals;

namespace VoxelGame.GUI.Input;

/// <summary>
///     Processes input and different types of focus for a visual tree.
/// </summary>
public sealed class InputRoot : IInputReceiver, IDisposable
{
    // todo: work on the case that controls receive hover when created directly under pointer

    private readonly Visual root;

    private Visual? hoveredVisual;
    private Route hoverRoute = Route.Empty;

    private PointF lastPointerPosition;

    /// <summary>
    ///     Creates a new <seealso cref="InputRoot" /> with the specified root visual.
    /// </summary>
    /// <param name="root">
    ///     The root visual of the visual tree. Input events will be hit-tested against this visual and its
    ///     descendants.
    /// </param>
    public InputRoot(Visual root)
    {
        this.root = root;

        KeyboardFocus = new Focus(OnKeyboardFocusChanged);
        PointerFocus = new Focus(OnPointerFocusChanged);
    }

    /// <summary>
    ///     The keyboard focus.
    /// </summary>
    public Focus KeyboardFocus { get; }

    /// <summary>
    ///     The pointer (mouse) focus.
    /// </summary>
    public Focus PointerFocus { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        KeyboardFocus.Clear();
        PointerFocus.Clear();

        hoverRoute.Dispose();
    }

    private static void OnKeyboardFocusChanged(Visual? previousFocusedVisual, Visual? newFocusedVisual)
    {
        previousFocusedVisual?.HandleKeyboardFocusLost();
        newFocusedVisual?.HandleKeyboardFocusGained();
    }

    private void OnPointerFocusChanged(Visual? previousFocusedVisual, Visual? newFocusedVisual)
    {
        previousFocusedVisual?.HandlePointerFocusLost();
        newFocusedVisual?.HandlePointerFocusGained();

        UpdateHoveredVisual(PointerFocus.GetFocused() ?? PerformHitTest(lastPointerPosition));
    }

    private Visual? PerformHitTest(PointF point)
    {
        Visual current = root;

        if (!CanReceiveInput(current))
            return null;

        if (!current.Bounds.Contains(point))
            return null;

        while (true)
        {
            Boolean foundChild = false;

            for (Int32 index = current.Children.Count - 1; index >= 0; index--)
            {
                Visual child = current.Children[index];

                if (!CanReceiveInput(child))
                    continue;

                // As the bounds are in the parent coordinate system, we use the parent (current) for the transformation.
                if (!child.Bounds.Contains(current.RootPointToLocal(point)))
                    continue;

                current = child;
                foundChild = true;

                break;
            }

            if (!foundChild)
                return current;
        }
    }

    private static Boolean CanReceiveInput(Visual visual)
    {
        return visual.Enablement.GetValue().CanReceiveInput && visual.Visibility.GetValue().IsVisible;
    }

    private Visual? GetKeyboardTarget()
    {
        return KeyboardFocus.GetFocused();
    }

    private Visual? GetPointerTarget(PointF point)
    {
        return PointerFocus.GetFocused() ?? PerformHitTest(point);
    }

    private static Boolean HandleEvent(InputEvent inputEvent)
    {
        using Route route = Route.Create(inputEvent.Target);

        for (Int32 index = 0; index < route.Count; index++)
        {
            Visual visual = route.GetFromTop(index);

            inputEvent.SetTarget(visual);
            visual.HandleInputPreview(inputEvent);

            if (inputEvent.Handled) return true;
        }

        for (Int32 index = 0; index < route.Count; index++)
        {
            Visual visual = route.GetFromBottom(index);

            inputEvent.SetTarget(visual);
            visual.HandleInput(inputEvent);

            if (inputEvent.Handled) return true;
        }

        return false;
    }

    private void UpdateHoveredVisual(Visual? visual)
    {
        if (visual == hoveredVisual)
            return;

        Route newHoverRoute = Route.Create(visual);
        Int32 firstDifferentIndex = Route.FindFirstDifferenceFromTop(hoverRoute, newHoverRoute);

        for (Int32 index = firstDifferentIndex; index < hoverRoute.Count; index++)
            hoverRoute.GetFromTop(index).HandlePointerLeave();

        for (Int32 index = firstDifferentIndex; index < newHoverRoute.Count; index++)
            newHoverRoute.GetFromTop(index).HandlePointerEnter();

        hoverRoute.Dispose();
        hoverRoute = newHoverRoute;

        hoveredVisual = visual;
    }

    private Boolean MoveKeyboardFocus(Boolean forward)
    {
        Visual? previous = GetKeyboardTarget();
        Visual? current = previous;
        Visual start = current ?? root;

        if (current == null && CanMoveFocusTo(start))
        {
            KeyboardFocus.Set(start);
            return KeyboardFocus.GetFocused() != previous;
        }

        if (forward)
            MoveKeyboardFocusForward(start);
        else
            MoveKeyboardFocusBackward(start);

        return KeyboardFocus.GetFocused() != previous;
    }

    private void MoveKeyboardFocusForward(Visual start)
    {
        Visual? current = start;

        do
        {
            Visual? next = null;

            if (current.Children.Count > 0)
            {
                next = current.Children[0];
            }
            else if (current.Parent != null)
            {
                next = current.Parent.GetChildAfter(current);

                Visual? climb = current;

                while (next == null && climb.Parent != null)
                {
                    climb = climb.Parent;
                    next = climb.Parent?.GetChildAfter(climb);
                }

                if (next == null && climb.Parent == null)
                    next = root;
            }

            if (next != null && CanMoveFocusTo(next))
            {
                KeyboardFocus.Set(next);
                return;
            }

            current = next;
        } while (current != null && current != start);
    }

    private void MoveKeyboardFocusBackward(Visual start)
    {
        Visual? current = start;

        do
        {
            Visual? next;

            if (current.Parent != null)
            {
                next = current.Parent.GetChildBefore(current);

                while (next is {Children.Count: > 0})
                    next = next.Children[^1];

                next ??= current.Parent;
            }
            else
            {
                next = root;

                while (next.Children.Count > 0)
                    next = next.Children[^1];
            }

            if (next != null && CanMoveFocusTo(next))
            {
                KeyboardFocus.Set(next);
                return;
            }

            current = next;
        } while (current != null && current != start);
    }

    private static Boolean CanMoveFocusTo(Visual visual)
    {
        return Focus.CanFocus(visual) && visual.IsNavigable.GetValue();
    }

    #region EVENTS

    /// <inheritdoc />
    public Boolean ReceiveKeyEvent(Key key, Boolean isDown, Boolean isRepeat, ModifierKeys modifiers, Boolean isSynthetic = false)
    {
        Visual? target = GetKeyboardTarget();

        if (target != null)
        {
            KeyEvent @event = new(target, key, isDown, isRepeat, modifiers, isSynthetic);

            if (HandleEvent(@event)) return true;
        }

        if (isDown && key == Key.Tab)
            return MoveKeyboardFocus(!modifiers.HasFlag(ModifierKeys.Shift));

        return false;
    }

    /// <inheritdoc />
    public Boolean ReceiveTextEvent(String text)
    {
        Visual? target = GetKeyboardTarget();

        return target != null && HandleEvent(new TextEvent(target, text));
    }

    /// <inheritdoc />
    public Boolean ReceivePointerButtonEvent(PointF position, PointerButton button, Boolean isDown, ModifierKeys modifiers, Boolean isSynthetic = false)
    {
        Visual? target = GetPointerTarget(position);

        return target != null && HandleEvent(new PointerButtonEvent(target, position, button, isDown, modifiers, isSynthetic));
    }

    /// <inheritdoc />
    public Boolean ReceivePointerMoveEvent(PointF position, Single deltaX, Single deltaY)
    {
        lastPointerPosition = position;

        Visual? target = GetPointerTarget(position);

        Boolean handled = target != null && HandleEvent(new PointerMoveEvent(target, position, deltaX, deltaY));

        UpdateHoveredVisual(PointerFocus.GetFocused() ?? target);

        return handled;
    }

    /// <inheritdoc />
    public Boolean ReceiveScrollEvent(PointF position, Single deltaX, Single deltaY)
    {
        Visual? target = GetPointerTarget(position);

        return target != null && HandleEvent(new ScrollEvent(target, position, deltaX, deltaY));
    }

    /// <summary>
    ///     Call when pointer interaction with the visual tree is interrupted by the client losing focus.
    ///     The captured pointer target is released while keyboard focus is preserved.
    /// </summary>
    public void HandlePointerFocusLost()
    {
        PointerFocus.Clear();
    }

    #endregion EVENTS
}
