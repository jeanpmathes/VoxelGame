// <copyright file="Renderer.cs" company="VoxelGame">
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
using VoxelGame.GUI.Graphics;
using VoxelGame.GUI.Texts;
using VoxelGame.GUI.Utilities;
using Brush = VoxelGame.GUI.Graphics.Brush;
using Font = VoxelGame.GUI.Texts.Font;

namespace VoxelGame.GUI.Rendering;

/// <summary>
///     Abstract base class that helps implement renderers.
/// </summary>
public abstract class Renderer : IRenderer
{
    private Single scale = 1.0f;

    /// <inheritdoc />
    public abstract void Begin();

    /// <inheritdoc />
    public abstract void End();

    /// <inheritdoc />
    public abstract void PushOffset(PointF offset);

    /// <inheritdoc />
    public abstract void PopOffset();

    /// <inheritdoc />
    public abstract void PushClip(RectangleF rectangle);

    /// <inheritdoc />
    public abstract void PopClip();

    /// <inheritdoc />
    public abstract void BeginClip();

    /// <inheritdoc />
    public abstract void EndClip();

    /// <inheritdoc />
    public abstract Boolean IsClipEmpty();

    /// <inheritdoc />
    public abstract void PushOpacity(Single opacity);

    /// <inheritdoc />
    public abstract void PopOpacity();

    /// <inheritdoc />
    public abstract IFormattedText CreateFormattedText(String text, Font font, TextOptions options);

    /// <inheritdoc />
    public abstract void DrawFilledRectangle(RectangleF rectangle, RadiusF corners, Brush brush);

    /// <inheritdoc />
    public abstract void DrawLinedRectangle(RectangleF rectangle, WidthF width, RadiusF corners, StrokeStyle stroke, Brush brush);

    /// <inheritdoc />
    public abstract void Resize(Size size);

    /// <inheritdoc />
    public virtual void Scale(Single newScale)
    {
        scale = newScale;
    }

    /// <summary>
    ///     Scale a point by applying the scale factor.
    /// </summary>
    /// <param name="point">The point to scale.</param>
    /// <returns>>The scaled point.</returns>
    protected PointF ApplyScale(PointF point)
    {
        point.X *= scale;
        point.Y *= scale;

        return point;
    }

    /// <summary>
    ///     Scale a size by applying the scale factor.
    /// </summary>
    /// <param name="size">The size to scale.</param>
    /// <returns>The scaled size.</returns>
    protected SizeF ApplyScale(SizeF size)
    {
        size.Width *= scale;
        size.Height *= scale;

        return size;
    }

    /// <summary>
    ///     Scale a rectangle by applying the scale factor.
    /// </summary>
    /// <param name="rectangle">The rectangle to scale.</param>
    /// <returns>The scaled rectangle.</returns>
    protected RectangleF ApplyScale(RectangleF rectangle)
    {
        rectangle.X *= scale;
        rectangle.Y *= scale;
        rectangle.Width *= scale;
        rectangle.Height *= scale;

        return rectangle;
    }

    /// <summary>
    ///     Apply the scale factor to a thickness, scaling each edge accordingly.
    /// </summary>
    /// <param name="thickness">The thickness to scale.</param>
    /// <returns>The scaled thickness.</returns>
    protected ThicknessF ApplyScale(ThicknessF thickness)
    {
        return new ThicknessF(
            thickness.Left * scale,
            thickness.Top * scale,
            thickness.Right * scale,
            thickness.Bottom * scale);
    }

    /// <summary>
    ///     Apply the scale factor to a radius, scaling both the X and Y values accordingly.
    /// </summary>
    /// <param name="radius">The radius to scale.</param>
    /// <returns>The scaled radius.</returns>
    protected RadiusF ApplyScale(RadiusF radius)
    {
        return new RadiusF(radius.X * scale, radius.Y * scale);
    }

    /// <summary>
    ///     Apply the scale factor to a width, scaling it accordingly.
    /// </summary>
    /// <param name="width">The width to scale.</param>
    /// <returns>The scaled width.</returns>
    protected WidthF ApplyScale(WidthF width)
    {
        return new WidthF(width.Value * scale);
    }

    /// <summary>
    ///     Apply the inverse of the scale factor to a point, effectively unscaling it.
    /// </summary>
    /// <param name="point">The point to unscale.</param>
    /// <returns>The unscaled point.</returns>
    protected PointF ApplyInverseScale(PointF point)
    {
        point.X /= scale;
        point.Y /= scale;

        return point;
    }

    /// <summary>
    ///     Apply the inverse of the scale factor to a size, effectively unscaling it.
    /// </summary>
    /// <param name="size">The size to unscale.</param>
    /// <returns>The unscaled size.</returns>
    protected SizeF ApplyInverseScale(SizeF size)
    {
        size.Width /= scale;
        size.Height /= scale;

        return size;
    }

    /// <summary>
    ///     Apply the inverse of the scale factor to a rectangle, effectively unscaling it.
    /// </summary>
    /// <param name="rectangle">The rectangle to unscale.</param>
    /// <returns>The unscaled rectangle.</returns>
    protected RectangleF ApplyInverseScale(RectangleF rectangle)
    {
        rectangle.X /= scale;
        rectangle.Y /= scale;
        rectangle.Width /= scale;
        rectangle.Height /= scale;

        return rectangle;
    }

    /// <summary>
    ///     Apply the inverse of the scale factor to a thickness, effectively unscaling it.
    /// </summary>
    /// <param name="thickness">The thickness to unscale.</param>
    /// <returns>The unscaled thickness.</returns>
    protected ThicknessF ApplyInverseScale(ThicknessF thickness)
    {
        return new ThicknessF(
            thickness.Left / scale,
            thickness.Top / scale,
            thickness.Right / scale,
            thickness.Bottom / scale);
    }

    /// <summary>
    ///     Apply the inverse of the scale factor to a radius, effectively unscaling it.
    /// </summary>
    /// <param name="radius">The radius to unscale.</param>
    /// <returns>The unscaled radius.</returns>
    protected RadiusF ApplyInverseScale(RadiusF radius)
    {
        return new RadiusF(radius.X / scale, radius.Y / scale);
    }

    /// <summary>
    ///     Apply the inverse of the scale factor to a width, effectively unscaling it.
    /// </summary>
    /// <param name="width">The width to unscale.</param>
    /// <returns>The unscaled width.</returns>
    protected WidthF ApplyInverseScale(WidthF width)
    {
        return new WidthF(width.Value / scale);
    }
}
