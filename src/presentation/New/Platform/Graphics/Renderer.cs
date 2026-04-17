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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Gwen.Net;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Graphics.Core;
using VoxelGame.GUI.Graphics;
using VoxelGame.GUI.Texts;
using VoxelGame.GUI.Utilities;
using VoxelGame.Presentation.Legacy.Platform;
using VoxelGame.Presentation.Legacy.Platform.Renderer;
using Size = System.Drawing.Size;
using SizeF = System.Drawing.SizeF;
using PointF = System.Drawing.PointF;
using RectangleF = System.Drawing.RectangleF;
using Bitmap = System.Drawing.Bitmap;
using Brush = VoxelGame.GUI.Graphics.Brush;
using Color = System.Drawing.Color;
using Font = VoxelGame.GUI.Texts.Font;
using FontStyle = System.Drawing.FontStyle;
using Image = VoxelGame.Core.Visuals.Image;
using Point = Gwen.Net.Point;
using Rectangle = Gwen.Net.Rectangle;
using Region = System.Drawing.Region;

namespace VoxelGame.Presentation.New.Platform.Graphics;

/// <summary>
///     Implements rendering of the GUI for this specific platform, which uses DirectX and associated technologies.
/// </summary>
public sealed class Renderer : GUI.Rendering.Renderer, IDisposable
{
    private readonly Client client;
    private readonly DirectXRenderer renderer;

    private readonly Dictionary<(Brush brush, Byte alpha), System.Drawing.Brush> systemBrushes = [];
    private readonly Dictionary<(Brush brush, Byte alpha, Single width, StrokeStyle stroke), Pen> systemPens = [];
    private readonly Dictionary<Font, System.Drawing.Font> systemFonts = [];

    private readonly Texture texture;

    private Bitmap? bitmap;
    private System.Drawing.Graphics? graphics;

    /// <summary>
    ///     Create a new instance of the <see cref="Renderer" /> class.
    /// </summary>
    /// <param name="client">The client providing access to the graphics API.</param>
    public Renderer(Client client)
    {
        this.client = client;

        renderer = new DirectXRenderer(client,
            GwenGuiSettings.Default.From(settings =>
            {
                settings.ShaderFile = FileSystem.GetResourceDirectory("Shaders").GetFile("GUI.hlsl");
            }));

        texture = new Texture(renderer);

        CreateTargets(new Size(client.Size.X, client.Size.Y));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (Pen pen in systemPens.Values)
            pen.Dispose();

        foreach (System.Drawing.Brush brush in systemBrushes.Values)
            brush.Dispose();

        foreach (System.Drawing.Font font in systemFonts.Values)
            font.Dispose();

        texture.Dispose();
        renderer.Dispose();

        graphics?.Dispose();
        bitmap?.Dispose();
    }

    #region TARGETS

    private void CreateTargets(Size size)
    {
        renderer.FinishLoading();

        ResizeTargets(size);
    }

    private void ResizeTargets(Size size)
    {
        renderer.Resize(new Vector2i(size.Width, size.Height));

        graphics?.Dispose();
        bitmap?.Dispose();

        bitmap = null;
        graphics = null;

        if (size is not {Width: > 0, Height: > 0}) return;

        bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
        graphics = System.Drawing.Graphics.FromImage(bitmap);

        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        if (isClipping && clipStack.Count > 0)
        {
            ApplyClippingRectangle(clipStack.Peek());
        }
        else if (offsetStack.Count > 0)
        {
            PointF offset = offsetStack.Peek();
            graphics.TranslateTransform(offset.X, offset.Y);
        }
    }

    /// <inheritdoc />
    public override void Begin()
    {
        renderer.Begin();

        EndClip();

        graphics?.Clear(Color.Transparent);
    }

    /// <inheritdoc />
    public override void End()
    {
        EndClip();

        if (bitmap != null)
        {
            Image image = new(bitmap);
            renderer.LoadTextureDirectly(texture, image);

            Rectangle screen = new(new Point(), new Gwen.Net.Size(client.Size.X, client.Size.Y));

            renderer.DrawColor = Gwen.Net.Color.White;
            renderer.DrawFilledRect(screen);
            renderer.DrawTexturedRect(texture, screen);
        }

        renderer.End();
    }

    /// <inheritdoc />
    public override void Resize(Size size)
    {
        ResizeTargets(size);
    }

    #endregion TARGETS

    #region TRANSFORM & CLIP & OPACITY

    private readonly Stack<PointF> offsetStack = new();
    private readonly Stack<RectangleF> clipStack = new();
    private readonly Stack<Single> opacityStack = new();

    private Boolean isClipping;

    private Single CurrentOpacity => opacityStack.Count > 0 ? opacityStack.Peek() : 1.0f;

    /// <inheritdoc />
    public override void PushOffset(PointF offset)
    {
        offset = ApplyScale(offset);

        graphics?.TranslateTransform(offset.X, offset.Y, MatrixOrder.Append);

        if (offsetStack.Count > 0)
        {
            PointF previousOffset = offsetStack.Peek();
            offset = new PointF(previousOffset.X + offset.X, previousOffset.Y + offset.Y);
        }

        offsetStack.Push(offset);
    }

