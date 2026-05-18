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
     * \brief Tracks the drawing state and associated stacks.
     */
    class State final
    {
    public:
        /**
         * \brief Add an offset to the current transform.
         * \param offset The offset to add to all later draw positions.
         */
        void PushOffset(PointF offset);

        /**
         * \brief Remove the most recently pushed offset and restore the previous transform.
         */
        void PopOffset();

        /**
         * \brief Intersect the current clip with another rectangle.
         * \param rectangle The clipping rectangle to intersect with the current clip.
         */
        void PushClip(RectangleF rectangle);

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

        [[nodiscard]] D2D1_MATRIX_3X2_F GetCurrentTransform() const;
        [[nodiscard]] RectangleF        GetCurrentClip() const;
        [[nodiscard]] bool              IsCurrentClipEmpty() const;
        [[nodiscard]] FLOAT             GetCurrentOpacity() const;

        /**
         * \brief Validate that the state is ready for rendering.
         */
        void Validate() const;

    private:
        std::vector<PointF>     offsetStack;
        std::vector<RectangleF> clipStack;
        std::vector<FLOAT>      opacityStack;

        PointF     currentOffset  = {0.0f, 0.0f};
        RectangleF currentClip    = {0.0f, 0.0f, 0.0f, 0.0f};
        FLOAT      currentOpacity = 1.0f;
    };
}
