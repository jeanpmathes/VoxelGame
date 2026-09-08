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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using VoxelGame.Core.Visuals.Colors;
using VoxelGame.Graphics.Objects.UserInterface;
using VoxelGame.GUI.Drawing;
using VoxelGame.GUI.Utilities;

namespace VoxelGame.Graphics.Definition.UserInterface;

/// <summary>
///     Records native user-interface draw commands.
/// </summary>
public sealed class CommandBuilder
{
    private readonly List<Command> commands = [];

    /// <summary>
    ///     Get the current commands built with this command builder.
    /// </summary>
    public ReadOnlySpan<Command> Commands => CollectionsMarshal.AsSpan(commands);

    /// <summary>
    ///     Clear all current commands.
    /// </summary>
    public void Clear()
    {
        commands.Clear();
    }

    /// <summary>
    ///     Add a call to the <c>PUSH_OFFSET</c> command.
    /// </summary>
    /// <remarks>
    ///     The <c>PUSH_OFFSET</c> command pushes an offset to the offset stack, which is applied to all later positions.
    /// </remarks>
    /// <seealso cref="CommandKind.PushOffset" />
    /// <param name="offset">The offset to push.</param>
    public void PushOffset(Point offset)
    {
        commands.Add(new Command
        {
            Kind = CommandKind.PushOffset,
            PushOffset = new PushOffsetCommand(offset)
        });
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
        commands.Add(new Command
        {
            Kind = CommandKind.PopOffset
        });
    }

    /// <summary>
    ///     Add a call to the <c>PUSH_CLIP</c> command.
    /// </summary>
    /// <remarks>
    ///     The <c>PUSH_CLIP</c> command pushes a clipping rectangle to the clip stack.
    /// </remarks>
    /// <seealso cref="CommandKind.PushClip" />
    /// <param name="rectangle">The clipping rectangle to push.</param>
    public void PushClip(Rectangle rectangle)
    {
        commands.Add(new Command
        {
            Kind = CommandKind.PushClip,
            PushClip = new PushClipCommand(rectangle)
        });
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
        commands.Add(new Command
        {
            Kind = CommandKind.PopClip
        });
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
        commands.Add(new Command
        {
            Kind = CommandKind.PushOpacity,
            PushOpacity = new PushOpacityCommand(opacity)
        });
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
        commands.Add(new Command
        {
            Kind = CommandKind.PopOpacity
        });
    }

    /// <summary>
    ///     Add a call to a filled rectangle color command.
    /// </summary>
    /// <param name="rectangle">The rectangle to draw.</param>
    /// <param name="radius">The corner radius.</param>
    /// <param name="color">The color to use.</param>
    public void DrawFilledRectangle(Rectangle rectangle, Radius radius, ColorS color)
    {
        commands.Add(radius == Radius.Zero
            ? new Command
            {
                Kind = CommandKind.DrawRectangleilledColor,
                DrawRectangleilledColor = new DrawRectangleilledColorCommand(rectangle, color)
            }
            : new Command
            {
                Kind = CommandKind.DrawRectangleilledRoundedColor,
                DrawRectangleilledRoundedColor = new DrawRectangleilledRoundedColorCommand(rectangle, radius, color)
            });
    }

    /// <summary>
    ///     Add a call to a filled rectangle brush command.
    /// </summary>
    /// <param name="rectangle">The rectangle to draw.</param>
    /// <param name="radius">The corner radius.</param>
    /// <param name="brush">The native brush to use.</param>
    public void DrawFilledRectangle(Rectangle rectangle, Radius radius, Brush brush)
    {
        commands.Add(radius == Radius.Zero
            ? new Command
            {
                Kind = CommandKind.DrawRectangleilledBrush,
                DrawRectangleilledBrush = new DrawRectangleilledBrushCommand(rectangle, brush.Self)
            }
            : new Command
            {
                Kind = CommandKind.DrawRectangleilledRoundedBrush,
                DrawRectangleilledRoundedBrush = new DrawRectangleilledRoundedBrushCommand(rectangle, radius, brush.Self)
            });
    }

    /// <summary>
    ///     Add a call to a lined rectangle color command.
    /// </summary>
    /// <param name="rectangle">The rectangle to draw.</param>
    /// <param name="width">The stroke width.</param>
    /// <param name="radius">The corner radius.</param>
    /// <param name="stroke">The stroke style.</param>
    /// <param name="color">The color to use.</param>
    public void DrawLinedRectangle(Rectangle rectangle, Width width, Radius radius, StrokeStyle stroke, ColorS color)
    {
        commands.Add(radius == Radius.Zero
            ? new Command
            {
                Kind = CommandKind.DrawRectangleLinedColor,
                DrawRectangleLinedColor = new DrawRectangleLinedColorCommand(rectangle, color, width.Value, stroke)
            }
            : new Command
            {
                Kind = CommandKind.DrawRectangleLinedRoundedColor,
                DrawRectangleLinedRoundedColor = new DrawRectangleLinedRoundedColorCommand(rectangle, radius, color, width.Value, stroke)
            });
    }

    /// <summary>
    ///     Add a call to a lined rectangle brush command.
    /// </summary>
    /// <param name="rectangle">The rectangle to draw.</param>
    /// <param name="width">The stroke width.</param>
    /// <param name="radius">The corner radius.</param>
    /// <param name="stroke">The stroke style.</param>
    /// <param name="brush">The native brush to use.</param>
    public void DrawLinedRectangle(Rectangle rectangle, Width width, Radius radius, StrokeStyle stroke, Brush brush)
    {
        commands.Add(radius == Radius.Zero
            ? new Command
            {
                Kind = CommandKind.DrawRectangleLinedBrush,
                DrawRectangleLinedBrush = new DrawRectangleLinedBrushCommand(rectangle, brush.Self, width.Value, stroke)
            }
            : new Command
            {
                Kind = CommandKind.DrawRectangleLinedRoundedBrush,
                DrawRectangleLinedRoundedBrush = new DrawRectangleLinedRoundedBrushCommand(rectangle, radius, brush.Self, width.Value, stroke)
            });
    }

    /// <summary>
    ///     Add a call to the <c>DRAW_TEXT_COLOR</c> command.
    /// </summary>
    /// <param name="text">The native text to draw.</param>
    /// <param name="position">The position at which to draw the text.</param>
    /// <param name="color">The color to use.</param>
    public void DrawText(Text text, Point position, ColorS color)
    {
        commands.Add(new Command
        {
            Kind = CommandKind.DrawTextColor,
            DrawTextColor = new DrawTextColorCommand(text.Self, position, color)
        });
    }

    /// <summary>
    ///     Add a call to the <c>DRAW_TEXT_BRUSH</c> command.
    /// </summary>
    /// <param name="text">The native text to draw.</param>
    /// <param name="position">The position at which to draw the text.</param>
    /// <param name="brush">The native brush to use.</param>
    public void DrawText(Text text, Point position, Brush brush)
    {
        commands.Add(new Command
        {
            Kind = CommandKind.DrawTextBrush,
            DrawTextBrush = new DrawTextBrushCommand(text.Self, position, brush.Self)
        });
    }
}
