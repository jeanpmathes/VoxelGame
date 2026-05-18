// <copyright file="Brush.hpp" company="VoxelGame">
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

namespace ui
{
    class Renderer;

    /**
     * \brief A user-interface brush, wrapping a \c ID2D1Brush object.
     */
    class Brush final : public Object
    {
        DECLARE_OBJECT_SUBCLASS(Brush)

    public:
        /**
         * \brief An index into active brush storage.
         */
        enum class Index : size_t
        {
        };

        Brush(Renderer& renderer, Index index, ComPtr<ID2D1SolidColorBrush> brush);

        void Reset(Index newIndex, ComPtr<ID2D1SolidColorBrush> newBrush);
        void Return();
        void SetIndex(std::optional<Index> newIndex);

        [[nodiscard]] Renderer&            GetRenderer() const;
        [[nodiscard]] std::optional<Index> GetIndex() const;
        [[nodiscard]] ID2D1Brush*          GetWrapped() const;

    private:
        Renderer*            renderer;
        std::optional<Index> index;

        ComPtr<ID2D1SolidColorBrush> wrapped;
    };
}
