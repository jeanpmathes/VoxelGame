// <copyright file="LinearLayout.cs" company="VoxelGame">
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

namespace VoxelGame.GUI.Visuals;

/// <summary>
///     A linear layout arranges its children in a single line, either horizontally or vertically.
/// </summary>
/// <seealso cref="Controls.LinearLayout" />
public class LinearLayout : Layout
{
    /// <summary>
    ///     Creates a new instance of the <see cref="LinearLayout" /> class.
    /// </summary>
    public LinearLayout()
    {
        Orientation = VisualProperty.Create(this, GUI.Orientation.Horizontal, Invalidation.Measure);
    }

    #region PROPERTIES

    /// <summary>
    ///     The orientation of the layout, which determines whether the children are arranged horizontally or vertically.
    /// </summary>
    public VisualProperty<Orientation> Orientation { get; }

    #endregion PROPERTIES

    private Boolean IsHorizontal => Orientation.GetValue() == GUI.Orientation.Horizontal;

    /// <inheritdoc />
    public override SizeF OnMeasure(SizeF availableSize)
    {
        SizeF desiredSize = SizeF.Empty;

        SizeF usableSize = availableSize - Padding.GetValue();

        if (IsHorizontal)
        {
            usableSize.Width = Single.PositiveInfinity;

            foreach (Visual child in Children)
            {
                SizeF childDesiredSize = child.Measure(usableSize);

                desiredSize.Width += childDesiredSize.Width;
                desiredSize.Height = Math.Max(desiredSize.Height, childDesiredSize.Height);
            }
        }
        else // Vertical.
        {
            usableSize.Height = Single.PositiveInfinity;

            foreach (Visual child in Children)
            {
                SizeF childDesiredSize = child.Measure(usableSize);

                desiredSize.Width = Math.Max(desiredSize.Width, childDesiredSize.Width);
                desiredSize.Height += childDesiredSize.Height;
            }
        }

        return desiredSize + Padding.GetValue();
    }

    /// <inheritdoc />
    public override void OnArrange(RectangleF finalRectangle)
    {
        finalRectangle -= Padding.GetValue();

        if (IsHorizontal)
        {
            Single x = finalRectangle.X;

            foreach (Visual child in Children)
            {
                SizeF childDesiredSize = child.MeasuredSize;

                child.Arrange(new RectangleF(new PointF(x, finalRectangle.Y), childDesiredSize with {Height = finalRectangle.Height}));

                x += childDesiredSize.Width;
            }
        }
        else // Vertical.
        {
            Single y = finalRectangle.Y;

            foreach (Visual child in Children)
            {
                SizeF childDesiredSize = child.MeasuredSize;

                child.Arrange(new RectangleF(new PointF(finalRectangle.X, y), childDesiredSize with {Width = finalRectangle.Width}));

                y += childDesiredSize.Height;
            }
        }
    }
}
