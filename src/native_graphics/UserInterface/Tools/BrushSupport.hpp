// <copyright file="BrushSupport.hpp" company="VoxelGame">
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

#include "UserInterface/Objects/Brush.hpp"

namespace ui
{
    class Renderer;

    /**
     * \brief Creates and owns UI brushes for one UI renderer, as well as performing pooling.
     */
    class BrushSupport final
    {
    public:
        static constexpr size_t MAX_FREE_BRUSHES = 32;

        explicit BrushSupport(Renderer& renderer);

        /**
         * \brief Get a solid-color brush.
         * \param color The brush color.
         * \returns The brush. Needs to be returned.
         */
        Brush& GetSolidColorBrush(Color color);

        /**
         * \brief Return a solid color brush.
         * Do not use this method, instead use \c Brush::Return() .
         * \param index The index of the brush to return.
         */
        void ReturnSolidColorBrush(Brush::Index index);

        /**
         * \brief Access a raw (unwrapped) solid-color brush for direct color drawing commands.
         * \param color The color of the requested brush.
         * \returns The raw Direct2D brush, owned by the support.
         */
        ID2D1Brush* UseRawSolidColorBrush(Color color);

        /**
         * \brief Validate that all wrapped resources have been returned to this support class.
         * 
         * This uses the debug layer to create messages if resources have not been returned.
         * It is invalid to use wrapped resources managed by this support class after the class has been freed.
         */
        void ValidateAllWrappedResourcesAreReturned() const;

    private:
        Renderer& renderer;

        Bag<std::unique_ptr<Brush>, Brush::Index> brushes;

        std::vector<std::unique_ptr<Brush>> freeSolidColorBrushes;

        ComPtr<ID2D1SolidColorBrush> scratchSolidColorBrush;
    };
}
