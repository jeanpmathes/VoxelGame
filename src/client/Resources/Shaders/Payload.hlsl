//  <copyright file="Payload.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#ifndef VG_SHADER_PAYLOAD_HLSL
#define VG_SHADER_PAYLOAD_HLSL

#include "CommonRT.hlsl"
#include "Packing.hlsl"
#include "PayloadRT.hlsl"

/**
 * Utilities to work with the ray payload.
 * The fields of the payload are used differently during the ray tracing process.
 *
 * Color: (uint2)
 *  - The field is zeroed on trace start.
 *  - On each closer hit, the color is updated.
 *  - On closest hit, the final shading color is written.
 *
 * Normal: (float2)
 *  - The field is zeroed on trace start.
 *  - On closest hit, the final normal is written.
 *
 * Position: (float3)
 *  - The x component is initialized with the total path length.
 *  - The y component contains the ray length, initialized with RAY_DISTANCE.
 *  - The z component is zeroed on trace start.
 *  - On each closer hit, the distance is updated.
 *  - On closest hit, the final position is written.
 *
 * Data: (uint1)
 *  - The field is zeroed on trace start.
 *  - The lower 24 bits are used for the fog color, which can be provided by the hit shader.
 */
namespace vg
{
    namespace ray
    {
        float4 GetColor(in native::rt::HitInfo const payload) { return float4(native::packing::UnpackColor4(payload.color)); }

        void SetColor(inout native::rt::HitInfo payload, in float4 const color) { payload.color = native::packing::PackColor4(color); }

        float3 GetNormal(in native::rt::HitInfo const payload) { return native::packing::UnpackNormal(payload.normal); }

        void SetNormal(inout native::rt::HitInfo payload, in float3 const normal) { payload.normal = native::packing::PackNormal(normal); }

        float GetPathLength(in native::rt::HitInfo const payload) { return payload.position.x; }

        void SetPathLength(inout native::rt::HitInfo payload, in float const path) { payload.position.x = path; }

        float GetRayDistance(in native::rt::HitInfo const payload) { return payload.position.y; }

        void SetRayDistance(inout native::rt::HitInfo payload, in float const distance) { payload.position.y = distance; }

        float3 GetPosition(in native::rt::HitInfo const payload) { return payload.position; }

        void SetPosition(inout native::rt::HitInfo payload, in float3 const position) { payload.position = position; }

        int GetData(in native::rt::HitInfo const payload) { return payload.data.x; }

        void SetData(inout native::rt::HitInfo payload, in int const data) { payload.data.x = data; }

        static int const FOG_COLOR_MASK = 0x00FFFFFF;

        float3 GetFogColor(in native::rt::HitInfo const payload) { return float3(native::packing::UnpackColor3(payload.data.x & FOG_COLOR_MASK)); }

        void SetFogColor(inout native::rt::HitInfo payload, in float3 const color) { payload.data.x = (payload.data.x & ~FOG_COLOR_MASK) | native::packing::PackColor3(color); }

        /**
         * \brief Create an initialized hit info / ray payload struct.
         * \param path The total path length.
         * \return The empty hit info struct.
         */
        native::rt::HitInfo GetInitialHitInfo(float const path)
        {
            native::rt::HitInfo payload;

            payload.color    = uint2(0, 0);
            payload.normal   = float2(0.0f, 0.0f);
            payload.position = float3(0.0f, 0.0f, 0.0f);
            payload.data     = uint1(0);

            // Allows any-hit to check if it is closer then write to payload.
            SetRayDistance(payload, native::rt::RAY_DISTANCE);

            // Allows estimating the ray footprint.
            SetPathLength(payload, path);

            return payload;
        }

        struct TraceResult
        {
            float3 position;
            float3 normal;
            float4 color;
            int    data;
            float  distance;
            float3 fogColor;
        };

        /**
         * Get an empty trace result.
         */
        TraceResult GetEmptyTraceResult()
        {
            TraceResult result;

            result.position = float3(0.0f, 0.0f, 0.0f);
            result.normal   = float3(0.0f, 0.0f, 0.0f);
            result.color    = float4(0.0f, 0.0f, 0.0f, 0.0f);
            result.data     = 0;
            result.distance = 0.0f;
            result.fogColor = float3(0.0f, 0.0f, 0.0f);

            return result;
        }

        /**
         * Get the result of a ray tracing operation from the payload.
         */
        TraceResult GetTraceResult(in native::rt::HitInfo const payload, float3 const origin)
        {
            TraceResult result;

            result.position = GetPosition(payload);
            result.normal   = GetNormal(payload);
            result.color    = GetColor(payload);
            result.data     = GetData(payload);
            result.distance = length(GetPosition(payload) - origin);
            result.fogColor = GetFogColor(payload);

            return result;
        }
    }
}

#define RGBA(color) float4((color).rgb, 1.0f)

#define SET_INTERMEDIATE_HIT_INFO(payload, info, shadingColor) \
    { \
        vg::ray::SetColor(payload, shadingColor); \
        vg::ray::SetRayDistance(payload, RayTCurrent()); \
    } (void)0


#define SET_FINAL_HIT_INFO(payload, info, shadingColor) \
    { \
        vg::ray::SetColor(payload, shadingColor); \
        vg::ray::SetNormal(payload, info.normal); \
        vg::ray::SetPosition(payload, info.GetPosition()); \
    } (void)0

#define SET_MISS_INFO(payload, shadingColor) \
    { \
        vg::ray::SetColor(payload, shadingColor); \
        vg::ray::SetNormal(payload, float3(0.0f, 0.0f, 0.0f)); \
        vg::ray::SetPosition(payload, WorldRayOrigin() + RayTCurrent() * WorldRayDirection()); \
    } (void)0

#endif
