// <copyright file="Commands.hpp" company="VoxelGame">
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

#pragma once

#include <cstddef>
#include <type_traits>

#include "UserInterface/Definitions.hpp"

namespace ui
{
    class Brush;
    class Text;

    /**
     * \brief Identifies how the active payload of a UI command must be interpreted.
     */
    enum class CommandKind : UINT32
    {
        PUSH_OFFSET                         = 0,
        POP_OFFSET                          = 1,
        PUSH_CLIP                           = 2,
        POP_CLIP                            = 3,
        PUSH_OPACITY                        = 4,
        POP_OPACITY                         = 5,
        DRAW_RECTANGLE_LINED_COLOR          = 6,
        DRAW_RECTANGLE_LINED_BRUSH          = 7,
        DRAW_RECTANGLE_LINED_ROUNDED_COLOR  = 8,
        DRAW_RECTANGLE_LINED_ROUNDED_BRUSH  = 9,
        DRAW_RECTANGLE_FILLED_COLOR         = 10,
        DRAW_RECTANGLE_FILLED_BRUSH         = 11,
        DRAW_RECTANGLE_FILLED_ROUNDED_COLOR = 12,
        DRAW_RECTANGLE_FILLED_ROUNDED_BRUSH = 13,
        DRAW_TEXT_COLOR                     = 14,
        DRAW_TEXT_BRUSH                     = 15,
    };

    /**
     * \brief Payload for adding an offset to the current drawing transform.
     */
    struct PushOffsetCommand
    {
        PointF offset;
    };

    /**
     * \brief Payload for intersecting the current clip with another rectangle.
     */
    struct PushClipCommand
    {
        RectangleF clip;
    };

    /**
     * \brief Payload for multiplying the current opacity by another value.
     */
    struct PushOpacityCommand
    {
        FLOAT opacity;
    };

    /**
     * \brief Payload for drawing a non-rounded rectangle outline with a temporary color brush.
     */
    struct DrawRectangleLinedColorCommand
    {
        RectangleF  rectangle;
        ColorF      color;
        FLOAT       strokeWidth;
        StrokeStyle strokeStyle;
    };

    /**
     * \brief Payload for drawing a non-rounded rectangle outline with a reusable UI brush.
     */
    struct DrawRectangleLinedBrushCommand
    {
        RectangleF  rectangle;
        Brush*      brush;
        FLOAT       strokeWidth;
        StrokeStyle strokeStyle;
    };

    /**
     * \brief Payload for drawing a rounded rectangle outline with a temporary color brush.
     */
    struct DrawRectangleLinedRoundedColorCommand
    {
        RectangleF  rectangle;
        RadiusF     radius;
        ColorF      color;
        FLOAT       strokeWidth;
        StrokeStyle strokeStyle;
    };

    /**
     * \brief Payload for drawing a rounded rectangle outline with a reusable UI brush.
     */
    struct DrawRectangleLinedRoundedBrushCommand
    {
        RectangleF  rectangle;
        RadiusF     radius;
        Brush*      brush;
        FLOAT       strokeWidth;
        StrokeStyle strokeStyle;
    };

    /**
     * \brief Payload for filling a non-rounded rectangle with a temporary color brush.
     */
    struct DrawRectangleFilledColorCommand
    {
        RectangleF rectangle;
        ColorF     color;
    };

    /**
     * \brief Payload for filling a non-rounded rectangle with a reusable UI brush.
     */
    struct DrawRectangleFilledBrushCommand
    {
        RectangleF rectangle;
        Brush*     brush;
    };

    /**
     * \brief Payload for filling a rounded rectangle with a temporary color brush.
     */
    struct DrawRectangleFilledRoundedColorCommand
    {
        RectangleF rectangle;
        RadiusF    radius;
        ColorF     color;
    };

    /**
     * \brief Payload for filling a rounded rectangle with a reusable UI brush.
     */
    struct DrawRectangleFilledRoundedBrushCommand
    {
        RectangleF rectangle;
        RadiusF    radius;
        Brush*     brush;
    };

    /**
     * \brief Payload for drawing a measured text layout with a temporary color brush.
     */
    struct DrawTextColorCommand
    {
        Text*  text;
        PointF position;
        ColorF color;
    };

    /**
     * \brief Payload for drawing a measured text layout with a reusable UI brush.
     */
    struct DrawTextBrushCommand
    {
        Text*  text;
        PointF position;
        Brush* brush;
    };

    /**
     * \brief A command for drawing the UI.
     */
    struct Command
    {
        CommandKind kind;
        UINT32      padding = 0; // To ensure alignment of pointers and doubles in payloads.

        union
        {
            PushOffsetCommand                      pushOffset;
            PushClipCommand                        pushClip;
            PushOpacityCommand                     pushOpacity;
            DrawRectangleLinedColorCommand         drawRectangleLinedColor;
            DrawRectangleLinedBrushCommand         drawRectangleLinedBrush;
            DrawRectangleLinedRoundedColorCommand  drawRectangleLinedRoundedColor;
            DrawRectangleLinedRoundedBrushCommand  drawRectangleLinedRoundedBrush;
            DrawRectangleFilledColorCommand        drawRectangleFilledColor;
            DrawRectangleFilledBrushCommand        drawRectangleFilledBrush;
            DrawRectangleFilledRoundedColorCommand drawRectangleFilledRoundedColor;
            DrawRectangleFilledRoundedBrushCommand drawRectangleFilledRoundedBrush;
            DrawTextColorCommand                   drawTextColor;
            DrawTextBrushCommand                   drawTextBrush;
        };
    };

    static_assert(offsetof(Command, pushOffset) == 8);
    static_assert(std::is_trivially_copyable_v<Command>);
}