    /// <inheritdoc />
    public override void PopOffset()
    {
        offsetStack.Pop();

        graphics?.ResetTransform();

        if (offsetStack.Count > 0)
        {
            PointF offset = offsetStack.Peek();
            graphics?.TranslateTransform(offset.X, offset.Y, MatrixOrder.Append);
        }
    }

    /// <inheritdoc />
    public override void PushClip(RectangleF rectangle)
    {
        rectangle = ApplyScale(rectangle);

        PointF absoluteOffset = offsetStack.Count > 0
            ? offsetStack.Peek()
            : new PointF(x: 0f, y: 0f);

        RectangleF effectiveRectangle = rectangle with
        {
            X = rectangle.X + absoluteOffset.X,
            Y = rectangle.Y + absoluteOffset.Y
        };

        if (clipStack.Count > 0)
            effectiveRectangle = RectangleF.Intersect(clipStack.Peek(), effectiveRectangle);

        clipStack.Push(effectiveRectangle);

        if (isClipping)
            ApplyClippingRectangle(effectiveRectangle);
    }

    /// <inheritdoc />
    public override void PopClip()
    {
        clipStack.Pop();

        if (!isClipping) return;

        graphics?.ResetClip();

        if (clipStack.Count > 0)
            ApplyClippingRectangle(clipStack.Peek());
    }

    /// <inheritdoc />
    public override void BeginClip()
    {
        if (isClipping) return;

        isClipping = true;

        if (clipStack.Count > 0)
            ApplyClippingRectangle(clipStack.Peek());
    }

    private void ApplyClippingRectangle(RectangleF effectiveRectangle)
    {
        if (graphics == null) return;

        graphics.ResetTransform();
        graphics.SetClip(effectiveRectangle, CombineMode.Replace);

        PointF absoluteOffset = offsetStack.Count > 0
            ? offsetStack.Peek()
            : new PointF(x: 0f, y: 0f);

        if (absoluteOffset.X != 0f || absoluteOffset.Y != 0f)
            graphics.TranslateTransform(absoluteOffset.X, absoluteOffset.Y);
    }

    /// <inheritdoc />
    public override void EndClip() // todo: check when this is ever called, maybe this can be simplified
    {
        if (!isClipping) return;

        isClipping = false;

        graphics?.ResetClip();
    }

    /// <inheritdoc />
    public override Boolean IsClipEmpty()
    {
        if (graphics == null) return true;

        Region clip = graphics.Clip;
        return clip.IsEmpty(graphics);
    }

    /// <inheritdoc />
    public override void PushOpacity(Single opacity)
    {
        opacity = Math.Clamp(opacity, min: 0.0f, max: 1.0f);

        opacityStack.Push(CurrentOpacity * opacity);
    }

    /// <inheritdoc />
    public override void PopOpacity()
    {
        if (opacityStack.Count <= 0) return;

        opacityStack.Pop();
    }

    #endregion TRANSFORM & CLIP & OPACITY

    #region TEXT

    /// <inheritdoc />
    public override IFormattedText CreateFormattedText(String text, Font font, TextOptions options)
    {
        return new FormattedText(this, text, font, options);
    }

    internal SizeF MeasureText(FormattedText text, SizeF availableSize)
    {
        if (graphics == null) return SizeF.Empty;

        availableSize = ApplyScale(availableSize);

        if (availableSize.Width <= 0.0f || availableSize.Height <= 0.0f)
            availableSize = SizeF.Empty;

        if (String.IsNullOrEmpty(text.Text))
            return SizeF.Empty;

        System.Drawing.Font systemFont = GetFont(text.Font);

        RectangleF layoutRectangle = new(PointF.Empty, availableSize);

        using StringFormat stringFormat = (StringFormat) text.StringFormat.Clone();
        stringFormat.SetMeasurableCharacterRanges([new CharacterRange(First: 0, text.Text.Length)]);

        Region[] regions = graphics.MeasureCharacterRanges(text.Text, systemFont, layoutRectangle, stringFormat);

        try
        {
            RectangleF bounds = regions[0].GetBounds(graphics);
            return ApplyInverseScale(bounds.Size);
        }
        finally
        {
            foreach (Region region in regions)
                region.Dispose();
        }
    }

    internal void DrawText(FormattedText text, RectangleF rectangle, Brush brush)
    {
        rectangle = ApplyScale(rectangle);

        System.Drawing.Brush? systemBrush = GetBrush(brush);

        if (systemBrush == null) return;

        System.Drawing.Font systemFont = GetFont(text.Font);

        graphics?.DrawString(text.Text, systemFont, systemBrush, rectangle, text.StringFormat);
    }

    #endregion TEXT

    #region RECTANGLES

