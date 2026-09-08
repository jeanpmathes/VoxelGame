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
using VoxelGame.GUI.Drawing;
using VoxelGame.GUI.Drawing.Brushes;
using VoxelGame.GUI.Texts;
using VoxelGame.GUI.Utilities;

namespace VoxelGame.GUI.Rendering;

/// <summary>
///     Abstract base class that helps implement renderers.
/// </summary>
public abstract class Renderer : IRenderer
{
    private Single scale = 1.0f;

    /// <inheritdoc />
    public abstract void Reset();

    /// <inheritdoc />
    public abstract void Submit();

    /// <inheritdoc />
    public abstract void PushOffset(Point offset);

    /// <inheritdoc />
    public abstract void PopOffset();

    /// <inheritdoc />
    public abstract void PushClip(Rectangle rectangle);

    /// <inheritdoc />
    public abstract void PopClip();

    /// <inheritdoc />
    public abstract void PushOpacity(Single opacity);

    /// <inheritdoc />
    public abstract void PopOpacity();

    /// <inheritdoc />
    public abstract IFormattedText CreateFormattedText(String text, TextOptions options);

    /// <inheritdoc />
    public abstract void DrawFilledRectangle(Rectangle rectangle, Radius corners, Brush brush);

    /// <inheritdoc />
    public abstract void DrawLinedRectangle(Rectangle rectangle, Width width, Radius corners, StrokeStyle stroke, Brush brush);

    /// <inheritdoc />
    public virtual void OnScale(Single newScale)
    {
        scale = newScale;
    }

    /// <summary>
    ///     Scale a point by applying the scale factor.
    /// </summary>
    /// <param name="point">The point to scale.</param>
    /// <returns>>The scaled point.</returns>
    protected Point ApplyScale(Point point)
    {
        return new Point(point.X * scale, point.Y * scale);
    }

    /// <summary>
    ///     Scale a size by applying the scale factor.
    /// </summary>
    /// <param name="size">The size to scale.</param>
    /// <returns>The scaled size.</returns>
    protected Size ApplyScale(Size size)
    {
        return new Size(size.Width * scale, size.Height * scale);
    }

    /// <summary>
    ///     Scale a rectangle by applying the scale factor.
    /// </summary>
    /// <param name="rectangle">The rectangle to scale.</param>
    /// <returns>The scaled rectangle.</returns>
    protected Rectangle ApplyScale(Rectangle rectangle)
    {
        return new Rectangle(rectangle.X * scale, rectangle.Y * scale, rectangle.Width * scale, rectangle.Height * scale);
    }

    /// <summary>
    ///     Apply the scale factor to a thickness, scaling each edge accordingly.
    /// </summary>
    /// <param name="thickness">The thickness to scale.</param>
    /// <returns>The scaled thickness.</returns>
    protected Thickness ApplyScale(Thickness thickness)
    {
        return new Thickness(
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
    protected Radius ApplyScale(Radius radius)
    {
        return new Radius(radius.X * scale, radius.Y * scale);
    }

    /// <summary>
    ///     Apply the scale factor to a width, scaling it accordingly.
    /// </summary>
    /// <param name="width">The width to scale.</param>
    /// <returns>The scaled width.</returns>
    protected Width ApplyScale(Width width)
    {
        return new Width(width.Value * scale);
    }

    /// <summary>
    ///     Apply the inverse of the scale factor to a point, effectively unscaling it.
    /// </summary>
    /// <param name="point">The point to unscale.</param>
    /// <returns>The unscaled point.</returns>
    protected Point ApplyInverseScale(Point point)
    {
        return new Point(point.X / scale, point.Y / scale);
    }

    /// <summary>
    ///     Apply the inverse of the scale factor to a size, effectively unscaling it.
    /// </summary>
    /// <param name="size">The size to unscale.</param>
    /// <returns>The unscaled size.</returns>
    protected Size ApplyInverseScale(Size size)
    {
        return new Size(size.Width / scale, size.Height / scale);
    }

    /// <summary>
    ///     Apply the inverse of the scale factor to a rectangle, effectively unscaling it.
    /// </summary>
    /// <param name="rectangle">The rectangle to unscale.</param>
    /// <returns>The unscaled rectangle.</returns>
    protected Rectangle ApplyInverseScale(Rectangle rectangle)
    {
        return new Rectangle(rectangle.X / scale, rectangle.Y / scale, rectangle.Width / scale, rectangle.Height / scale);
    }

    /// <summary>
    ///     Apply the inverse of the scale factor to a thickness, effectively unscaling it.
    /// </summary>
    /// <param name="thickness">The thickness to unscale.</param>
    /// <returns>The unscaled thickness.</returns>
    protected Thickness ApplyInverseScale(Thickness thickness)
    {
        return new Thickness(
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
    protected Radius ApplyInverseScale(Radius radius)
    {
        return new Radius(radius.X / scale, radius.Y / scale);
    }

    /// <summary>
    ///     Apply the inverse of the scale factor to a width, effectively unscaling it.
    /// </summary>
    /// <param name="width">The width to unscale.</param>
    /// <returns>The unscaled width.</returns>
    protected Width ApplyInverseScale(Width width)
    {
        return new Width(width.Value / scale);
    }
}
