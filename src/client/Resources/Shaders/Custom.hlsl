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
     * \brief The antialiasing data. The renderer uses adaptive supersampling.
     */
    struct AntiAliasing
    {
        /**
         * \brief Whether antialiasing is enabled. If false, the sample counts and threshold are ignored.
         */
        bool isEnabled;

        /**
         * \brief Whether to visualize the sampling rate (for debugging purposes).
         */
        bool showSamplingRate;

        /**
         * \brief The size of the sampling grid applied to each pixel.
         */
        uint minimumSamplingGridSize;

        /**
         * \brief The maximum size of the sampling grid, will only be used if the variance threshold is exceeded.
         */
        uint maximumSamplingGridSize;

        /**
         * \brief The variance threshold. If the variance of the samples (their luminance) exceeds this threshold, more samples are taken.
         */
        float varianceThreshold;

        /**
         * \brief The depth threshold. If the depth difference of the samples exceeds this threshold, more samples are taken.
         */
        float depthThreshold;
    };

    /**
     * \brief The custom data.
     */
    struct Custom
    {
        /**
         * \brief Whether to render wireframes.
         */
        bool showWireframes;

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

        /**
         * \brief The sky color.
         */
        float3 skyColor;

        /**
         * \brief The color of the air fog, used when not in a fog volume for the far distance.
         */
        float3 airFogColor;

        /**
         * \brief The density of the air fog, used when not in a fog volume for the far distance.
         */
        float airFogDensity;

        /**
         * \brief The antialiasing settings.
         */
        AntiAliasing antiAliasing;
    };

    ConstantBuffer<Custom> custom : register(b1);
}

#endif
