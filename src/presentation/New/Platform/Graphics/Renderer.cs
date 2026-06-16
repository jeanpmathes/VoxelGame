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
using System.Diagnostics;
using System.Drawing;
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Definition.UserInterface;
using VoxelGame.Graphics.Objects.UserInterface;
using VoxelGame.GUI.Graphics;
using VoxelGame.GUI.Texts;
using VoxelGame.GUI.Utilities;
using PointF = System.Drawing.PointF;
using RectangleF = System.Drawing.RectangleF;
using Brush = VoxelGame.GUI.Graphics.Brush;

namespace VoxelGame.Presentation.New.Platform.Graphics;

/// <summary>
///     Implements rendering of the GUI for this specific platform, which uses DirectX and associated technologies.
/// </summary>
public sealed class Renderer : GUI.Rendering.Renderer, IDisposable
{
    private readonly Client client;
    private readonly VoxelGame.Graphics.Objects.UserInterface.Renderer renderer;
    private readonly CommandBuilder commands;

    private readonly BrushMap brushes;
    private readonly TextFormatMap textFormats;

    private Int32 skippedClipDepth;

    /// <summary>
    ///     Create a new instance of the <see cref="Renderer" /> class.
    /// </summary>
    /// <param name="client">The client providing access to the graphics API.</param>
    public Renderer(Client client)
    {
        this.client = client;

        renderer = client.CreateUserInterface(0);
        commands = new CommandBuilder();

        brushes = new BrushMap(this);
        textFormats = new TextFormatMap(this);
    }

    private Boolean IsCommandRecordingSuppressed => skippedClipDepth > 0;

    /// <inheritdoc />
    public void Dispose()
    {
        brushes.Dispose();
        textFormats.Dispose();

        renderer.Dispose();
    }

    /// <inheritdoc />
    public override void Reset()
    {
        commands.Clear();

        skippedClipDepth = 0;
    }

    /// <inheritdoc />
    public override void Submit()
    {
        Debug.Assert(skippedClipDepth == 0);

        renderer.Submit(commands.Commands);
    }

    /// <inheritdoc />
    public override void PushOffset(PointF offset)
    {
        if (IsCommandRecordingSuppressed)
            return;

        commands.PushOffset(Sanitize(ApplyScale(offset)));
    }

    /// <inheritdoc />
    public override void PopOffset()
    {
        if (IsCommandRecordingSuppressed)
            return;

        commands.PopOffset();
    }

    /// <inheritdoc />
    public override void PushClip(RectangleF rectangle)
    {
        if (IsCommandRecordingSuppressed)
        {
            skippedClipDepth += 1;

            return;
        }

        rectangle = Sanitize(ApplyScale(rectangle));

        if (rectangle.Width <= 0.0f || rectangle.Height <= 0.0f)
        {
            skippedClipDepth = 1;

            return;
        }

        commands.PushClip(rectangle);
    }

    /// <inheritdoc />
    public override void PopClip()
    {
        if (skippedClipDepth > 0)
        {
            skippedClipDepth -= 1;

            return;
        }

        commands.PopClip();
    }

    /// <inheritdoc />
    public override void PushOpacity(Single opacity)
    {
        if (IsCommandRecordingSuppressed)
            return;

        commands.PushOpacity(Math.Clamp(Single.IsFinite(opacity) ? opacity : 1.0f, min: 0.0f, max: 1.0f));
    }

    /// <inheritdoc />
    public override void PopOpacity()
    {
        if (IsCommandRecordingSuppressed)
            return;

        commands.PopOpacity();
    }

    /// <inheritdoc />
    public override IFormattedText CreateFormattedText(String text, TextOptions options)
    {
        return new FormattedText(this, text, options);
    }

    /// <inheritdoc />
    public override void DrawFilledRectangle(RectangleF rectangle, RadiusF corners, Brush brush)
    {
        if (IsCommandRecordingSuppressed)
            return;

        rectangle = Sanitize(ApplyScale(rectangle));

        if (rectangle.Width <= 0.0f || rectangle.Height <= 0.0f)
            return;

        corners = ApplyScale(corners);

        VoxelGame.Graphics.Objects.UserInterface.Brush? uiBrush = brushes.Get(brush, out Color? color);

        if (color != null)
            commands.DrawFilledRectangle(rectangle, corners, color.Value);
        else if (uiBrush != null)
            commands.DrawFilledRectangle(rectangle, corners, uiBrush);
    }

