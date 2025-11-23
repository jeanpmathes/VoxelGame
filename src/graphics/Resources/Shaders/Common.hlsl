//  <copyright file="Common.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#ifndef NATIVE_SHADER_COMMON_HLSL
#define NATIVE_SHADER_COMMON_HLSL

#define POW2(x) ((x) * (x))
#define POW3(x) (POW2(x) * (x))
#define POW4(x) (POW2(x) * POW2(x))
#define POW5(x) (POW3(x) * POW2(x))

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
    float GetLuminance(in float3 const color) { return dot(color, float3(0.2126729f,  0.7151522f, 0.0721750f)); }

    /**
     * \brief Translates the UV coordinates between the OpenGL and DirectX coordinate system.
     * \param uv The UV coordinates in the OpenGL/DirectX coordinate system.
     * \return The UV coordinates in the DirectX/OpenGL coordinate system.
     */
    float2 TranslateUV(in float2 const uv) { return float2(uv.x, 1.0f - uv.y); }

    /**
     * \brief The color red.
     */
    static float3 const RED = float3(1.0f, 0.0f, 0.0f);

    /**
     * \brief The color green.
     */
    static float3 const GREEN = float3(0.0f, 1.0f, 0.0f);

    /**
     * \brief The color blue.
     */
    static float3 const BLUE = float3(0.0f, 0.0f, 1.0f);
}

#endif
