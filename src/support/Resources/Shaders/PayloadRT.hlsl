//  <copyright file="PayloadRT.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

// ReSharper disable CppRedundantEmptyStatement
// ReSharper disable CppInconsistentNaming
// @formatter:off

#ifndef NATIVE_SHADER_PAYLOAD_RT_HLSL
#define NATIVE_SHADER_PAYLOAD_RT_HLSL

/**
 * \brief The payloads passed along with the rays.
 */
namespace native
{
    namespace rt
    {
        /**
         * \brief The payload passed along with the standard rays.
         */
        struct [raypayload] HitInfo
        {
            uint2 color : read(caller, anyhit, closesthit, miss) : write(caller, anyhit, closesthit, miss);
            float2 normal : read(caller, anyhit, closesthit, miss) : write(caller, closesthit, miss);
            float3 position : read(caller, anyhit, closesthit, miss) : write(caller, anyhit, closesthit, miss);
            uint1 data : read(caller) : write(caller);
        };

        /**
         * \brief The payload passed along with the shadow rays.
         */
        struct [raypayload] ShadowHitInfo
        {
            bool isHit : read(caller) : write(caller, closesthit, miss);
        };
    }
}

// @formatter:on

#endif
