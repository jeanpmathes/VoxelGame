// <copyright file="ControlInputHelper.cs" company="VoxelGame">
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
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Input;
using VoxelGame.GUI.Visuals;
using Xunit;
using Canvas = VoxelGame.GUI.Controls.Canvas;

namespace VoxelGame.GUI.Tests.Input;

/// <summary>
///     Utilities for sending pointer input events that are guaranteed to hit a specific control.
/// </summary>
public static class ControlInputHelper
{
    private static PointF GetHitPoint(Control control)
    {
        Visual? visual = control.Visualization.GetValue();

        Assert.NotNull(visual);

        PointF localCenter = new(
            visual.Bounds.X + visual.Bounds.Width / 2,
            visual.Bounds.Y + visual.Bounds.Height / 2);

        return visual.LocalPointToRoot(localCenter);
    }

    extension(Canvas canvas)
    {
        /// <summary>
        ///     Simulates a full pointer click — button down followed by button up — at the center of the control.
        /// </summary>
        /// <param name="control">The control to click.</param>
        public void Click(Control control)
        {
            canvas.Render();

            PointF point = GetHitPoint(control);

            canvas.Input.GetValue()?.ReceivePointerButtonEvent(point, PointerButton.Left, isDown: true, ModifierKeys.None);
            canvas.Input.GetValue()?.ReceivePointerButtonEvent(point, PointerButton.Left, isDown: false, ModifierKeys.None);
        }

        /// <summary>
        ///     Simulates a pointer button-down event at the center of the control.
        /// </summary>
        /// <param name="control">The control to press.</param>
        public void Press(Control control)
        {
            canvas.Render();

            canvas.Input.GetValue()?.ReceivePointerButtonEvent(GetHitPoint(control), PointerButton.Left, isDown: true, ModifierKeys.None);
        }

        /// <summary>
        ///     Simulates a pointer button-down event at the specified point.
        /// </summary>
        /// <param name="point">The point to press.</param>
        public void Press(PointF point)
        {
            canvas.Render();

            canvas.Input.GetValue()?.ReceivePointerButtonEvent(point, PointerButton.Left, isDown: true, ModifierKeys.None);
        }

        /// <summary>
        ///     Simulates a pointer button-up event at the center of the control.
        /// </summary>
        /// <param name="control">The control to release over.</param>
        public void Release(Control control)
        {
            canvas.Render();

            canvas.Input.GetValue()?.ReceivePointerButtonEvent(GetHitPoint(control), PointerButton.Left, isDown: false, ModifierKeys.None);
        }

        /// <summary>
        ///     Simulates a pointer button-up event at the specified point.
        /// </summary>
        /// <param name="point">The point to release.</param>
        public void Release(PointF point)
        {
            canvas.Render();

            canvas.Input.GetValue()?.ReceivePointerButtonEvent(point, PointerButton.Left, isDown: false, ModifierKeys.None);
        }

        /// <summary>
        ///     Simulates a pointer move event to the center of the control.
        /// </summary>
        /// <param name="control">The control to move the pointer to.</param>
        public void MovePointerTo(Control control)
        {
            canvas.Render();

            canvas.Input.GetValue()?.ReceivePointerMoveEvent(GetHitPoint(control), deltaX: 0, deltaY: 0);
        }

        /// <summary>
        ///     Simulates a pointer move event to the specified point.
        /// </summary>
        /// <param name="point">The point to move the pointer to.</param>
        public void MovePointerTo(PointF point)
        {
            canvas.Render();

            canvas.Input.GetValue()?.ReceivePointerMoveEvent(point, deltaX: 0, deltaY: 0);
        }

        /// <summary>
        ///     Simulates a scroll event at the specified point.
        /// </summary>
        /// <param name="point">The point to scroll.</param>
        /// <param name="deltaX">The amount to scroll horizontally.</param>
        /// <param name="deltaY">The amount to scroll vertically.</param>
        public void Scroll(PointF point, Single deltaX, Single deltaY)
        {
            canvas.Render();

            canvas.Input.GetValue()?.ReceiveScrollEvent(point, deltaX, deltaY);
        }

        /// <summary>
        ///     Simulates a key-down event.
        /// </summary>
        /// <param name="key">The key to send.</param>
        /// <param name="modifiers">The modifiers to send.</param>
        public void PressKey(Key key, ModifierKeys modifiers = ModifierKeys.None)
        {
            canvas.Render();

            canvas.Input.GetValue()?.ReceiveKeyEvent(key, isDown: true, isRepeat: false, modifiers);
        }

        /// <summary>
        ///     Simulates a key-up event.
        /// </summary>
        /// <param name="key">The key to send.</param>
        public void ReleaseKey(Key key)
        {
            canvas.Render();

            canvas.Input.GetValue()?.ReceiveKeyEvent(key, isDown: false, isRepeat: false, ModifierKeys.None);
        }

        /// <summary>
        ///     Simulates a text input event.
        /// </summary>
        /// <param name="text">The text to send.</param>
        public void EnterText(String text)
        {
            canvas.Render();

            canvas.Input.GetValue()?.ReceiveTextEvent(text);
        }

        /// <summary>
        ///     Sets the keyboard focus to the specified control.
        /// </summary>
        /// <param name="control">The control to receive keyboard focus.</param>
        public void Focus(Control control)
        {
            canvas.Render();

            canvas.Input.GetValue()?.KeyboardFocus.Set(control);
        }

        /// <summary>
        ///     Sets the keyboard focus to the specified visual.
        /// </summary>
        /// <param name="visual">The visual to receive keyboard focus.</param>
        public void Focus(Visual visual)
        {
            canvas.Render();

            canvas.Input.GetValue()?.KeyboardFocus.Set(visual);
        }

        /// <summary>
        ///     Retrieves the currently keyboard-focused visual element within the canvas.
        /// </summary>
        /// <returns>The currently focused <see cref="Visual" /> if one exists; otherwise, null.</returns>
        public Visual? GetKeyboardFocused()
        {
            canvas.Render();

            return canvas.Input.GetValue()?.KeyboardFocus.GetFocused();
        }

        /// <summary>
        ///     Retrieves the currently pointer-focused visual element within the canvas.
        /// </summary>
        /// <returns>The currently focused <see cref="Visual" /> if one exists; otherwise, null.</returns>
        public Visual? GetPointerFocused()
        {
            canvas.Render();

            return canvas.Input.GetValue()?.PointerFocus.GetFocused();
        }
    }
}