    /// <inheritdoc />
    public override void DrawLinedRectangle(RectangleF rectangle, WidthF width, RadiusF corners, StrokeStyle stroke, Brush brush)
    {
        if (IsCommandRecordingSuppressed)
            return;

        rectangle = Sanitize(ApplyScale(rectangle));

        if (rectangle.Width <= 0.0f || rectangle.Height <= 0.0f)
            return;

        width = ApplyScale(width);
        corners = ApplyScale(corners);

        VoxelGame.Graphics.Objects.UserInterface.Brush? uiBrush = brushes.Get(brush, out Color? color);

        if (color != null)
            commands.DrawLinedRectangle(rectangle, width, corners, stroke, color.Value);
        else if (uiBrush != null)
            commands.DrawLinedRectangle(rectangle, width, corners, stroke, uiBrush);
    }

    internal VoxelGame.Graphics.Objects.UserInterface.Brush CreateSolidColorBrush(Color color)
    {
        return renderer.CreateSolidColorBrush(color);
    }

    internal TextFormat CreateTextFormat(TextOptions options)
    {
        return renderer.CreateTextFormat(new TextFormatDescription
        {
            FontFamily = options.Font.Family,
            Weight = options.Font.Weight.Value,
            Size = options.Font.Size,
            Style = options.Font.Style,
            Stretch = options.Font.Stretch,
            Wrapping = options.Wrapping,
            Alignment = options.Alignment,
            Trimming = options.Trimming,
            LineHeight = options.LineHeight
        });
    }

    internal Text CreateText(String content, TextOptions options)
    {
        TextFormat textFormat = textFormats.Request(options);

        Text text = renderer.CreateText(content, textFormat);
        text.SetDisposeHandler(OnTextDisposed);

        return text;
    }

    private void OnTextDisposed(Text text)
    {
        textFormats.Return(text.Format);
    }

    internal void DrawText(Text text, PointF position, Brush brush)
    {
        if (IsCommandRecordingSuppressed)
            return;

        position = Sanitize(ApplyScale(position));

        VoxelGame.Graphics.Objects.UserInterface.Brush? uiBrush = brushes.Get(brush, out Color? color);

        if (color != null)
            commands.DrawText(text, position, color.Value);
        else if (uiBrush != null)
            commands.DrawText(text, position, uiBrush);
    }

    /// <summary>
    ///     Sanitize a size value, ensuring that the values are safe for the native rendering implementation.
    ///     This removes negative values and infinite values.
    /// </summary>
    /// <param name="size">The size to sanitize.</param>
    /// <returns>The sanitized size.</returns>
    internal SizeF Sanitize(SizeF size)
    {
        return new SizeF(
            SanitizeDimension(size.Width, client.Size.X),
            SanitizeDimension(size.Height, client.Size.Y));
    }

    private static Single SanitizeDimension(Single value, Int32 maximumRealDimensionValue)
    {
        if (Single.IsNaN(value) || value <= 0.0f) return 0.0f;

        return Single.IsInfinity(value)
            ? Math.Max(val1: 0.0f, maximumRealDimensionValue)
            : value;
    }

    /// <summary>
    ///     Sanitize a rectangle value, ensuring that the values are safe for the native rendering implementation.
    ///     This removes negative values and infinite values.
    /// </summary>
    /// <param name="rectangle">The rectangle to sanitize.</param>
    /// <returns>The sanitized rectangle.</returns>
    internal RectangleF Sanitize(RectangleF rectangle)
    {
        return new RectangleF(
            Sanitize(rectangle.X),
            Sanitize(rectangle.Y),
            SanitizeDimension(rectangle.Width, client.Size.X),
            SanitizeDimension(rectangle.Height, client.Size.Y));
    }

    private static PointF Sanitize(PointF point)
    {
        return new PointF(Sanitize(point.X), Sanitize(point.Y));
    }

    private static Single Sanitize(Single value)
    {
        return Single.IsFinite(value) ? value : 0.0f;
    }

    internal new SizeF ApplyScale(SizeF size)
    {
        return base.ApplyScale(size);
    }

    internal new SizeF ApplyInverseScale(SizeF size)
    {
        return base.ApplyInverseScale(size);
    }
}
