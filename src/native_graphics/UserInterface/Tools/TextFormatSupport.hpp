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

#include <unordered_map>
#include "UserInterface/Tools/TextFormat.hpp"

namespace ui
{
    class Context;
    class Renderer;

    /**
     * \brief Creates and caches UI text formats for one UI renderer.
     */
    class TextFormatSupport final
    {
    public:
        explicit TextFormatSupport(Renderer& renderer);

        /**
         * \brief Get a cached text format, creating it if it does not already exist.
         * \param description The description used to create or look up the text format.
         * \returns The text format.
         */
        TextFormat& GetTextFormat(TextFormatDescription const& description);

    private:
        Renderer& renderer;

        std::set<std::wstring>                                                                            fontFamilies;
        std::unordered_map<TextFormatDescription, std::unique_ptr<TextFormat>, TextFormatDescriptionHash> formats;
    };
}
