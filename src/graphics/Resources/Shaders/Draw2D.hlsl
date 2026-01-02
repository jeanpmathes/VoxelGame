// <copyright file="Draw2D.hlsl" company="VoxelGame">
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

#ifndef NATIVE_SHADER_DRAW_2D_HLSL
#define NATIVE_SHADER_DRAW_2D_HLSL

/**
 * \brief The bindings for the Draw2D pipeline.
 */
namespace native
{
    namespace draw2d
    {
        /**
         * \brief Whether to use the texture or not.
         */
        struct UseTexture
        {
            bool value;
        };

        ConstantBuffer<UseTexture> useTexture : register(b1);

        /**
         * \brief The current render time.
         */
        struct Time
        {
            float value;
        };

        ConstantBuffer<Time> time : register(b0, space1);

        Texture2D    texture[] : register(t0);
        SamplerState sampler : register(s0);
    }
}

#endif
