// <copyright file="Spatial.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>


#include "SpatialRT.hlsl"

#include "Custom.hlsl"
#include "Decoding.hlsl"

// @formatter:off
using namespace native::rt;
// @formatter:on

/**
 * \brief Defines basic data and operations for the spatial rendering used by the game.
 */
namespace vg
{
    namespace spatial
    {
        /**
         * Read the mesh data.
         * The first part of the mesh data are the vertex positions of the triangle that were hit.
         * A normal is calculated from these positions.
         * The order of the positions is CCW.
         * Additionally, the data stored in the quad the triangle belongs to is read.
         * For this to work, the following conditions must be met:
         * - The mesh must be a quad mesh.
         * - The vertex order is CW.
         */
        void ReadMeshData(
            out int3 indices, out float3 posA, out float3 posB, out float3 posC, out float3 normal, out uint4 data)
        {
            // A quad looks like this:
            // 1 -- 2
            // |  / |
            // | /  |
            // 0 -- 3
            // The top left triangle is the first one, the bottom right triangle is the second one.

            uint const instance = InstanceID();

            uint const primitiveIndex = PrimitiveIndex();
            bool const isFirst        = (primitiveIndex % 2) == 0;
            uint const vertexIndex    = (primitiveIndex / 2) * native::spatial::VERTICES_PER_QUAD;

            indices      = isFirst ? int3(0, 1, 2) : int3(0, 2, 3);
            int3 const i = indices + vertexIndex;

            posA = vertices[instance][i[0]].position;
            posB = vertices[instance][i[1]].position;
            posC = vertices[instance][i[2]].position;

            float3 const e1 = posB - posA;
            float3 const e2 = posC - posA;

            normal = mul(instances[instance].worldNormal, float4(normalize(cross(e1, e2)), 0.0)).xyz * -1.0;
            normal = normalize(normal);

            posA = mul(instances[instance].world, float4(posA, 1.0f)).xyz;
            posB = mul(instances[instance].world, float4(posB, 1.0f)).xyz;
            posC = mul(instances[instance].world, float4(posC, 1.0f)).xyz;

            data = uint4(
                vertices[instance][vertexIndex + 0].data,
                vertices[instance][vertexIndex + 1].data,
                vertices[instance][vertexIndex + 2].data,
                vertices[instance][vertexIndex + 3].data);

            if (decode::GetNormalInvertedFlag(data)) normal *= -1.0f;
        }

        /**
         * \brief Information about the hit and both the triangle and quad that were hit.
         */
        struct Info
        {
            /**
             * \brief The a position of the current triangle.
             */
            float3 a;

            /**
             * \brief The b position of the current triangle.
             */
            float3 b;

            /**
             * \brief The c position of the current triangle.
             */
            float3 c;

            /**
             * \brief The indices of the current triangle, relative only to the current quad. This means that the indices are in the range [0, 3].
             */
            int3 indices;

            /**
             * \brief The normal of the current triangle.
             */
            float3 normal;

            /**
             * \brief The interpolation factors of the current triangle.
             */
            float3 barycentric;

            /**
             * \brief The current quad data.
             */
            uint4 data;

            /**
             * \brief Get the position of the current intersection, using the barycentric coordinates.
             * \return The position of the current intersection.
             */
            float3 GetPosition() { return a * barycentric.x + b * barycentric.y + c * barycentric.z; }

            /**
             * \brief Get the distance from the current intersection to the borders of the current triangle.
             * \return The distance.
             */
            float GetDistanceToTriangleBorders()
            {
                float3 const p = GetPosition();

                float3 const ba = b - a, cb = c - b, ac = a - c;
                float3 const pa = p - a, pb = p - b, pc = p - c;

                float3 const ae = pa - ba * clamp(dot(pa, ba) / dot(ba, ba), 0.0f, 1.0f);
                float3 const be = pb - cb * clamp(dot(pb, cb) / dot(cb, cb), 0.0f, 1.0f);
                float3 const ce = pc - ac * clamp(dot(pc, ac) / dot(ac, ac), 0.0f, 1.0f);

                return sqrt(min(min(dot(ae, ae), dot(be, be)), dot(ce, ce)));
            }
        };

        /**
         * \brief Get the current triangle info.
         * \param attributes The attributes of the current hit.
         * \return The current triangle info.
         */
        Info GetCurrentInfo(in native::rt::Attributes const attributes)
        {
            Info info;

            ReadMeshData(info.indices, info.a, info.b, info.c, info.normal, info.data);

            info.barycentric = native::rt::GetBarycentrics(attributes);

            return info;
        }

        /**
         * \brief Rotate the UV coordinates by 90 degrees.
         * \param uv The UV coordinates.
         * \return The rotated UV coordinates.
         */
        float2 RotateUV(float2 uv)
        {
            float2 rotatedUV;

            rotatedUV.x = uv.y;
            rotatedUV.y = abs(uv.x - 1.0f);

            return rotatedUV;
        }

        /**
         * \brief Calculate the shading for the current hit.
         * \param info The info about the current hit.
         * \param baseColor The base color to use, is the color of the texture.
         * \return The calculated color with shading applied.
         */
        float3 CalculateShading(in Info info, float3 const baseColor)
        {
            bool const inner = decode::GetNormalInvertedFlag(info.data);
            
            float3 const dirToLight = native::spatial::global.lightDir * -1.0f;
            float3 const normal     = info.normal * (inner ? -1.0f : 1.0f);

            float3 color = baseColor;

            bool const shaded = !decode::GetUnshadedFlag(info.data);
            float      intensity;

            if (shaded)
            {
                RayDesc ray;
                ray.Origin    = info.GetPosition();
                ray.Direction = dirToLight;
                ray.TMin      = native::rt::RAY_EPSILON;
                ray.TMax      = native::rt::RAY_DISTANCE;

                native::rt::ShadowHitInfo shadowPayload;
                shadowPayload.isHit = false;

                TraceRay(
                    native::rt::spaceBVH,
                    RAY_FLAG_NONE,
                    native::rt::MASK_SHADOW,
                    RT_HIT_ARG(1),
                    ray,
                    shadowPayload);

                float const energy = dot(normal, dirToLight);

                if (!shadowPayload.isHit) intensity = clamp(energy, native::spatial::global.minLight, 1.0f);
                else
                    intensity = lerp(
                        native::spatial::global.minShadow,
                        native::spatial::global.minLight,
                        clamp(energy * -1.0f, 0.0f, 1.0f));
            }
            else intensity = 1.0f;

            color *= intensity;

            if (custom.wireframe)
            {
                float const edge = info.GetDistanceToTriangleBorders();
                color            = edge < 0.005f ? 1.0f : lerp(color, 0.0f, 0.2f);
            }

            return color;
        }
    }
}
