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
        static constexpr size_t MAX_FREE_TEXT_FORMATS = 32;

        explicit TextFormatSupport(Renderer& renderer);

        /**
         * \brief Create or reuse a wrapped DirectWrite text format.
         * \param description The description used to create the DirectWrite text format.
         * \returns The active text format inserted into format storage.
         */
        TextFormat& Get(TextFormatDescription description);

        void Return(TextFormat& format);

    private:
        Renderer& renderer;

        Bag<std::unique_ptr<TextFormat>, TextFormat::Index> formats;

        std::vector<std::unique_ptr<TextFormat>> freeFormats;
    };
}
