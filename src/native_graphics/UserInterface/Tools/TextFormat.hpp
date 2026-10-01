// <copyright file="TextFormat.hpp" company="VoxelGame">
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
    class Renderer;

    /**
     * \brief A user-interface text format, wrapping a \c IDWriteTextFormat object.
     */
    class TextFormat final
    {
    public:
        explicit TextFormat(Renderer& renderer);

        void Reset(TextFormatDescription const& newDescription);

        [[nodiscard]] Renderer&          GetRenderer() const;
        [[nodiscard]] IDWriteTextFormat* GetWrapped() const;

    private:
        Renderer* renderer;

        ComPtr<IDWriteTextFormat>   wrapped;
        ComPtr<IDWriteInlineObject> trimmingSign;
    };
}