    /// <inheritdoc />
    public override void DrawFilledRectangle(RectangleF rectangle, RadiusF corners, Brush brush)
    {
        System.Drawing.Brush? systemBrush = GetBrush(brush);

        if (systemBrush == null) return;

        rectangle = ApplyScale(rectangle);
        corners = ApplyScale(corners);

        if (corners == RadiusF.Zero)
        {
            graphics?.FillRectangle(systemBrush, rectangle);
        }
        else
        {
            graphics?.FillRoundedRectangle(systemBrush, rectangle, corners.ToSizeF());
        }
    }

    /// <inheritdoc />
    public override void DrawLinedRectangle(RectangleF rectangle, WidthF width, RadiusF corners, StrokeStyle stroke, Brush brush)
    {
        System.Drawing.Brush? systemBrush = GetBrush(brush);

        if (systemBrush == null) return;

        rectangle = ApplyScale(rectangle);
        width = ApplyScale(width);
        corners = ApplyScale(corners);

        if (width == WidthF.Zero) return;

        Pen? systemPen = GetPen(brush, width.Value, stroke);
        if (systemPen == null) return;

        Single halfWidth = width.Value / 2f;
        RectangleF adjustedRectangle = new(rectangle.X + halfWidth, rectangle.Y + halfWidth, rectangle.Width - width.Value, rectangle.Height - width.Value);

        if (corners == RadiusF.Zero)
        {
            graphics?.DrawRectangle(systemPen, adjustedRectangle);
        }
        else
        {
            graphics?.DrawRoundedRectangle(systemPen, adjustedRectangle, corners.ToSizeF());
        }
    }

    #endregion RECTANGLES

    #region MAPPINGS

    private Byte GetEffectiveAlpha(Byte alpha)
    {
        return (Byte) Math.Clamp((Int32) Math.Round(alpha * CurrentOpacity), Byte.MinValue, Byte.MaxValue);
    }

    private Byte? TryGetEffectiveAlpha(Brush brush)
    {
        return brush switch
        {
            SolidColorBrush solidColorBrush => GetEffectiveAlpha(solidColorBrush.Color.A),
            TransparentBrush => 0,
            _ => null
        };
    }

    private System.Drawing.Brush? GetBrush(Brush brush)
    {
        Byte? alpha = TryGetEffectiveAlpha(brush);

        if (alpha is null or 0)
            return null;

        (Brush, Byte) key = (brush, alpha.Value);

        if (systemBrushes.TryGetValue(key, out System.Drawing.Brush? systemBrush))
            return systemBrush;

        systemBrush = brush switch
        {
            SolidColorBrush solidColorBrush => new SolidBrush(Color.FromArgb(alpha.Value, solidColorBrush.Color.R, solidColorBrush.Color.G, solidColorBrush.Color.B)),
            _ => null
        };

        if (systemBrush != null)
            systemBrushes[key] = systemBrush;

        return systemBrush;
    }

    private Pen? GetPen(Brush brush, Single width, StrokeStyle stroke)
    {
        Byte? alpha = TryGetEffectiveAlpha(brush);

        if (alpha is null or 0)
            return null;

        (Brush, Byte, Single, StrokeStyle) key = (brush, alpha.Value, width, stroke);

        if (systemPens.TryGetValue(key, out Pen? systemPen))
            return systemPen;

        System.Drawing.Brush? systemBrush = GetBrush(brush);

        if (systemBrush == null) return null;

        systemPen = new Pen(systemBrush, width);
        ApplyStrokeStyleToPen(systemPen, stroke);

        systemPens[key] = systemPen;

        return systemPen;
    }

    private System.Drawing.Font GetFont(Font font)
    {
        if (systemFonts.TryGetValue(font, out System.Drawing.Font? systemFont))
            return systemFont;

        systemFont = new System.Drawing.Font(font.Family, font.Size, GetFontStyleFromTextsStyle(font.Style) | GetFontStyleFromTextsWeight(font.Weight));

        systemFonts[font] = systemFont;

        return systemFont;
    }

    private static FontStyle GetFontStyleFromTextsStyle(Style style)
    {
        return style switch
        {
            Style.Normal => FontStyle.Regular,
            Style.Italic or Style.Oblique => FontStyle.Italic,
            _ => FontStyle.Regular
        };
    }

    private static FontStyle GetFontStyleFromTextsWeight(Weight weight)
    {
        return weight >= Weight.SemiBold ? FontStyle.Bold : FontStyle.Regular;
    }

    private static void ApplyStrokeStyleToPen(Pen pen, StrokeStyle strokeStyle)
    {
        switch (strokeStyle)
        {
            case StrokeStyle.Solid:
                pen.DashStyle = DashStyle.Solid;
                pen.DashCap = DashCap.Flat;
                break;
            case StrokeStyle.Dashes:
                pen.DashStyle = DashStyle.Dash;
                pen.DashCap = DashCap.Flat;
                break;
            case StrokeStyle.Squared:
                pen.DashStyle = DashStyle.Dot;
                pen.DashCap = DashCap.Flat;
                break;
            case StrokeStyle.Dotted:
                pen.DashStyle = DashStyle.Dot;
                pen.DashCap = DashCap.Round;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(strokeStyle), strokeStyle, message: null);
        }
    }

    #endregion MAPPINGS
}
