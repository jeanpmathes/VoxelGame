// <copyright file="TextSupport.hpp" company="VoxelGame">
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

#include "UserInterface/Objects/Text.hpp"

namespace ui
{
    class Context;
    class Renderer;
    class TextFormat;

    /**
     * \brief Owns text objects of a UI renderer.
     */
    class TextSupport final
    {
    public:
        explicit TextSupport(Renderer& renderer);

        /**
         * \brief Create a text object.
         * \param text The text content used to create the object.
         * \param format The text format used to create the object.
         * \returns The active text object.
         */
        Text& Get(WCHAR const* text, TextFormat& format);

        void Return(Text& text);

    private:
        Renderer& renderer;

        Bag<std::unique_ptr<Text>, Text::Index> texts;
    };
}
