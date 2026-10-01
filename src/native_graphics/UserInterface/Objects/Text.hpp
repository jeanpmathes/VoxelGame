// <copyright file="Text.hpp" company="VoxelGame">
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

#include "Objects/Object.hpp"
#include "UserInterface/Definitions.hpp"

namespace ui
{
    class Renderer;
    class TextFormat;

    /**
     * \brief A user-interface text object, wrapping a measured \c IDWriteTextLayout.
     *
     * Text objects allow changing format and content.
     * They remember the last available size passed to \c Measure and use it for drawing.
     */
    class Text final : public Object
    {
        DECLARE_OBJECT_SUBCLASS(Text)

    public:
        /**
         * \brief An index into active text storage.
         */
        enum class Index : size_t
        {
        };

        explicit Text(Renderer& renderer);

        /**
         * Return this text to the renderer. Do not use after returning.
         */
        void Return();

        void Reset(Index newIndex, WCHAR const* newText, UINT newTextLength, TextFormatDescription const& newFormat);

        /**
         * \brief Set the text content.
         * \param newText The new text content.
         * \param newTextLength The length of the new text content.
         */
        void SetContent(WCHAR const* newText, UINT newTextLength);

        /**
         * \brief Set the text format.
         * \param newFormat The new text format description.
         */
        void SetFormat(TextFormatDescription const& newFormat);

        [[nodiscard]] Renderer&            GetRenderer() const;
        [[nodiscard]] std::optional<Index> GetIndex() const;
        [[nodiscard]] IDWriteTextLayout*   GetWrapped() const;

        /**
         * \brief Measure this text for the available size and prepare the layout for drawing.
         *
         * The available size is stored and used when drawn later as the maximum size.
         *
         * \param newAvailableSize The available size in UI coordinates.
         * \returns The measured size in UI coordinates.
         */
        [[nodiscard]] Size Measure(Size newAvailableSize);

    private:
        void InitializeLayout();

        Renderer*            renderer;
        std::optional<Index> index;

        ComPtr<IDWriteTextLayout> layout;

        TextFormat*  format = nullptr;
        std::wstring text;
        Size         availableSize = {0.0f, 0.0f};
    };
}
