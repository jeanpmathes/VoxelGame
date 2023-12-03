// <copyright file="Spatial.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Common.hlsl"
#include "Payloads.hlsl"
#include "Space.hlsl"

cbuffer MaterialCB : register(b2) {
uint gMaterialIndex;
}

ConstantBuffer<Instance> instances[] : register(b3);

RaytracingAccelerationStructure spaceBVH : register(t0); // todo: rename all shader globals to include g prefix
StructuredBuffer<SpatialVertex> vertices[] : register(t1);

Texture2D gTextureSlotOne[] : register(t0, space1);
Texture2D gTextureSlotTwo[] : register(t0, space2);

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
void ReadMeshData(out int3 indices, out float3 posA, out float3 posB, out float3 posC, out float3 normal,
                  out uint4 data)
{
    // A quad looks like this:
    // 1 -- 2
    // |  / |
    // | /  |
    // 0 -- 3
    // The top left triangle is the first one, the bottom right triangle is the second one.

    const uint instance = InstanceID();
    
    const uint primitiveIndex = PrimitiveIndex();
    const bool isFirst = (primitiveIndex % 2) == 0;
    const uint vertexIndex = (primitiveIndex / 2) * VG_VERTICES_PER_QUAD;

    indices = isFirst ? int3(0, 1, 2) : int3(0, 2, 3);
    const int3 i = indices + vertexIndex;

    posA = vertices[instance][i[0]].position;
    posB = vertices[instance][i[1]].position;
    posC = vertices[instance][i[2]].position;

    const float3 e1 = posB - posA;
    const float3 e2 = posC - posA;

    normal = mul(instances[instance].worldNormal, float4(normalize(cross(e1, e2)), 0.0)).xyz * -1.0;
    normal = normalize(normal);

    posA = mul(instances[instance].world, float4(posA, 1.0)).xyz;
    posB = mul(instances[instance].world, float4(posB, 1.0)).xyz;
    posC = mul(instances[instance].world, float4(posC, 1.0)).xyz;

    data = uint4(
        vertices[instance][vertexIndex + 0].data,
        vertices[instance][vertexIndex + 1].data,
        vertices[instance][vertexIndex + 2].data,
        vertices[instance][vertexIndex + 3].data);
}

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
    float3 GetPosition()
    {
        return a * barycentric.x + b * barycentric.y + c * barycentric.z;
    }
};

Info GetCurrentInfo(const in Attributes attributes) // todo: write doc-comments for all functions in shaders
{
    Info info;

    ReadMeshData(info.indices, info.a, info.b, info.c, info.normal, info.data);

    info.barycentric = GetBarycentrics(attributes);

    return info;
}

float2 RotateUV(float2 uv)
{
    float2 rotatedUV;

    rotatedUV.x = uv.y;
    rotatedUV.y = abs(uv.x - 1.0);

    return rotatedUV;
}

uint GetAnimatedIndex(const uint index, const uint frameCount, const float quadFactor)
{
    if (index == 0) return 0;
    
    const uint quadID = PrimitiveIndex() / 2;
    return index + uint(fmod(gTime * frameCount + quadID * quadFactor, frameCount));
}

uint GetAnimatedBlockTextureIndex(const uint index)
{
    return GetAnimatedIndex(index, 8, 0.125);
}

uint GetAnimatedFluidTextureIndex(const uint index)
{
    return GetAnimatedIndex(index, 16, 0.00);
}

float3 CalculateShading(in Info info, const float3 baseColor)
{
    const float3 dirToLight = -gLightDir;
    const float3 normal = info.normal;

    float3 color = baseColor;

    RayDesc ray;
    ray.Origin = info.GetPosition();
    ray.Direction = dirToLight;
    ray.TMin = VG_RAY_EPSILON;
    ray.TMax = VG_RAY_DISTANCE;

    ShadowHitInfo shadowPayload;
    shadowPayload.isHit = false;

    TraceRay(spaceBVH, RAY_FLAG_NONE, VG_MASK_SHADOW, VG_HIT_ARG(1), ray, shadowPayload);

    const float visibility = shadowPayload.isHit ? 0.0 : 1.0;

    const float lightIntensity = clamp(dot(normal, dirToLight) * visibility, gMinLight, 1.0);
    color *= lightIntensity;

    return color;
}
