// <copyright file="Animation.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#ifndef NATIVE_SHADER_ANIMATION_HLSL
#define NATIVE_SHADER_ANIMATION_HLSL

#include "Space.hlsl"

#define SPACE space3

/*
 * This contains the required structures and bindings for the animation system.
 */
namespace native
{
    namespace animation
    {
        struct WorkInfo
        {
            uint value;
        };

        /**
         * \brief Index of the work load.
         */
        ConstantBuffer<WorkInfo> index : register(b0, space1);

        /**
         * \brief Size of the work load.
         */
        ConstantBuffer<WorkInfo> size : register(b1, space1);

        /**
         * \brief The source vertex data buffer. This data is read and transformed by the animation shader.
         */
        StructuredBuffer<spatial::SpatialVertex> source[] : register(t0, SPACE);

        /**
         * \brief The destination vertex data buffer. This data is written to by the animation shader.
         */
        RWStructuredBuffer<spatial::SpatialVertex> destination[] : register(u0, SPACE);
    }
}

#endif
