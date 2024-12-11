//  <copyright file="Draw2D.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#ifndef NATIVE_SHADER_DRAW_2D_HLSL
#define NATIVE_SHADER_DRAW_2D_HLSL

/**
 * \brief The bindings for the effect pipeline.
 */
namespace native
{
    namespace effect
    {
        /**
         * \brief The data for the effect pipeline.
         */
        struct EffectData
        {
            float4x4 mvp;
        };

        ConstantBuffer<EffectData> data : register(b1);
    }
}

#endif
