// <copyright file="Renderer.hpp" company="VoxelGame">
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
#include "UserInterface/Commands.hpp"

#include "UserInterface/Tools/BrushSupport.hpp"
#include "UserInterface/Tools/StrokeSupport.hpp"
#include "UserInterface/Tools/TextFormatSupport.hpp"
#include "UserInterface/Tools/TextSupport.hpp"

namespace ui
{
    /**
     * \brief Renderer for one managed user-interface layer.
     *
     * Multiple renderers are ordered by their priority in \c NativeClient, allowing later UI layers to appear on top of earlier layers.
     */
    class Renderer final : public Object
    {
        DECLARE_OBJECT_SUBCLASS(Renderer)

    public:
        Renderer(NativeClient& client, INT priority);

        [[nodiscard]] INT GetPriority() const;

        [[nodiscard]] BrushSupport&      GetBrushSupport();
        [[nodiscard]] TextFormatSupport& GetTextFormatSupport();
        [[nodiscard]] TextSupport&       GetTextSupport();
        [[nodiscard]] StrokeSupport&     GetStrokeSupport();

        /**
         * \brief Replace the current command list with new commands.
         *
         * The command array is copied immediately.
         * Rendering always uses the most recently submitted list.
         *
         * \param newCommands The pointer to the array of commands to copy.
         * \param newCommandCount The number of commands to copy.
         */
        void SubmitCommands(Command const* newCommands, UINT newCommandCount);

        /**
         * \brief Execute the latest submitted commands.
         * \param frameIndex The frame to render into.
         */
        void Render(UINT frameIndex);

        void Free();

    private:
        INT                  priority;
        std::vector<Command> commands;

        BrushSupport      brushSupport;
        TextFormatSupport textFormatSupport;
        TextSupport       textSupport;
        StrokeSupport     strokeSupport;
    };
}
