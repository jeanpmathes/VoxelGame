//  <copyright file="Section.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#ifndef VG_SHADER_SECTION_HLSL
#define VG_SHADER_SECTION_HLSL

#include "CameraRT.hlsl"

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
         * \brief Get the estimated width of the ray cone.
         * \param path The length of rays up to the previous hit.
         * \return The estimated cone width.
         */
        float GetConeWidth(const float path)
        {
            // See: Ray Tracing Gems, Chapter 20.6

            return (path + RayTCurrent()) * native::rt::camera.spread;
        }

        /**
         * \brief Calculate the (doubled) area of the triangle in world space.
         */
        float GetWorldAreaOfTriangle(const in spatial::Info info)
        {
            // See: Ray Tracing Gems, Chapter 20.6

            return length(cross(info.a - info.b, info.a - info.c));
        }

        /**
         * \brief Calculate the (doubled) area of the triangle in texture space.
         */
        float GetTexelAreaOfTriangle(const in spatial::Info info)
        {
            // See: Ray Tracing Gems, Chapter 20.6

            const float4x2 uvs = decode::GetUVs(info.data);

            const float2 uv0 = uvs[info.indices.x];
            const float2 uv1 = uvs[info.indices.y];
            const float2 uv2 = uvs[info.indices.z];

            const float textureSize = native::spatial::global.textureSize.x * native::spatial::global.textureSize.y;
            return textureSize * abs((uv1.x - uv0.x) * (uv2.y - uv0.y) - (uv2.x - uv0.x) * (uv1.y - uv0.y));
        }

        /**
         * \brief Compute the level of detail to use for the texture at a hit.
         * \param path The length of rays up to the previous hit.
         * \param info Information about the hit.
         * \return The computed LOD.
         */
        float GetLOD(const float path, const in spatial::Info info)
        {
            // See: Ray Tracing Gems, Chapter 20.6

            const float width = GetConeWidth(path);
            const float textureSize = native::spatial::global.textureSize.x * native::spatial::global.textureSize.y;

            const float world = GetWorldAreaOfTriangle(info);
            const float texel = GetTexelAreaOfTriangle(info);
            float lod = 0.5f * log2(world / texel);

            lod += log2(width);
            lod += 0.5f * log2(textureSize);

            lod -= log2(abs(dot(WorldRayDirection(), info.normal)));

            return lod;
        }
        
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
         * \param path The length of rays up to the previous hit.
         * \param info Information about the quad.
         * \param useTextureRepetition Whether to use texture repetition.
         * \param isBlock Whether the quad is part of a block or a fluid.
         * \return The final texture index for the quad.
         */
        int4 GetBaseColorIndex(
            const float path,
            const in spatial::Info info,
            const bool useTextureRepetition,
            const bool isBlock)
        {
            const float2 uv = GetUV(info, useTextureRepetition);
            uint textureIndex = animation::GetAnimatedTextureIndex(
                info.data, PrimitiveIndex() / 2, native::spatial::global.time, isBlock);

            uint mip = 0;
            uint2 size = native::spatial::global.textureSize.xy;

            if (path >= 0.0f)
            {
                const float lod = GetLOD(path, info);
                const uint maxMip = native::spatial::global.textureSize.z - 1;

                mip = clamp(uint(lod), 0, maxMip);
                size = uint2(
                    max(1, size.x >> mip),
                    max(1, size.y >> mip));
            }

            const float2 ts = frac(uv) * float2(size);
            uint2 texel = uint2(ts.x, ts.y);

            return int4(texel.x, texel.y, mip, textureIndex);
        }

        /**
         * \brief Get the base color (no shading) for a basic quad.
         * \param path The length of rays up to the previous hit.
         * \param info Information about the quad.
         * \return The base color (no shading) for a basic quad.
         */
        float4 GetBasicBaseColor(
            const float path,
            const in spatial::Info info)
        {
            int4 index = GetBaseColorIndex(path, info, true, true);
            return native::rt::textureSlotOne[index.w].Load(index.xyz);
        }

        /**
         * \brief Get the base color (no shading) for a foliage quad.
         * \param path The length of rays up to the previous hit.
         * \param info Information about the quad.
         * \return The base color (no shading) for a foliage quad.
         */
        float4 GetFoliageBaseColor(
            const float path,
            const in spatial::Info info)
        {
            int4 index = GetBaseColorIndex(path, info, false, true);
            return native::rt::textureSlotOne[index.w].Load(index.xyz);
        }

        /**
         * \brief Get the base color (no shading) for a fluid quad.
         * \param path The length of rays up to the previous hit.
         * \param info Information about the quad.
         * \return The base color (no shading) for a fluid quad.
         */
        float4 GetFluidBaseColor(
            const float path,
            const in spatial::Info info)
        {
            int4 index = GetBaseColorIndex(path, info, true, false);
            return native::rt::textureSlotTwo[index.w].Load(index.xyz);
        }

#define GET_PATH payload.alpha
#define GET_SHADOW_PATH -1.0f

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
