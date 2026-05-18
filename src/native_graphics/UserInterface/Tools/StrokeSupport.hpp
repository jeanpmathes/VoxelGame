// <copyright file="StrokeSupport.hpp" company="VoxelGame">
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

#include "UserInterface/Definitions.hpp"

namespace ui
{
    class Context;

    /**
     * \brief Owns the Direct2D stroke-style presets for one renderer.
     */
    class StrokeSupport final
    {
    public:
        explicit StrokeSupport(Renderer& renderer);

        /**
         * \brief Resolve a UI stroke-style value to a Direct2D stroke style.
         * \param style The UI stroke-style preset.
         * \returns The Direct2D stroke style.
         */
        ID2D1StrokeStyle* Get(StrokeStyle style);

    private:
        Renderer& renderer;

        ComPtr<ID2D1StrokeStyle> solid;
        ComPtr<ID2D1StrokeStyle> dashes;
        ComPtr<ID2D1StrokeStyle> squared;
        ComPtr<ID2D1StrokeStyle> dotted;
    };
}
