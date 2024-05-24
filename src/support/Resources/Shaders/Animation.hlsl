// <copyright file="Animation.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#ifndef NATIVE_SHADER_ANIMATION_HLSL
#define NATIVE_SHADER_ANIMATION_HLSL

#include "Space.hlsl"

/*
 * This contains the required structures and bindings for the animation system.
 */
namespace native
{
    namespace animation
    {
        /**
         * \brief The smallest unit in which work-loads are passed to the GPU.
         */
        struct Submission
        {
            uint index;
            uint instance;
            
            uint offset;
            uint count;
        };

        /**
         * \brief All submissions for a single thread group.
         */
        struct ThreadGroup
        {
            Submission submissions[16];
        };

        /**
         * \brief Contains the work description data for the thread groups.
         */
        StructuredBuffer<ThreadGroup> threadGroupData : register(t0, space3);

        /**
         * \brief The source vertex data buffer. This data is read and transformed by the animation shader.
         */
        StructuredBuffer<spatial::SpatialVertex> source[] : register(t1, space3);

        /**
         * \brief The destination vertex data buffer. This data is written to by the animation shader.
         */
        RWStructuredBuffer<spatial::SpatialVertex> destination[] : register(u0, space3);
    }
}

#endif
