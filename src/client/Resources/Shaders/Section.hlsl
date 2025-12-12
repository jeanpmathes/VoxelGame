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
            in float3   intersection,
            in float3   normal,
            in float3   direction,
            in float    coneWidth,
            in float3   position0,
            in float3   position1,
            in float3   position2,
            in float3x2 uvs,
            in float2   uv,
            out float2  ddx,
            out float2  ddy)
        {
            // Compute the two ellipse axes a1 and a2.

            float3       a1 = direction - dot(normal, direction) * normal;
            float3 const p1 = a1 - dot(direction, a1) * direction;
            a1              *= coneWidth / max(0.0001, length(p1));

            float3       a2 = cross(normal, a1);
            float3 const p2 = a2 - dot(direction, a2) * direction;
            a2              *= coneWidth / max(0.0001, length(p2));

            float3       eP, delta           = intersection - position0;
            float3 const e1                  = position1 - position0;
            float3 const e2                  = position2 - position0;
            float const  oneOverAreaTriangle = 1.0 / dot(normal, cross(e1, e2));

            // Compute the two texture gradients ddx and ddy.

            eP             = delta + a1;
            float const u1 = dot(normal, cross(eP, e2)) * oneOverAreaTriangle;
            float const v1 = dot(normal, cross(e1, eP)) * oneOverAreaTriangle;
            ddx            = (1.0 - u1 - v1) * uvs[0] + u1 * uvs[1] + v1 * uvs[2] - uv;

            eP             = delta + a2;
            float const u2 = dot(normal, cross(eP, e2)) * oneOverAreaTriangle;
            float const v2 = dot(normal, cross(e1, eP)) * oneOverAreaTriangle;
            ddy            = (1.0 - u2 - v2) * uvs[0] + u2 * uvs[1] + v2 * uvs[2] - uv;
        }

        /**
         * \brief Calculate the final UV coordinates for a quad.
         * \param info Information about the quad.
         * \param triangleUVs Output UV coordinates for the triangle vertices.
         * \param useTextureRepetition Whether to use texture repetition.
         * \return The final UV coordinates for the quad.
         */
        float2 GetUV(in spatial::Info const info, out float3x2 triangleUVs, bool const useTextureRepetition)
        {
            float4x2 const quadUVs = decode::GetUVs(info.data);

            for (int index = 0; index < 3; index++) triangleUVs[index] = quadUVs[info.indices[index]];

            if (decode::GetTextureRotationFlag(info.data)) for (int index = 0; index < 3; index++) triangleUVs[index] =
                spatial::RotateUV(triangleUVs[index]);

            for (int index = 0; index < 3; index++) triangleUVs[index] = native::TranslateUV(triangleUVs[index]);

            if (useTextureRepetition)
            {
                float2 repetition = decode::GetTextureRepetition(info.data);

                for (int index = 0; index < 3; index++) triangleUVs[index] *= repetition;
            }

            return triangleUVs[0] * info.barycentric.x + triangleUVs[1] * info.barycentric.y + triangleUVs[2] * info.
                barycentric.z;
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
            float3x2 uvs;
            float2   uv           = GetUV(info, uvs, useTextureRepetition);
            uint     textureIndex = animation::GetAnimatedTextureIndex(
                info.data,
                PrimitiveIndex() / 2,
                native::spatial::global.time,
                isBlock);

            float2 ddx, ddy;
            ComputeAnisotropicEllipseAxes(
                info.GetPosition(),
                info.normal,
                WorldRayDirection(),
                GetConeWidth(path),
                info.a,
                info.b,
                info.c,
                uvs,
                uv,
                ddx,
                ddy);

            // Because anisotropic filter implies linear filtering in DirectX, we need to center UVs manually.

            uv *= native::spatial::global.textureSize.xy;
            uv = floor(uv) + 0.5f;
            uv /= native::spatial::global.textureSize.xy;

            float4 color;

            if (isBlock) color = native::rt::textureSlotOne[textureIndex].SampleGrad(
                native::spatial::sampler,
                uv,
                ddx,
                ddy);
            else color = native::rt::textureSlotTwo[textureIndex].SampleGrad(native::spatial::sampler, uv, ddx, ddy);

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
