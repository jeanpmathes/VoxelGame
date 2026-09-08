// <copyright file="Commands.cs" company="VoxelGame">
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
using System.Runtime.InteropServices;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Visuals.Colors;
using VoxelGame.GUI.Drawing;
using VoxelGame.GUI.Utilities;

namespace VoxelGame.Graphics.Definition.UserInterface;

/// <summary>
///     Native UI command kind mapping to <c>ui::CommandKind</c>.
/// </summary>
internal enum CommandKind : UInt32
{
    /// <summary>
    ///     The <c>PUSH_OFFSET</c> command.
    ///     Pushes an offset to the offset stack, which is applied to all later positions.
    /// </summary>
    /// <seealso cref="PushOffsetCommand" />
    PushOffset = 0,

    /// <summary>
    ///     The <c>POP_OFFSET</c> command.
    ///     Pops the most recent offset from the offset stack.
    /// </summary>
    PopOffset = 1,

    /// <summary>
    ///     The <c>PUSH_CLIP</c> command.
    ///     Pushes a clipping rectangle to the clip stack.
    /// </summary>
    /// <remarks>
    /// Note that clipping and opacity cannot be interleaved.
    /// </remarks>
    /// <seealso cref="PushClipCommand" />
    PushClip = 2,

    /// <summary>
    ///     The <c>POP_CLIP</c> command.
    ///     Pops the most recent clipping rectangle from the clip stack.
    /// </summary>
    /// <remarks>
    /// Note that clipping and opacity cannot be interleaved.
    /// </remarks>
    PopClip = 3,

    /// <summary>
    ///     The <c>PUSH_OPACITY</c> command.
    ///     Pushes an opacity value to the opacity stack.
    /// </summary>
    /// <remarks>
    /// Note that clipping and opacity cannot be interleaved.
    /// </remarks>
    /// <seealso cref="PushOpacityCommand" />
    PushOpacity = 4,

    /// <summary>
    ///     The <c>POP_OPACITY</c> command.
    ///     Pops the most recent opacity value from the opacity stack.
    /// </summary>
    /// <remarks>
    /// Note that clipping and opacity cannot be interleaved.
    /// </remarks>
    PopOpacity = 5,

    /// <summary>
    ///     The <c>DRAW_RECTANGLE_LINED_COLOR</c> command.
    ///     Draws a non-rounded rectangle outline with a direct color.
    /// </summary>
    /// <seealso cref="DrawRectangleLinedColorCommand" />
    DrawRectangleLinedColor = 6,

    /// <summary>
    ///     The <c>DRAW_RECTANGLE_LINED_BRUSH</c> command.
    ///     Draws a non-rounded rectangle outline with a native brush.
    /// </summary>
    /// <seealso cref="DrawRectangleLinedBrushCommand" />
    DrawRectangleLinedBrush = 7,

    /// <summary>
    ///     The <c>DRAW_RECTANGLE_LINED_ROUNDED_COLOR</c> command.
    ///     Draws a rounded rectangle outline with a direct color.
    /// </summary>
    /// <seealso cref="DrawRectangleLinedRoundedColorCommand" />
    DrawRectangleLinedRoundedColor = 8,

    /// <summary>
    ///     The <c>DRAW_RECTANGLE_LINED_ROUNDED_BRUSH</c> command.
    ///     Draws a rounded rectangle outline with a native brush.
    /// </summary>
    /// <seealso cref="DrawRectangleLinedRoundedBrushCommand" />
    DrawRectangleLinedRoundedBrush = 9,

    /// <summary>
    ///     The <c>DRAW_RECTANGLE_FILLED_COLOR</c> command.
    ///     Draws a non-rounded filled rectangle with a direct color.
    /// </summary>
    /// <seealso cref="DrawRectangleilledColorCommand" />
    DrawRectangleilledColor = 10,

    /// <summary>
    ///     The <c>DRAW_RECTANGLE_FILLED_BRUSH</c> command.
    ///     Draws a non-rounded filled rectangle with a native brush.
    /// </summary>
    /// <seealso cref="DrawRectangleilledBrushCommand" />
    DrawRectangleilledBrush = 11,

    /// <summary>
    ///     The <c>DRAW_RECTANGLE_FILLED_ROUNDED_COLOR</c> command.
    ///     Draws a rounded filled rectangle with a direct color.
    /// </summary>
    /// <seealso cref="DrawRectangleilledRoundedColorCommand" />
    DrawRectangleilledRoundedColor = 12,

