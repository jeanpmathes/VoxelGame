// <copyright file="State.hpp" company="VoxelGame">
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
    /**
     * \brief Tracks the drawing state and associated stacks for a renderer and applies them to the drawing context.
     */
    class State final
    {
    public:
        explicit State(Renderer& renderer);

        /**
         * \brief Add an offset to the current transform.
         * \param offset The offset to add to all later draw positions.
         */
        void PushOffset(Point offset);

        /**
         * \brief Remove the most recently pushed offset and restore the previous transform.
         */
        void PopOffset();

        /**
         * \brief Intersect the current clip with another rectangle.
         * \param rectangle The clipping rectangle to intersect with the current clip.
         */
        void PushClip(Rectangle rectangle);

        /**
         * \brief Pop the current clipping rectangle and restore the previous clip.
         */
        void PopClip();

        /**
         * \brief Push a new opacity which is multiplied with the current opacity.
         * \param opacity The opacity multiplier to apply to later draw commands.
         */
        void PushOpacity(FLOAT opacity);

        /**
         * \brief Pop the current opacity and restore the previous opacity.
         */
        void PopOpacity();

        /**
         * \brief Validate that the state is ready for rendering.
         */
        void Validate() const;

    private:
        Renderer& renderer;

        std::vector<Point> offsetStack;
        size_t             clipStackCounter    = 0;
        size_t             opacityStackCounter = 0;

        [[nodiscard]] D2D1_MATRIX_3X2_F GetCurrentTransform() const;
    };
}
