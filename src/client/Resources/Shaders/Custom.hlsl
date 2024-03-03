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
    };

    ConstantBuffer<Custom> custom : register(b1);
}

#endif