    /// <summary>
    ///     The <c>DRAW_RECTANGLE_FILLED_ROUNDED_BRUSH</c> command.
    ///     Draws a rounded filled rectangle with a native brush.
    /// </summary>
    /// <seealso cref="DrawRectangleilledRoundedBrushCommand" />
    DrawRectangleilledRoundedBrush = 13,

    /// <summary>
    ///     The <c>DRAW_TEXT_COLOR</c> command.
    ///     Draws text with a direct color.
    /// </summary>
    /// <seealso cref="DrawTextColorCommand" />
    DrawTextColor = 14,

    /// <summary>
    ///     The <c>DRAW_TEXT_BRUSH</c> command.
    ///     Draws text with a native brush.
    /// </summary>
    /// <seealso cref="DrawTextBrushCommand" />
    DrawTextBrush = 15
}

/// <summary>
///     Payload for the <see cref="CommandKind.PushOffset" /> command.
/// </summary>
/// <param name="offset">The offset to push.</param>
[ValueSemantics]
[StructLayout(LayoutKind.Sequential)]
internal readonly partial struct PushOffsetCommand(Point offset)
{
    /// <summary>
    ///     The offset to push.
    /// </summary>
    internal readonly Point Offset = offset;

    /// <inheritdoc />
    public override String ToString()
    {
        return FormattableString.Invariant($"{nameof(PushOffsetCommand)} {{ {nameof(Offset)} = ({Offset.X}, {Offset.Y}) }}");
    }
}

/// <summary>
///     Payload for the <see cref="CommandKind.PushClip" /> command.
/// </summary>
/// <param name="clip">The clipping rectangle to push.</param>
[ValueSemantics]
[StructLayout(LayoutKind.Sequential)]
internal readonly partial struct PushClipCommand(Rectangle clip)
{
    /// <summary>
    ///     The clipping rectangle to push.
    /// </summary>
    internal readonly Rectangle Clip = clip;

    /// <inheritdoc />
    public override String ToString()
    {
        return FormattableString.Invariant($"{nameof(PushClipCommand)} {{ {nameof(Clip)} = ({Clip.X}, {Clip.Y}, {Clip.Width}, {Clip.Height}) }}");
    }
}

/// <summary>
///     Payload for the <see cref="CommandKind.PushOpacity" /> command.
/// </summary>
/// <param name="opacity">The opacity value to push.</param>
[ValueSemantics]
[StructLayout(LayoutKind.Sequential)]
internal readonly partial struct PushOpacityCommand(Single opacity)
{
    /// <summary>
    ///     The opacity value to push.
    /// </summary>
    internal readonly Single Opacity = opacity;

    /// <inheritdoc />
    public override String ToString()
    {
        return FormattableString.Invariant($"{nameof(PushOpacityCommand)} {{ {nameof(Opacity)} = {Opacity} }}");
    }
}

/// <summary>
///     Payload for the <see cref="CommandKind.DrawRectangleLinedColor" /> command.
/// </summary>
/// <param name="rectangle">The rectangle to draw.</param>
/// <param name="color">The color to use.</param>
/// <param name="strokeWidth">The stroke width.</param>
/// <param name="strokeStyle">The stroke style.</param>
[ValueSemantics]
[StructLayout(LayoutKind.Sequential)]
internal readonly partial struct DrawRectangleLinedColorCommand(Rectangle rectangle, ColorS color, Single strokeWidth, StrokeStyle strokeStyle)
{
    /// <summary>
    ///     The rectangle to draw.
    /// </summary>
    internal readonly Rectangle Rectangle = rectangle;

    /// <summary>
    ///     The color to use.
    /// </summary>
    internal readonly ColorS Color = color;

    /// <summary>
    ///     The stroke width.
    /// </summary>
    internal readonly Single StrokeWidth = strokeWidth;

    /// <summary>
    ///     The stroke style.
    /// </summary>
    internal readonly StrokeStyle StrokeStyle = strokeStyle;

    /// <inheritdoc />
    public override String ToString()
    {
        return FormattableString.Invariant($"{nameof(DrawRectangleLinedColorCommand)} {{ {nameof(Rectangle)} = ({Rectangle.X}, {Rectangle.Y}, {Rectangle.Width}, {Rectangle.Height}), {nameof(Color)} = ({Color.R}, {Color.G}, {Color.B}, {Color.A}), {nameof(StrokeWidth)} = {StrokeWidth}, {nameof(StrokeStyle)} = {StrokeStyle.ToStringFast()} }}");
    }
}

