// <copyright file="Text.cs" company="VoxelGame">
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
using VoxelGame.GUI.Texts;
using VoxelGame.GUI.Utilities;
using Brush = VoxelGame.GUI.Graphics.Brush;
using Font = VoxelGame.GUI.Texts.Font;

namespace VoxelGame.GUI.Visuals;

/// <summary>
///     Displays text.
/// </summary>
/// <seealso cref="Controls.Text" />
public class Text : Visual
{
    private IFormattedText? formattedText;

    /// <summary>
    ///     Creates a new instance of the <see cref="Text" /> class.
    /// </summary>
    public Text()
    {
        FontFamily = VisualProperty.Create(this, "", _ => InvalidateText());
        FontSize = VisualProperty.Create(this, defaultValue: 12f, _ => InvalidateText());
        FontStyle = VisualProperty.Create(this, Style.Normal, _ => InvalidateText());
        FontWeight = VisualProperty.Create(this, Weight.Normal, _ => InvalidateText());
        FontStretch = VisualProperty.Create(this, Stretch.Normal, _ => InvalidateText());

        Wrapping = VisualProperty.Create(this, TextWrapping.Wrap, _ => InvalidateText());
        Alignment = VisualProperty.Create(this, TextAlignment.Leading, _ => InvalidateText());
        Trimming = VisualProperty.Create(this, TextTrimming.None, _ => InvalidateText());
        LineHeight = VisualProperty.Create(this, defaultValue: 0f, _ => InvalidateText());

        Content = VisualProperty.Create(this, "", _ => InvalidateText());

        TextBrush = VisualProperty.Create(this, BindToOwnerForeground(), Invalidation.Render);
    }

    #region PROPERTIES

    /// <summary>
    ///     The family of the font used to display the text content.
    /// </summary>
    public VisualProperty<String> FontFamily { get; }

    /// <summary>
    ///     The size of the font used to display the text content.
    /// </summary>
    public VisualProperty<Single> FontSize { get; }

    /// <summary>
    ///     The style of the font used to display the text content, such as normal, italic, or oblique.
    /// </summary>
    public VisualProperty<Style> FontStyle { get; }

    /// <summary>
    ///     The weight of the font used to display the text content, such as normal, bold, or light.
    /// </summary>
    public VisualProperty<Weight> FontWeight { get; }

    /// <summary>
    ///     The stretch of the font used to display the text content, such as normal, condensed, or expanded.
    /// </summary>
    public VisualProperty<Stretch> FontStretch { get; }

    /// <summary>
    ///     How text wraps when it exceeds the layout width.
    /// </summary>
    public VisualProperty<TextWrapping> Wrapping { get; }

    /// <summary>
    ///     The horizontal alignment of text within the layout bounds.
    /// </summary>
    public VisualProperty<TextAlignment> Alignment { get; }

    /// <summary>
    ///     How text is trimmed when it overflows the layout bounds.
    /// </summary>
    public VisualProperty<TextTrimming> Trimming { get; }

    /// <summary>
    ///     The line height in the same units as the font size.
    ///     A value of <c>0</c> means the renderer uses the font's natural line height.
    /// </summary>
    public VisualProperty<Single> LineHeight { get; }

    /// <summary>
    ///     The text content to be displayed by the control.
    /// </summary>
    public VisualProperty<String> Content { get; }

    /// <summary>
    ///     The brush used to draw the text content.
    /// </summary>
    public VisualProperty<Brush> TextBrush { get; }

    #endregion PROPERTIES

    #region TEXT

    /// <inheritdoc />
    public override void OnAttach()
    {
        if (formattedText == null)
            CreateFormattedText();
    }

    /// <inheritdoc />
    public override void OnDetach(Boolean isReparenting)
    {
        if (isReparenting) return;

        formattedText?.Dispose();
        formattedText = null;
    }

    private void InvalidateText()
    {
        if (formattedText == null) return;

        formattedText.Dispose();
        formattedText = null;

        if (!IsAttached) return;

        CreateFormattedText();
    }

    private void CreateFormattedText()
    {
        formattedText = Renderer.CreateFormattedText(Content.GetValue(),
            new Font
            {
                Family = FontFamily.GetValue(),
                Size = FontSize.GetValue(),
                Style = FontStyle.GetValue(),
                Weight = FontWeight.GetValue(),
                Stretch = FontStretch.GetValue()
            },
            new TextOptions
            {
                Wrapping = Wrapping.GetValue(),
                Alignment = Alignment.GetValue(),
                Trimming = Trimming.GetValue(),
                LineHeight = LineHeight.GetValue()
            });

        InvalidateMeasure();
    }

    /// <inheritdoc />
    protected override void OnRender()
    {
        base.OnRender();

        formattedText?.Draw(finalTextRectangle, TextBrush.GetValue());
    }

    #endregion TEXT

    #region LAYOUT

    private RectangleF finalTextRectangle;

    /// <inheritdoc />
    public override SizeF OnMeasure(SizeF availableSize)
    {
        if (formattedText == null)
            return SizeF.Empty;

        availableSize -= Padding.GetValue();

        SizeF textSize = formattedText.Measure(Sizes.Max(SizeF.Empty, availableSize));

        return textSize + Padding.GetValue();
    }

    /// <inheritdoc />
    public override void OnArrange(RectangleF finalRectangle)
    {
        finalTextRectangle = finalRectangle - Padding.GetValue();
    }

    #endregion LAYOUT
}
