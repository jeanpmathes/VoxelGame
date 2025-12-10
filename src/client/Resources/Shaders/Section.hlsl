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
         * \brief Compute anisotropic ellipse axes and texture gradients to be used for anisotropic texture filtering.
         * 
         * The function is based on the method described in
         * "Improved Shader and Texture Level of Detail Using Ray Cones" (Journal of Computer Graphics Techniques, 2021)
         * by T. Akenine-Möller, C. Crassin, J. Boksansky, L. Belcour, A. Panteleev, and O. Wright
         */
        void ComputeAnisotropicEllipseAxes(
            in float3  intersectionPoint,
            in float3  normal,
            in float3  direction,
            in float   rayConeRadiusAtIntersection,
            in float3  positions[3],
            in float2  textureCoordinates[3],
            in float2  interpolatedTextureCoordinatesAtIntersection,
            out float2 ddx,
            out float2 ddy)
        {
            // Compute the two ellipse axes a1 and a2.

            float3       a1 = direction - dot(normal, direction) * normal;
            float3 const p1 = a1 - dot(direction, a1) * direction;
            a1              *= rayConeRadiusAtIntersection / max(0.0001, length(p1));

            float3       a2 = cross(normal, a1);
            float3 const p2 = a2 - dot(direction, a2) * direction;
            a2              *= rayConeRadiusAtIntersection / max(0.0001, length(p2));

            float3       eP, delta           = intersectionPoint - positions[0];
            float3 const e1                  = positions[1] - positions[0];
            float3 const e2                  = positions[2] - positions[0];
            float const  oneOverAreaTriangle = 1.0 / dot(normal, cross(e1, e2));

            // Compute the two texture gradients ddx and ddy.

            eP = delta + a1;
            float const u1 = dot(normal, cross(eP, e2)) * oneOverAreaTriangle;
            float const v1 = dot(normal, cross(e1, eP)) * oneOverAreaTriangle;
            ddx = (1.0 - u1 - v1) * textureCoordinates[0] + u1 * textureCoordinates[1] + v1 * textureCoordinates[2] -
                interpolatedTextureCoordinatesAtIntersection;

            eP = delta + a2;
            float const u2 = dot(normal, cross(eP, e2)) * oneOverAreaTriangle;
            float const v2 = dot(normal, cross(e1, eP)) * oneOverAreaTriangle;
            ddy = (1.0 - u2 - v2) * textureCoordinates[0] + u2 * textureCoordinates[1] + v2 * textureCoordinates[2] -
                interpolatedTextureCoordinatesAtIntersection;
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
         * \brief Sample the base color for a given texture index.
         * \param index The texture index to sample.
         * \param isBlock Whether the texture is part of a block or a fluid.
         * \return The sampled base color.
         */
        float4 SampleBaseColor(uint4 const index, bool const isBlock)
        {
            if (isBlock) return native::rt::textureSlotOne[index.w].Load(index.xyz);
            else return native::rt::textureSlotTwo[index.w].Load(index.xyz);
        }

        /**
         * \brief Sample the base color for a hit against a quad.
         * \param path The length of rays up to the previous hit.
         * \param info Information about the quad.
         * \param useTextureRepetition Whether to use texture repetition.
         * \param isBlock Whether the quad is part of a block or a fluid.
         * \return The sampled base color for the quad.
         */
        float4 SampleBaseColor(
            float const            path,
            in spatial::Info const info,
            bool const             useTextureRepetition,
            bool const             isBlock)
        {
            float2 uv           = GetUV(info, useTextureRepetition);
            uint   textureIndex = animation::GetAnimatedTextureIndex(
                info.data,
                PrimitiveIndex() / 2,
                native::spatial::global.time,
                isBlock);

            // todo: optimize, as there is a lot of repeated calculation of these values, and aling storage format between ellipse function and this stuff

            float3 intersectionPoint = WorldRayOrigin() + RayTCurrent() * WorldRayDirection();
            // todo: compare with interpolated position from barycentric coords
            float3 normal                      = info.normal;
            float3 direction                   = WorldRayDirection();
            float  rayConeRadiusAtIntersection = GetConeWidth(path) / 2.0f;
            // todo: check if /2 is correct here, maybe put it into spread factor

            float3 positions[3];
            positions[0] = info.a;
            positions[1] = info.b;
            positions[2] = info.c;

            float2         textureCoordinates[3]; // todo: the texture coordinates calculation is very ugly right now
            float4x2 const uvs    = decode::GetUVs(info.data); // todo: align storage format
            textureCoordinates[0] = uvs[info.indices.x];
            textureCoordinates[1] = uvs[info.indices.y];
            textureCoordinates[2] = uvs[info.indices.z];

            if (decode::GetTextureRotationFlag(info.data))
            {
                textureCoordinates[0] = spatial::RotateUV(textureCoordinates[0]);
                textureCoordinates[1] = spatial::RotateUV(textureCoordinates[1]);
                textureCoordinates[2] = spatial::RotateUV(textureCoordinates[2]);
            }

            textureCoordinates[0] = native::TranslateUV(textureCoordinates[0]);
            textureCoordinates[1] = native::TranslateUV(textureCoordinates[1]);
            textureCoordinates[2] = native::TranslateUV(textureCoordinates[2]);

            if (useTextureRepetition)
            {
                float2 repetition     = decode::GetTextureRepetition(info.data);
                textureCoordinates[0] *= repetition;
                textureCoordinates[1] *= repetition;
                textureCoordinates[2] *= repetition;
            }

            float2 interpolatedTextureCoordinatesAtIntersection = uv;

            float2 ddx, ddy;
            ComputeAnisotropicEllipseAxes(
                intersectionPoint,
                normal,
                direction,
                rayConeRadiusAtIntersection,
                positions,
                textureCoordinates,
                interpolatedTextureCoordinatesAtIntersection,
                ddx,
                ddy);

            uint textureSize = 32; // todo: set through constant, in same area as texture slots

            // Because anisotropic filter implies linear filtering in DirectX, we need to center UVs manually.

            uv *= native::spatial::global.textureSize.xy;
            uv = floor(uv) + 0.5f;
            uv /= native::spatial::global.textureSize.xy;

            float4 color;

            if (isBlock)
            {
                color = native::rt::textureSlotOne[textureIndex].SampleGrad(native::spatial::sampler, uv, ddx, ddy);
            }
            else
            {
                color = native::rt::textureSlotTwo[textureIndex].SampleGrad(native::spatial::sampler, uv, ddx, ddy);
            }

            return color;
        }

        /**
         * \brief Get the base color (no shading) for a basic quad.
         * \param path The length of rays up to the previous hit.
         * \param info Information about the quad.
         * \return The base color (no shading) for a basic quad.
         */
        float4 GetBasicBaseColor(float const path, in spatial::Info const info)
        {
            return SampleBaseColor(path, info, true, true);
        }

        /**
         * \brief Get the base color (no shading) for a foliage quad.
         * \param path The length of rays up to the previous hit.
         * \param info Information about the quad.
         * \return The base color (no shading) for a foliage quad.
         */
        float4 GetFoliageBaseColor(float const path, in spatial::Info const info)
        {
            return SampleBaseColor(path, info, false, true);
        }

        /**
         * \brief Get the base color (no shading) for a fluid quad.
         * \param path The length of rays up to the previous hit.
         * \param info Information about the quad.
         * \return The base color (no shading) for a fluid quad.
         */
        float4 GetFluidBaseColor(float const path, in spatial::Info const info)
        {
            return SampleBaseColor(path, info, true, false);
        }

        /**
         * \brief Get the dominant color for a fluid quad.
         * \param info Information about the quad.
         * \return The dominant color of the quad.
         */
        float4 GetFluidDominantColor(in spatial::Info const info)
        {
            return native::rt::textureSlotTwo[decode::GetTextureIndex(info.data)].Load(
                int3(
                    0,
                    0,
                    // Only one texel in highest mip level.
                    native::spatial::global.textureSize.z - 1 // Index of the highest mip level.
                ));
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