/// <summary>
///     Payload for the <see cref="CommandKind.DrawRectangleLinedBrush" /> command.
/// </summary>
/// <param name="rectangle">The rectangle to draw.</param>
/// <param name="brush">The native brush to use.</param>
/// <param name="strokeWidth">The stroke width.</param>
/// <param name="strokeStyle">The stroke style.</param>
[ValueSemantics]
[StructLayout(LayoutKind.Sequential)]
internal readonly partial struct DrawRectangleLinedBrushCommand(Rectangle rectangle, IntPtr brush, Single strokeWidth, StrokeStyle strokeStyle)
{
    /// <summary>
    ///     The rectangle to draw.
    /// </summary>
    internal readonly Rectangle Rectangle = rectangle;

    /// <summary>
    ///     The native brush to use.
    /// </summary>
    internal readonly IntPtr Brush = brush;

    /// <summary>
    ///     The stroke width.
    /// </summary>
    internal readonly Single StrokeWidth = strokeWidth;

    /// <summary>
    ///     The stroke style.
    /// </summary>
    internal readonly StrokeStyle StrokeStyle = strokeStyle;

    /// <inheritdoc />
    public override String ToString()
    {
        return FormattableString.Invariant($"{nameof(DrawRectangleLinedBrushCommand)} {{ {nameof(Rectangle)} = ({Rectangle.X}, {Rectangle.Y}, {Rectangle.Width}, {Rectangle.Height}), {nameof(Brush)} = 0x{Brush.ToInt64():X}, {nameof(StrokeWidth)} = {StrokeWidth}, {nameof(StrokeStyle)} = {StrokeStyle.ToStringFast()} }}");
    }
}

/// <summary>
///     Payload for the <see cref="CommandKind.DrawRectangleLinedRoundedColor" /> command.
/// </summary>
/// <param name="rectangle">The rectangle to draw.</param>
/// <param name="radius">The corner radius.</param>
/// <param name="color">The color to use.</param>
/// <param name="strokeWidth">The stroke width.</param>
/// <param name="strokeStyle">The stroke style.</param>
[ValueSemantics]
[StructLayout(LayoutKind.Sequential)]
internal readonly partial struct DrawRectangleLinedRoundedColorCommand(Rectangle rectangle, Radius radius, ColorS color, Single strokeWidth, StrokeStyle strokeStyle)
{
    /// <summary>
    ///     The rectangle to draw.
    /// </summary>
    internal readonly Rectangle Rectangle = rectangle;

    /// <summary>
    ///     The corner radius.
    /// </summary>
    internal readonly Radius Radius = radius;

    /// <summary>
    ///     The color to use.
    /// </summary>
    internal readonly ColorS Color = color;

    /// <summary>
    ///     The stroke width.
    /// </summary>
    internal readonly Single StrokeWidth = strokeWidth;

    /// <summary>
    ///     The stroke style.
    /// </summary>
    internal readonly StrokeStyle StrokeStyle = strokeStyle;

    /// <inheritdoc />
    public override String ToString()
    {
        return FormattableString.Invariant(
            $"{nameof(DrawRectangleLinedRoundedColorCommand)} {{ {nameof(Rectangle)} = ({Rectangle.X}, {Rectangle.Y}, {Rectangle.Width}, {Rectangle.Height}), {nameof(Radius)} = ({Radius.X}, {Radius.Y}), {nameof(Color)} = ({Color.R}, {Color.G}, {Color.B}, {Color.A}), {nameof(StrokeWidth)} = {StrokeWidth}, {nameof(StrokeStyle)} = {StrokeStyle.ToStringFast()} }}");
    }
}

