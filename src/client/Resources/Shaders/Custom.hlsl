//  <copyright file="Custom.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#ifndef VG_SHADER_CUSTOM_HLSL
#define VG_SHADER_CUSTOM_HLSL

/**
 * \brief Defines the custom spatial data that is added to the space rendering pipeline.
 */
namespace vg
{
    /**
     * \brief The custom data.
     */
    struct Custom
    {
        /**
         * \brief Whether to render wireframes.
         */
        bool wireframe;

        /**
         * \brief The direction of the wind.
         */
        float3 windDir;

        /**
         * \brief The size of the part of the view plane that is inside a fog volume. Given in relative size, positive values start from the bottom, negative values from the top.
         */
        float fogOverlapSize;

        /**
         * \brief Color of the fog volume the view plane is currently in.
         */
        float3 fogOverlapColor;
    };

    ConstantBuffer<Custom> custom : register(b1);

    static float3 const SKY_COLOR = float3(0.5f, 0.8f, 0.9f);
}

#endif
