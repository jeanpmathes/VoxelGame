//  <copyright file="Section.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#ifndef VG_SHADER_SECTION_HLSL
#define VG_SHADER_SECTION_HLSL

#include "CameraRT.hlsl"
#include "Common.hlsl"

#include "Decoding.hlsl"
#include "Spatial.hlsl"
#include "TextureAnimation.hlsl"

#define LOAD_SLOT_ONE(index) native::rt::textureSlotOne[index.w].Load(index.xyz)
#define LOAD_SLOT_TWO(index) native::rt::textureSlotTwo[index.w].Load(index.xyz)

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
        float GetConeWidth(float const path)
        {
            // See: Ray Tracing Gems, Chapter 20.6

            return (path + RayTCurrent()) * native::rt::camera.spread;
        }

        /**
         * \brief Calculate the (doubled) area of the triangle in world space.
         */
        float GetDoubleWorldAreaOfTriangle(in spatial::Info const info)
        {
            // See: Ray Tracing Gems, Chapter 20.2

            return length(cross(info.a - info.b, info.a - info.c));
        }

        /**
         * \brief Calculate the (doubled) area of the triangle in texture space.
         */
        float GetDoubleTexelAreaOfTriangle(in spatial::Info const info)
        {
            // See: Ray Tracing Gems, Chapter 20.2

            float4x2 const uvs        = decode::GetUVs(info.data);
            float2 const   repetition = decode::GetTextureRepetition(info.data);

            float2 uv0 = uvs[info.indices.x];
            float2 uv1 = uvs[info.indices.y];
            float2 uv2 = uvs[info.indices.z];

            if (decode::GetTextureRotationFlag(info.data))
            {
                uv0 = spatial::RotateUV(uv0);
                uv1 = spatial::RotateUV(uv1);
                uv2 = spatial::RotateUV(uv2);
            }

            uv0 *= repetition;
            uv1 *= repetition;
            uv2 *= repetition;

            // The wh factor is not used here as it comes in later when summing up the LOD.

            return abs((uv1.x - uv0.x) * (uv2.y - uv0.y) - (uv2.x - uv0.x) * (uv1.y - uv0.y));
        }

        /**
         * \brief Compute the level of detail to use for the texture at a hit.
         * \param path The length of rays up to the previous hit.
         * \param info Information about the hit.
         * \return The computed LOD.
         */
        float GetLOD(float const path, in spatial::Info const info)
        {
            // See: Ray Tracing Gems, Chapter 20.6

            float const width = GetConeWidth(path);
            float const world = GetDoubleWorldAreaOfTriangle(info);
            float const texel = GetDoubleTexelAreaOfTriangle(info);
            float       lod   = 0.5f * log2(texel / world);

            lod += log2(width);
            lod += 0.5f * log2(native::spatial::global.textureSize.x * native::spatial::global.textureSize.y);
            lod -= log2(abs(dot(WorldRayDirection(), info.normal)));

            return lod;
        }

        /**
         * \brief Calculate the final UV coordinates for a quad.
         * \param info Information about the quad.
         * \param useTextureRepetition Whether to use texture repetition.
         * \return The final UV coordinates for the quad.
         */
        float2 GetUV(in spatial::Info const info, bool const useTextureRepetition)
        {
            float4x2 const uvs = decode::GetUVs(info.data);

            float2 const uvX = uvs[info.indices.x];
            float2 const uvY = uvs[info.indices.y];
            float2 const uvZ = uvs[info.indices.z];

            float2 uv = uvX * info.barycentric.x + uvY * info.barycentric.y + uvZ * info.barycentric.z;

            if (decode::GetTextureRotationFlag(info.data)) uv = spatial::RotateUV(uv);

            uv = native::TranslateUV(uv);

            if (useTextureRepetition) uv *= decode::GetTextureRepetition(info.data);

            return uv;
        }

        /**
         * \brief Create a color encoding the mip level used for sampling.
         * \param mip The mip level that was sampled.
         * \return A color visualizing the mip level.
         */
        float3 GetMipColor(uint const mip)
        {
            switch (mip % 6)
            {
            case 0:
                return float3(1.0f, 0.0f, 0.0f);
            case 1:
                return float3(1.0f, 0.5f, 0.0f);
            case 2:
                return float3(1.0f, 1.0f, 0.0f);
            case 3:
                return float3(0.0f, 1.0f, 0.0f);
            case 4:
                return float3(0.0f, 0.5f, 1.0f);
            default:
                return float3(0.5f, 0.0f, 1.0f);
            }
        }

        /**
         * \brief Apply mip level visualization to a sampled color.
         * \param color The sampled color.
         * \param mip The mip level used to retrieve the color.
         * \return Either the original color or a visualization.
         */
        float4 ApplyMipVisualization(float4 const color, uint const mip)
        {
            float luminance  = native::GetLuminance(color.rgb);
            float brightness = lerp(0.2f, 1.0f, luminance);

            float3 const mipColor = GetMipColor(mip) * brightness;
            return float4(mipColor, color.a);
        }

        /**
         * \brief Sample the base color for a given texture index.
         * \param index The texture index to sample.
         * \param isBlock Whether the texture is part of a block or a fluid.
         * \return The sampled base color.
         */
        float4 SampleBaseColor(uint4 const index, bool const isBlock) { return isBlock ? LOAD_SLOT_ONE(index) : LOAD_SLOT_TWO(index); }

        /**
         * \brief Sample the base color for a hit against a quad.
         * \param path The length of rays up to the previous hit.
         * \param info Information about the quad.
         * \param useTextureRepetition Whether to use texture repetition.
         * \param isBlock Whether the quad is part of a block or a fluid.
         * \return The sampled base color for the quad.
         */
        float4 SampleBaseColor(float const path, in spatial::Info const info, bool const useTextureRepetition, bool const isBlock)
        {
            float2 const uv           = GetUV(info, useTextureRepetition);
            uint         textureIndex = animation::GetAnimatedTextureIndex(info.data, PrimitiveIndex() / 2, native::spatial::global.time, isBlock);

            uint3 size = native::spatial::global.textureSize.xyz;
            
            float4 color;
            uint mip;

            if (path >= 0.0f)
            {
                uint const maxMip = size.z - 1;

                float lod = GetLOD(path, info);
                lod       = clamp(lod, 0.0f, maxMip);

                float const interpolation = frac(lod);

                uint const mipLow  = uint(floor(lod));
                uint const mipHigh = min(mipLow + 1, maxMip);

                uint2 const  sizeLow  = uint2(max(1, size.x >> mipLow), max(1, size.y >> mipLow));
                float2 const tsLow    = frac(uv) * float2(sizeLow);
                uint2 const  texelLow = uint2(tsLow.x, tsLow.y);

                color = SampleBaseColor(int4(texelLow.x, texelLow.y, mipLow, textureIndex), isBlock);
                mip = mipLow;
                
                if (mipHigh != mipLow)
                {
                    uint2 const  sizeHigh   = uint2(max(1, size.x >> mipHigh), max(1, size.y >> mipHigh));
                    float2 const tsHigh     = frac(uv) * float2(sizeHigh);
                    uint2 const  texelHigh  = uint2(tsHigh.x, tsHigh.y);
                    
                    float4 const otherColor = SampleBaseColor(int4(texelHigh.x, texelHigh.y, mipHigh, textureIndex), isBlock);
                    
                    color = lerp(color, otherColor, interpolation);
                }
            }
            else
            {
                float2 const ts    = frac(uv) * float2(size.xy);
                uint2        texel = uint2(ts.x, ts.y);

                color = SampleBaseColor(int4(texel.x, texel.y, 0, textureIndex), isBlock);
                mip = 0;
            }
            
            if (custom.showLevelOfDetail) 
                color = ApplyMipVisualization(color, mip);

            return color;
        }

        /**
         * \brief Get the base color (no shading) for a basic quad.
         * \param path The length of rays up to the previous hit.
         * \param info Information about the quad.
         * \return The base color (no shading) for a basic quad.
         */
        float4 GetBasicBaseColor(float const path, in spatial::Info const info) { return SampleBaseColor(path, info, true, true); }

        /**
         * \brief Get the base color (no shading) for a foliage quad.
         * \param path The length of rays up to the previous hit.
         * \param info Information about the quad.
         * \return The base color (no shading) for a foliage quad.
         */
        float4 GetFoliageBaseColor(float const path, in spatial::Info const info) { return SampleBaseColor(path, info, false, true); }

        /**
         * \brief Get the base color (no shading) for a fluid quad.
         * \param path The length of rays up to the previous hit.
         * \param info Information about the quad.
         * \return The base color (no shading) for a fluid quad.
         */
        float4 GetFluidBaseColor(float const path, in spatial::Info const info) { return SampleBaseColor(path, info, true, false); }

        /**
         * \brief Get the dominant color for a fluid quad.
         * \param info Information about the quad.
         * \return The dominant color of the quad.
         */
        float4 GetFluidDominantColor(in spatial::Info const info)
        {
            int4 const index = int4(
                0,
                0,
                // Only one texel in highest mip level.
                native::spatial::global.textureSize.z - 1,
                // Index of the highest mip level.
                decode::GetTextureIndex(info.data));
            return LOAD_SLOT_TWO(index);
        }

        /**
         * \brief Path length to use for shadow rays.
         */
        static float const SHADOW_PATH = -1.0f;
    }
}

#undef LOAD_SLOT_ONE
#undef LOAD_SLOT_TWO

#endif
