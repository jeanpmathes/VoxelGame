// <copyright file="CommandBuilder.cs" company="VoxelGame">
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
using VoxelGame.Graphics.Objects.UserInterface;
using VoxelGame.GUI.Graphics;
using VoxelGame.GUI.Utilities;
using Brush = VoxelGame.Graphics.Objects.UserInterface.Brush;

namespace VoxelGame.Graphics.Definition.UserInterface;

/// <summary>
///     Records native user-interface draw commands.
/// </summary>
public sealed class CommandBuilder
{
    /// <summary>
    ///     Get the current commands built with this command builder.
    /// </summary>
    public ReadOnlySpan<Command> Commands => throw
        // todo: Expose the recorded native UI command span.
        // Contract: callers must not assign union fields directly; commands are created only through helper methods.
        new NotImplementedException();

    /// <summary>
    ///     Clear all current commands.
    /// </summary>
    public void Clear()
    {
        // todo: Clear the recorded command list.
        // Contract: the builder only records calls and does not optimize opacity or stack operations.
    }

    /// <summary>
    ///     Add a call to the <c>PUSH_OFFSET</c> command.
    /// </summary>
    /// <remarks>
    ///     The <c>PUSH_OFFSET</c> command pushes an offset to the offset stack, which is applied to all later positions.
    /// </remarks>
    /// <seealso cref="CommandKind.PushOffset" />
    /// <param name="offset">The offset to push.</param>
    public void PushOffset(PointF offset)
    {
        // todo: Record a PushOffset command.
        _ = offset;
    }

    /// <summary>
    ///     Add a call to the <c>POP_OFFSET</c> command.
    /// </summary>
    /// <remarks>
    ///     The <c>POP_OFFSET</c> command pops the most recent offset from the offset stack.
    /// </remarks>
    /// <seealso cref="CommandKind.PopOffset" />
    public void PopOffset()
    {
        // todo: Record a PopOffset command.
    }

    /// <summary>
    ///     Add a call to the <c>PUSH_CLIP</c> command.
    /// </summary>
    /// <remarks>
    ///     The <c>PUSH_CLIP</c> command pushes a clipping rectangle to the clip stack.
    /// </remarks>
    /// <seealso cref="CommandKind.PushClip" />
    /// <param name="rectangle">The clipping rectangle to push.</param>
    public void PushClip(RectangleF rectangle)
    {
        // todo: Record a PushClip command.
        _ = rectangle;
    }

    /// <summary>
    ///     Add a call to the <c>POP_CLIP</c> command.
    /// </summary>
    /// <remarks>
    ///     The <c>POP_CLIP</c> command pops the most recent clipping rectangle from the clip stack.
    /// </remarks>
    /// <seealso cref="CommandKind.PopClip" />
    public void PopClip()
    {
        // todo: Record a PopClip command.
    }

    /// <summary>
    ///     Add a call to the <c>PUSH_OPACITY</c> command.
    /// </summary>
    /// <remarks>
    ///     The <c>PUSH_OPACITY</c> command pushes an opacity value to the opacity stack.
    /// </remarks>
    /// <seealso cref="CommandKind.PushOpacity" />
    /// <param name="opacity">The opacity value to push.</param>
    public void PushOpacity(Single opacity)
    {
        // todo: Record a PushOpacity command.
        _ = opacity;
    }

    /// <summary>
    ///     Add a call to the <c>POP_OPACITY</c> command.
    /// </summary>
    /// <remarks>
    ///     The <c>POP_OPACITY</c> command pops the most recent opacity value from the opacity stack.
    /// </remarks>
    /// <seealso cref="CommandKind.PopOpacity" />
    public void PopOpacity()
    {
        // todo: Record a PopOpacity command.
    }

    /// <summary>
    ///     Add a call to a filled rectangle color command.
    /// </summary>
    /// <param name="rectangle">The rectangle to draw.</param>
    /// <param name="radius">The corner radius.</param>
    /// <param name="color">The color to use.</param>
    public void DrawFilledRectangle(RectangleF rectangle, RadiusF radius, Color color)
    {
        // todo: Record a filled rectangle color command.
        // Contract: color commands are emitted for solid color brushes when possible.
        _ = rectangle;
        _ = radius;
        _ = color;
    }

    /// <summary>
    ///     Add a call to a filled rectangle brush command.
    /// </summary>
    /// <param name="rectangle">The rectangle to draw.</param>
    /// <param name="radius">The corner radius.</param>
    /// <param name="brush">The native brush to use.</param>
    public void DrawFilledRectangle(RectangleF rectangle, RadiusF radius, Brush brush)
    {
        // todo: Record a filled rectangle brush command.
        // Contract: brush commands are emitted only when a brush cannot be represented as a direct color command.
        _ = rectangle;
        _ = radius;
        _ = brush;
    }

    /// <summary>
    ///     Add a call to a lined rectangle color command.
    /// </summary>
    /// <param name="rectangle">The rectangle to draw.</param>
    /// <param name="width">The stroke width.</param>
    /// <param name="radius">The corner radius.</param>
    /// <param name="stroke">The stroke style.</param>
    /// <param name="color">The color to use.</param>
    public void DrawLinedRectangle(RectangleF rectangle, WidthF width, RadiusF radius, StrokeStyle stroke, Color color)
    {
        // todo: Record a lined rectangle color command.
        _ = rectangle;
        _ = width;
        _ = radius;
        _ = stroke;
        _ = color;
    }

    /// <summary>
    ///     Add a call to a lined rectangle brush command.
    /// </summary>
    /// <param name="rectangle">The rectangle to draw.</param>
    /// <param name="width">The stroke width.</param>
    /// <param name="radius">The corner radius.</param>
    /// <param name="stroke">The stroke style.</param>
    /// <param name="brush">The native brush to use.</param>
    public void DrawLinedRectangle(RectangleF rectangle, WidthF width, RadiusF radius, StrokeStyle stroke, Brush brush)
    {
        // todo: Record a lined rectangle brush command.
        _ = rectangle;
        _ = width;
        _ = radius;
        _ = stroke;
        _ = brush;
    }

    /// <summary>
    ///     Add a call to the <c>DRAW_TEXT_COLOR</c> command.
    /// </summary>
    /// <param name="text">The native text to draw.</param>
    /// <param name="position">The position at which to draw the text.</param>
    /// <param name="color">The color to use.</param>
    public void DrawText(Text text, PointF position, Color color)
    {
        // todo: Record a text color command.
        // Contract: the presentation renderer applies GUI scale before calling the builder.
        _ = text;
        _ = position;
        _ = color;
    }

    /// <summary>
    ///     Add a call to the <c>DRAW_TEXT_BRUSH</c> command.
    /// </summary>
    /// <param name="text">The native text to draw.</param>
    /// <param name="position">The position at which to draw the text.</param>
    /// <param name="brush">The native brush to use.</param>
    public void DrawText(Text text, PointF position, Brush brush)
    {
        // todo: Record a text brush command.
        _ = text;
        _ = position;
        _ = brush;
    }
}