/// <summary>
///     Payload for the <see cref="CommandKind.DrawRectangleLinedRoundedBrush" /> command.
/// </summary>
/// <param name="rectangle">The rectangle to draw.</param>
/// <param name="radius">The corner radius.</param>
/// <param name="brush">The native brush to use.</param>
/// <param name="strokeWidth">The stroke width.</param>
/// <param name="strokeStyle">The stroke style.</param>
[ValueSemantics]
[StructLayout(LayoutKind.Sequential)]
internal readonly partial struct DrawRectangleLinedRoundedBrushCommand(Rectangle rectangle, Radius radius, IntPtr brush, Single strokeWidth, StrokeStyle strokeStyle)
{
    /// <summary>
    ///     The rectangle to draw.
    /// </summary>
    internal readonly Rectangle Rectangle = rectangle;

    /// <summary>
    ///     The corner radius.
    /// </summary>
    internal readonly Radius Radius = radius;

    /// <summary>
    ///     The native brush to use.
    /// </summary>
    internal readonly IntPtr Brush = brush;

    /// <summary>
    ///     The stroke width.
    /// </summary>
    internal readonly Single StrokeWidth = strokeWidth;

    /// <summary>
    ///     The stroke style.
    /// </summary>
    internal readonly StrokeStyle StrokeStyle = strokeStyle;

    /// <inheritdoc />
    public override String ToString()
    {
        return FormattableString.Invariant(
            $"{nameof(DrawRectangleLinedRoundedBrushCommand)} {{ {nameof(Rectangle)} = ({Rectangle.X}, {Rectangle.Y}, {Rectangle.Width}, {Rectangle.Height}), {nameof(Radius)} = ({Radius.X}, {Radius.Y}), {nameof(Brush)} = 0x{Brush.ToInt64():X}, {nameof(StrokeWidth)} = {StrokeWidth}, {nameof(StrokeStyle)} = {StrokeStyle.ToStringFast()} }}");
    }
}

/// <summary>
///     Payload for the <see cref="CommandKind.DrawRectangleilledColor" /> command.
/// </summary>
/// <param name="rectangle">The rectangle to draw.</param>
/// <param name="color">The color to use.</param>
[ValueSemantics]
[StructLayout(LayoutKind.Sequential)]
internal readonly partial struct DrawRectangleilledColorCommand(Rectangle rectangle, ColorS color)
{
    /// <summary>
    ///     The rectangle to draw.
    /// </summary>
    internal readonly Rectangle Rectangle = rectangle;

    /// <summary>
    ///     The color to use.
    /// </summary>
    internal readonly ColorS Color = color;

    /// <inheritdoc />
    public override String ToString()
    {
        return FormattableString.Invariant($"{nameof(DrawRectangleilledColorCommand)} {{ {nameof(Rectangle)} = ({Rectangle.X}, {Rectangle.Y}, {Rectangle.Width}, {Rectangle.Height}), {nameof(Color)} = ({Color.R}, {Color.G}, {Color.B}, {Color.A}) }}");
    }
}

/// <summary>
///     Payload for the <see cref="CommandKind.DrawRectangleilledBrush" /> command.
/// </summary>
/// <param name="rectangle">The rectangle to draw.</param>
/// <param name="brush">The native brush to use.</param>
[ValueSemantics]
[StructLayout(LayoutKind.Sequential)]
internal readonly partial struct DrawRectangleilledBrushCommand(Rectangle rectangle, IntPtr brush)
{
    /// <summary>
    ///     The rectangle to draw.
    /// </summary>
    internal readonly Rectangle Rectangle = rectangle;

    /// <summary>
    ///     The native brush to use.
    /// </summary>
    internal readonly IntPtr Brush = brush;

    /// <inheritdoc />
    public override String ToString()
    {
        return FormattableString.Invariant($"{nameof(DrawRectangleilledBrushCommand)} {{ {nameof(Rectangle)} = ({Rectangle.X}, {Rectangle.Y}, {Rectangle.Width}, {Rectangle.Height}), {nameof(Brush)} = 0x{Brush.ToInt64():X} }}");
    }
}

/// <summary>
///     Payload for the <see cref="CommandKind.DrawRectangleilledRoundedColor" /> command.
/// </summary>
/// <param name="rectangle">The rectangle to draw.</param>
/// <param name="radius">The corner radius.</param>
/// <param name="color">The color to use.</param>
[ValueSemantics]
[StructLayout(LayoutKind.Sequential)]
internal readonly partial struct DrawRectangleilledRoundedColorCommand(Rectangle rectangle, Radius radius, ColorS color)
{
    /// <summary>
    ///     The rectangle to draw.
    /// </summary>
    internal readonly Rectangle Rectangle = rectangle;

    /// <summary>
    ///     The corner radius.
    /// </summary>
    internal readonly Radius Radius = radius;

    /// <summary>
    ///     The color to use.
    /// </summary>
    internal readonly ColorS Color = color;

    /// <inheritdoc />
    public override String ToString()
    {
        return FormattableString.Invariant($"{nameof(DrawRectangleilledRoundedColorCommand)} {{ {nameof(Rectangle)} = ({Rectangle.X}, {Rectangle.Y}, {Rectangle.Width}, {Rectangle.Height}), {nameof(Radius)} = ({Radius.X}, {Radius.Y}), {nameof(Color)} = ({Color.R}, {Color.G}, {Color.B}, {Color.A}) }}");
    }
}

