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
         * \brief Get a text object.
         * \param textContent The text content used to create the object.
         * \param textLength The length of the text content.
         * \param format The text format used to create the object.
         * \returns The text object. Needs to be returned.
         */
        Text& GetText(WCHAR const* textContent, UINT textLength, TextFormat& format);

        /**
         * \brief Return a text.
         * Do not use this method, instead use \c Text::Return() .
         * \param index The index of the text to return.
         */
        void ReturnText(Text::Index index);

        /**
         * \brief Validate that all wrapped resources have been returned to this support class.
         * 
         * This uses the debug layer to create messages if resources have not been returned.
         * It is invalid to use wrapped resources managed by this support class after the class has been freed.
         */
        void ValidateAllWrappedResourcesAreReturned() const;

    private:
        Renderer& renderer;

        Bag<std::unique_ptr<Text>, Text::Index> texts;
    };
}
