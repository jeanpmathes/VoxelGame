// <copyright file="Border.cs" company="VoxelGame">
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

using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Drawing;
using VoxelGame.GUI.Utilities;
using Brush = VoxelGame.GUI.Drawing.Brushes.Brush;

namespace VoxelGame.GUI.Visuals;

/// <summary>
///     Draws a border and background around its child visual.
/// </summary>
/// <see cref="Controls.Border" />
public class Border : Visual
{
    /// <summary>
    ///     Creates a new border visual.
    /// </summary>
    public Border()
    {
        BorderBrush = VisualProperty.Create(this, BindToOwnerForeground(), Invalidation.Render);
        BorderWidth = VisualProperty.Create(this, Width.One, Invalidation.Measure);
        BorderRadius = VisualProperty.Create(this, Radius.Zero, Invalidation.Render);
        BorderStrokeStyle = VisualProperty.Create(this, StrokeStyle.Solid, Invalidation.Render);
    }

    /// <summary>
    ///     Gets or sets the single child visual.
    /// </summary>
    public Visual? Child
    {
        get => Children.Count > 0 ? Children[0] : null;
        init => SetChild(value);
    }

    /// <inheritdoc />
    public override Size OnMeasure(Size availableSize)
    {
        Thickness borderThickness = BorderWidth.GetValue().ToThickness();

        availableSize -= borderThickness;

        Size desiredSize = base.OnMeasure(availableSize);

        desiredSize += borderThickness;

        return desiredSize;
    }

    /// <inheritdoc />
    public override void OnArrange(Rectangle finalRectangle)
    {
        finalRectangle -= BorderWidth.GetValue().ToThickness();
        finalRectangle -= Padding.GetValue();

        if (finalRectangle.IsEmpty)
            return;

        foreach (Visual child in Children)
            child.Arrange(finalRectangle);
    }

    /// <inheritdoc />
    protected override void OnRender()
    {
        Renderer.DrawFilledRectangle(LocalBounds, BorderRadius.GetValue(), Background.GetValue());
        Renderer.DrawLinedRectangle(LocalBounds, BorderWidth.GetValue(), BorderRadius.GetValue(), BorderStrokeStyle.GetValue(), BorderBrush.GetValue());
    }

    #region PROPERTIES

    /// <summary>
    ///     The brush used to draw the border.
    /// </summary>
    public VisualProperty<Brush> BorderBrush { get; }

    /// <summary>
    ///     The thickness of the border.
    /// </summary>
    public VisualProperty<Width> BorderWidth { get; }

    /// <summary>
    ///     The radius of the corners. This affects both the background and the border.
    /// </summary>
    public VisualProperty<Radius> BorderRadius { get; }

    /// <summary>
    ///     The style of the border stroke.
    /// </summary>
    public VisualProperty<StrokeStyle> BorderStrokeStyle { get; }

    #endregion PROPERTIES
}
