//  <copyright file="Section.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#ifndef VG_SHADER_SECTION_HLSL
#define VG_SHADER_SECTION_HLSL

#include "Spatial.hlsl"
#include "Decoding.hlsl"
#include "TextureAnimation.hlsl"

/**
 * \brief Utilities providing operations for rendering sections.
 */
namespace vg
{
    namespace section
    {
        /**
         * \brief Calculate the final UV coordinates for a quad.
         * \param info Information about the quad.
         * \param useTextureRepetition Whether to use texture repetition.
         * \return The final UV coordinates for the quad.
         */
        float2 GetUV(const in spatial::Info info, const bool useTextureRepetition)
        {
            const float4x2 uvs = decode::GetUVs(info.data);

            const float2 uvX = uvs[info.indices.x];
            const float2 uvY = uvs[info.indices.y];
            const float2 uvZ = uvs[info.indices.z];

            float2 uv = uvX * info.barycentric.x + uvY * info.barycentric.y + uvZ * info.barycentric.z;

            if (decode::GetTextureRotationFlag(info.data))
                uv = spatial::RotateUV(uv);

            uv = native::TranslateUV(uv);

            if (useTextureRepetition)
                uv *= decode::GetTextureRepetition(info.data);

            return uv;
        }

        /**
         * \brief Get the final texture index for a quad.
         * \param info Information about the quad.
         * \param useTextureRepetition Whether to use texture repetition.
         * \param isBlock Whether the quad is part of a block or a fluid.
         * \return The final texture index for the quad.
         */
        int4 GetBaseColorIndex(const in spatial::Info info, const bool useTextureRepetition, const bool isBlock)
        {
            const float2 uv = GetUV(info, useTextureRepetition);
            uint textureIndex = animation::GetAnimatedTextureIndex(
                info.data, PrimitiveIndex() / 2, native::spatial::global.time, isBlock);

            const float2 ts = frac(uv) * float2(
                native::spatial::global.textureSize.x,
                native::spatial::global.textureSize.y);
            uint2 texel = uint2(ts.x, ts.y);
            const uint mip = 0;

            return int4(textureIndex, texel.x, texel.y, mip);
        }

        /**
         * \brief Get the base color (no shading) for a basic quad.
         * \param info Information about the quad.
         * \return The base color (no shading) for a basic quad.
         */
        float4 GetBasicBaseColor(const in spatial::Info info)
        {
            int4 index = GetBaseColorIndex(info, true, true);
            return native::rt::textureSlotOne[index.x].Load(index.yzw);
        }

        /**
         * \brief Get the base color (no shading) for a foliage quad.
         * \param info Information about the quad.
         * \return The base color (no shading) for a foliage quad.
         */
        float4 GetFoliageBaseColor(const in spatial::Info info)
        {
            int4 index = GetBaseColorIndex(info, false, true);
            return native::rt::textureSlotOne[index.x].Load(index.yzw);
        }

        /**
         * \brief Get the base color (no shading) for a fluid quad.
         * \param info Information about the quad.
         * \return The base color (no shading) for a fluid quad.
         */
        float4 GetFluidBaseColor(const in spatial::Info info)
        {
            int4 index = GetBaseColorIndex(info, true, false);
            return native::rt::textureSlotTwo[index.x].Load(index.yzw);
        }

#define SET_HIT_INFO(payload, info, shading) \
    { \
        payload.distance = RayTCurrent(); \
        payload.normal = info.normal; \
        payload.color = shading; \
        payload.alpha = 1.0f; \
    } (void)0
    }
}

#endif
