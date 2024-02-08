//  <copyright file="Draw2D.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
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
