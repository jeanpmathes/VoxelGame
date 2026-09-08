// <copyright file="Definitions.hpp" company="VoxelGame">
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

#include <Windows.h>

namespace ui
{
    /**
     * \brief A two-dimensional point, corresponding to the type on the C# side.
     */
    struct Point
    {
        FLOAT x;
        FLOAT y;

        D2D1_POINT_2F ToD2D1() const;
    };

    /**
     * \brief A two-dimensional size, corresponding to the type on the C# side.
     */
    struct Size
    {
        FLOAT width;
        FLOAT height;

        D2D1_SIZE_F ToD2D1() const;
    };

    struct Radius;

    /**
     * \brief A rectangle in UI coordinates, corresponding to the type on the C# side.
     */
    struct Rectangle
    {
        FLOAT x;
        FLOAT y;
        FLOAT width;
        FLOAT height;

        D2D1_RECT_F       ToD2D1() const;
        D2D1_ROUNDED_RECT ToD2D1(Radius const& radius) const;
    };

    /**
     * \brief Horizontal and vertical radii used for rounded rectangles, corresponding to the type on the C# side.
     */
    struct Radius
    {
        FLOAT x;
        FLOAT y;
    };

    /**
     * \brief Linear color data, corresponding to the ColorS type on the C# side.
     */
    struct Color
    {
        FLOAT r;
        FLOAT g;
        FLOAT b;
        FLOAT a;

        D2D1_COLOR_F ToD2D1() const;
    };

    /**
     * \brief Stroke presets supported by the UI system.
     */
    enum class StrokeStyle : UINT8
    {
        SOLID   = 0,
        DASHES  = 1,
        SQUARED = 2,
        DOTTED  = 3,
    };

    /**
     * \brief Font-style values supported by the UI system.
     */
    enum class FontStyle : UINT8
    {
        NORMAL  = 0,
        ITALIC  = 1,
        OBLIQUE = 2,
    };

    /**
     * \brief Font-stretch values supported by the UI system.
     */
    enum class FontStretch : UINT8
    {
        ULTRA_CONDENSED = 0,
        EXTRA_CONDENSED = 1,
        CONDENSED       = 2,
        SEMI_CONDENSED  = 3,
        NORMAL          = 4,
        SEMI_EXPANDED   = 5,
        EXPANDED        = 6,
        EXTRA_EXPANDED  = 7,
        ULTRA_EXPANDED  = 8,
    };

    /**
     * \brief Horizontal text alignment modes supported by the UI system.
     */
    enum class TextAlignment : UINT8
    {
        LEADING  = 0,
        CENTER   = 1,
        TRAILING = 2,
        JUSTIFY  = 3,
    };

    /**
     * \brief Text wrapping modes supported by the UI system.
     */
    enum class TextWrapping : UINT8
    {
        NO_WRAP = 0,
        WRAP    = 1,
    };

    /**
     * \brief Text trimming modes supported by the UI system.
     */
    enum class TextTrimming : UINT8
    {
        NONE               = 0,
        CHARACTER          = 1,
        WORD               = 2,
        CHARACTER_ELLIPSIS = 3,
        WORD_ELLIPSIS      = 4,
        PATH_ELLIPSIS      = 5,
    };

    /**
     * \brief Text format description.
     */
    struct TextFormatDescription
    {
        LPCWSTR       fontFamily;
        INT16         weight;
        FontStyle     style;
        FontStretch   stretch;
        FLOAT         size;
        TextWrapping  wrapping;
        TextAlignment alignment;
        TextTrimming  trimming;
        FLOAT         lineHeight;
    };
}
