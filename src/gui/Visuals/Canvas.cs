// <copyright file="Canvas.cs" company="VoxelGame">
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
using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Input;

namespace VoxelGame.GUI.Visuals;

/// <summary>
///     The root visual for a user interface.
/// </summary>
/// <seealso cref="Controls.Canvas" />
public class Canvas : Visual
{
    private readonly Slot<InputRoot?> input;

    /// <summary>
    ///     Create a new instance of the <see cref="Canvas" /> class.
    /// </summary>
    public Canvas()
    {
        input = new Slot<InputRoot?>(value: null, this);
    }

    /// <summary>
    ///     The input handler for this canvas, responsible for processing input events and determining which visual should
    ///     receive them.
    /// </summary>
    public ReadOnlySlot<InputRoot?> Input => input;

    /// <summary>
    ///     Gets or sets the single child visual.
    /// </summary>
    public Visual? Child
    {
        get => Children.Count > 0 ? Children[0] : null;
        set => SetChild(value);
    }

    /// <inheritdoc />
    public override void OnAttach()
    {
        if (input.GetValue() == null)
            input.SetValue(new InputRoot(this));
    }

    /// <inheritdoc />
    public override void OnDetach(Boolean isReparenting)
    {
        if (isReparenting)
            return;

        input.GetValue()?.Dispose();
        input.SetValue(null);
    }

    /// <inheritdoc />
    public override void OnBoundsChanged(RectangleF oldBounds, RectangleF newBounds)
    {
        InvalidateMeasure();
    }

    /// <inheritdoc />
    public override void Render()
    {
        Renderer.Begin();

        Renderer.PushOffset(Point.Empty);
        Renderer.PushClip(Bounds);

        base.Render();

        Renderer.EndClip();

        Renderer.PopClip();
        Renderer.PopOffset();

        Renderer.End();
    }
}
