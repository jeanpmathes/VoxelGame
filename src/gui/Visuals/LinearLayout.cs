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
using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Utilities;

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
    public override Size OnMeasure(Size availableSize)
    {
        Single desiredWidth = 0;
        Single desiredHeight = 0;

        Size usableSize = availableSize - Padding.GetValue();

        if (IsHorizontal)
        {
            foreach (Visual child in Children)
            {
                Size childDesiredSize = child.Measure(usableSize with {Width = Single.PositiveInfinity});

                desiredWidth += childDesiredSize.Width;
                desiredHeight = Math.Max(desiredHeight, childDesiredSize.Height);
            }
        }
        else // Vertical.
        {
            foreach (Visual child in Children)
            {
                Size childDesiredSize = child.Measure(usableSize with {Height = Single.PositiveInfinity});

                desiredWidth = Math.Max(desiredWidth, childDesiredSize.Width);
                desiredHeight += childDesiredSize.Height;
            }
        }

        return new Size(desiredWidth, desiredHeight) + Padding.GetValue();
    }

    /// <inheritdoc />
    public override void OnArrange(Rectangle finalRectangle)
    {
        finalRectangle -= Padding.GetValue();

        if (IsHorizontal)
        {
            Single x = finalRectangle.X;

            foreach (Visual child in Children)
            {
                Size childDesiredSize = child.MeasuredSize;

                child.Arrange(new Rectangle(new Point(x, finalRectangle.Y), childDesiredSize with {Height = finalRectangle.Height}));

                x += childDesiredSize.Width;
            }
        }
        else // Vertical.
        {
            Single y = finalRectangle.Y;

            foreach (Visual child in Children)
            {
                Size childDesiredSize = child.MeasuredSize;

                child.Arrange(new Rectangle(new Point(finalRectangle.X, y), childDesiredSize with {Width = finalRectangle.Width}));

                y += childDesiredSize.Height;
            }
        }
    }
}