/// <summary>
///     Payload for the <see cref="CommandKind.DrawRectangleilledRoundedBrush" /> command.
/// </summary>
/// <param name="rectangle">The rectangle to draw.</param>
/// <param name="radius">The corner radius.</param>
/// <param name="brush">The native brush to use.</param>
[ValueSemantics]
[StructLayout(LayoutKind.Sequential)]
internal readonly partial struct DrawRectangleilledRoundedBrushCommand(Rectangle rectangle, Radius radius, IntPtr brush)
{
    /// <summary>
    ///     The rectangle to draw.
    /// </summary>
    internal readonly Rectangle Rectangle = rectangle;

    /// <summary>
    ///     The corner radius.
    /// </summary>
    internal readonly Radius Radius = radius;

    /// <summary>
    ///     The native brush to use.
    /// </summary>
    internal readonly IntPtr Brush = brush;

    /// <inheritdoc />
    public override String ToString()
    {
        return FormattableString.Invariant($"{nameof(DrawRectangleilledRoundedBrushCommand)} {{ {nameof(Rectangle)} = ({Rectangle.X}, {Rectangle.Y}, {Rectangle.Width}, {Rectangle.Height}), {nameof(Radius)} = ({Radius.X}, {Radius.Y}), {nameof(Brush)} = 0x{Brush.ToInt64():X} }}");
    }
}

/// <summary>
///     Payload for the <see cref="CommandKind.DrawTextColor" /> command.
/// </summary>
/// <param name="text">The native text to draw.</param>
/// <param name="position">The position at which to draw the text.</param>
/// <param name="color">The color to use.</param>
[ValueSemantics]
[StructLayout(LayoutKind.Sequential)]
internal readonly partial struct DrawTextColorCommand(IntPtr text, Point position, ColorS color)
{
    /// <summary>
    ///     The native text to draw.
    /// </summary>
    internal readonly IntPtr Text = text;

    /// <summary>
    ///     The position at which to draw the text.
    /// </summary>
    internal readonly Point Position = position;

    /// <summary>
    ///     The color to use.
    /// </summary>
    internal readonly ColorS Color = color;

    /// <inheritdoc />
    public override String ToString()
    {
        return FormattableString.Invariant($"{nameof(DrawTextColorCommand)} {{ {nameof(Text)} = 0x{Text.ToInt64():X}, {nameof(Position)} = ({Position.X}, {Position.Y}), {nameof(Color)} = ({Color.R}, {Color.G}, {Color.B}, {Color.A}) }}");
    }
}

/// <summary>
///     Payload for the <see cref="CommandKind.DrawTextBrush" /> command.
/// </summary>
/// <param name="text">The native text to draw.</param>
/// <param name="position">The position at which to draw the text.</param>
/// <param name="brush">The native brush to use.</param>
[ValueSemantics]
[StructLayout(LayoutKind.Sequential)]
internal readonly partial struct DrawTextBrushCommand(IntPtr text, Point position, IntPtr brush)
{
    /// <summary>
    ///     The native text to draw.
    /// </summary>
    internal readonly IntPtr Text = text;

    /// <summary>
    ///     The position at which to draw the text.
    /// </summary>
    internal readonly Point Position = position;

    /// <summary>
    ///     The native brush to use.
    /// </summary>
    internal readonly IntPtr Brush = brush;

    /// <inheritdoc />
    public override String ToString()
    {
        return FormattableString.Invariant($"{nameof(DrawTextBrushCommand)} {{ {nameof(Text)} = 0x{Text.ToInt64():X}, {nameof(Position)} = ({Position.X}, {Position.Y}), {nameof(Brush)} = 0x{Brush.ToInt64():X} }}");
    }
}

