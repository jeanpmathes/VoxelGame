// <copyright file="TextFormatSupport.hpp" company="VoxelGame">
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

#include "UserInterface/Objects/TextFormat.hpp"

namespace ui
{
    class Context;
    class Renderer;

    /**
     * \brief Creates and owns UI text formats for one UI renderer, as well as performing pooling.
     */
    class TextFormatSupport final
    {
    public:
        explicit TextFormatSupport(Renderer& renderer);

        /**
         * \brief Get a wrapped text format.
         * \param description The description used to create the text format.
         * \returns The text format. Needs to be returned.
         */
        TextFormat& GetTextFormat(TextFormatDescription const& description);

        /**
         * \brief Return a text format.
         * Do not use this method, instead use \c TextFormat::Return() .
         * \param index The index of the text format to return.
         */
        void ReturnTextFormat(TextFormat::Index index);

        /**
         * \brief Validate that all wrapped resources have been returned to this support class.
         * 
         * This uses the debug layer to create messages if resources have not been returned.
         * It is invalid to use wrapped resources managed by this support class after the class has been freed.
         */
        void ValidateAllWrappedResourcesAreReturned() const;

    private:
        Renderer& renderer;

        Bag<std::unique_ptr<TextFormat>, TextFormat::Index> textFormats;
    };
}
