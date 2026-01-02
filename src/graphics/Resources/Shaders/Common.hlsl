// <copyright file="Common.hlsl" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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

#ifndef NATIVE_SHADER_COMMON_HLSL
#define NATIVE_SHADER_COMMON_HLSL

#define POW2(x) ((x) * (x))
#define POW3(x) (POW2(x) * (x))
#define POW4(x) (POW2(x) * POW2(x))
#define POW5(x) (POW3(x) * POW2(x))

#define PI 3.14159265359f

/**
 * \brief Contains common functions and constants for shaders.
 */
namespace native
{
    /**
     * \brief Get the RGB color from the HSV hue.
     * \param h The hue.
     * \return The RGB color.
     */
    float3 HUEtoRGB(in float const h)
    {
        float r = abs(h * 6 - 3) - 1;
        float g = 2 - abs(h * 6 - 2);
        float b = 2 - abs(h * 6 - 4);
        return saturate(float3(r, g, b));
    }

    /**
     * \brief Get the luminance of a color.
     * \param color The color.
     * \return The luminance.
     */
    float GetLuminance(in float3 const color) { return dot(color, float3(0.2126729f, 0.7151522f, 0.0721750f)); }

    /**
     * \brief Translates the UV coordinates between the OpenGL and DirectX coordinate system.
     * \param uv The UV coordinates in the OpenGL/DirectX coordinate system.
     * \return The UV coordinates in the DirectX/OpenGL coordinate system.
     */
    float2 TranslateUV(in float2 const uv) { return float2(uv.x, 1.0f - uv.y); }
}

#endif