/// <summary>
///     UI command union mapping to <c>ui::Command</c>.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct Command
{
    /// <summary>
    ///     The command kind.
    /// </summary>
    [FieldOffset(0)] internal CommandKind Kind;

    /// <summary>
    ///     Payload for the <see cref="CommandKind.PushOffset" /> command.
    /// </summary>
    [FieldOffset(8)] internal PushOffsetCommand PushOffset;

    /// <summary>
    ///     Payload for the <see cref="CommandKind.PushClip" /> command.
    /// </summary>
    [FieldOffset(8)] internal PushClipCommand PushClip;

    /// <summary>
    ///     Payload for the <see cref="CommandKind.PushOpacity" /> command.
    /// </summary>
    [FieldOffset(8)] internal PushOpacityCommand PushOpacity;

    /// <summary>
    ///     Payload for the <see cref="CommandKind.DrawRectangleLinedColor" /> command.
    /// </summary>
    [FieldOffset(8)] internal DrawRectangleLinedColorCommand DrawRectangleLinedColor;

    /// <summary>
    ///     Payload for the <see cref="CommandKind.DrawRectangleLinedBrush" /> command.
    /// </summary>
    [FieldOffset(8)] internal DrawRectangleLinedBrushCommand DrawRectangleLinedBrush;

    /// <summary>
    ///     Payload for the <see cref="CommandKind.DrawRectangleLinedRoundedColor" /> command.
    /// </summary>
    [FieldOffset(8)] internal DrawRectangleLinedRoundedColorCommand DrawRectangleLinedRoundedColor;

    /// <summary>
    ///     Payload for the <see cref="CommandKind.DrawRectangleLinedRoundedBrush" /> command.
    /// </summary>
    [FieldOffset(8)] internal DrawRectangleLinedRoundedBrushCommand DrawRectangleLinedRoundedBrush;

    /// <summary>
    ///     Payload for the <see cref="CommandKind.DrawRectangleilledColor" /> command.
    /// </summary>
    [FieldOffset(8)] internal DrawRectangleilledColorCommand DrawRectangleilledColor;

    /// <summary>
    ///     Payload for the <see cref="CommandKind.DrawRectangleilledBrush" /> command.
    /// </summary>
    [FieldOffset(8)] internal DrawRectangleilledBrushCommand DrawRectangleilledBrush;

    /// <summary>
    ///     Payload for the <see cref="CommandKind.DrawRectangleilledRoundedColor" /> command.
    /// </summary>
    [FieldOffset(8)] internal DrawRectangleilledRoundedColorCommand DrawRectangleilledRoundedColor;

    /// <summary>
    ///     Payload for the <see cref="CommandKind.DrawRectangleilledRoundedBrush" /> command.
    /// </summary>
    [FieldOffset(8)] internal DrawRectangleilledRoundedBrushCommand DrawRectangleilledRoundedBrush;

    /// <summary>
    ///     Payload for the <see cref="CommandKind.DrawTextColor" /> command.
    /// </summary>
    [FieldOffset(8)] internal DrawTextColorCommand DrawTextColor;

    /// <summary>
    ///     Payload for the <see cref="CommandKind.DrawTextBrush" /> command.
    /// </summary>
    [FieldOffset(8)] internal DrawTextBrushCommand DrawTextBrush;

    /// <inheritdoc />
    public override readonly String ToString()
    {
        return Kind switch
        {
            CommandKind.PushOffset => PushOffset.ToString(),
            CommandKind.PopOffset => nameof(CommandKind.PopOffset),
            CommandKind.PushClip => PushClip.ToString(),
            CommandKind.PopClip => nameof(CommandKind.PopClip),
            CommandKind.PushOpacity => PushOpacity.ToString(),
            CommandKind.PopOpacity => nameof(CommandKind.PopOpacity),
            CommandKind.DrawRectangleLinedColor => DrawRectangleLinedColor.ToString(),
            CommandKind.DrawRectangleLinedBrush => DrawRectangleLinedBrush.ToString(),
            CommandKind.DrawRectangleLinedRoundedColor => DrawRectangleLinedRoundedColor.ToString(),
            CommandKind.DrawRectangleLinedRoundedBrush => DrawRectangleLinedRoundedBrush.ToString(),
            CommandKind.DrawRectangleilledColor => DrawRectangleilledColor.ToString(),
            CommandKind.DrawRectangleilledBrush => DrawRectangleilledBrush.ToString(),
            CommandKind.DrawRectangleilledRoundedColor => DrawRectangleilledRoundedColor.ToString(),
            CommandKind.DrawRectangleilledRoundedBrush => DrawRectangleilledRoundedBrush.ToString(),
            CommandKind.DrawTextColor => DrawTextColor.ToString(),
            CommandKind.DrawTextBrush => DrawTextBrush.ToString(),
            _ => Kind.ToStringFast()
        };
    }
}
