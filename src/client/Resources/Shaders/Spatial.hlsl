//  <copyright file="Spatial.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Common.hlsl"

struct SpatialVertex
{
    float3 vertex;
    uint data;
};

StructuredBuffer<SpatialVertex> vertices : register(t0);

cbuffer GlobalCB : register(b0) {
float gTime;
float3 gLightDir;
float gMinLight;
uint2 gTextureSize;
}

cbuffer InstanceCB : register(b1) {
float4x4 iWorld;
float4x4 iWorldNormal;
}

Texture2D gTextureSlotOne[] : register(t0, space1);
Texture2D gTextureSlotTwo[] : register(t0, space2);

RaytracingAccelerationStructure spaceBVH : register(t1);

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

    const uint primitiveIndex = PrimitiveIndex();
    const bool isFirst = (primitiveIndex % 2) == 0;
    const uint vertexIndex = (primitiveIndex / 2) * 4;

    indices = isFirst ? int3(0, 1, 2) : int3(0, 2, 3);
    const int3 i = indices + vertexIndex;

    posA = vertices[i[0]].vertex;
    posB = vertices[i[1]].vertex;
    posC = vertices[i[2]].vertex;

    const float3 e1 = posB - posA;
    const float3 e2 = posC - posA;

    normal = mul(iWorldNormal, float4(normalize(cross(e1, e2)), 0.0)).xyz * -1.0;
    normal = normalize(normal);

    data = uint4(
        vertices[vertexIndex + 0].data,
        vertices[vertexIndex + 1].data,
        vertices[vertexIndex + 2].data,
        vertices[vertexIndex + 3].data);
}

struct Info
{
    /**
     * The a position of the current triangle.
     */
    float3 a;

    /**
     * The b position of the current triangle.
     */
    float3 b;

    /**
     * The c position of the current triangle.
     */
    float3 c;

    /**
     * The indices of the current triangle, relative only to the current quad.
     * This means that the indices are in the range [0, 3].
     */
    int3 indices;

    /**
     * The normal of the current triangle.
     */
    float3 normal;

    /**
     * The interpolation factors of the current triangle.
     */
    float3 barycentric;

    /**
     * The current quad data.
     */
    uint4 data;
};

Info GetCurrentInfo(const in Attributes attributes)
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

float3 CalculateShading(const float3 normal, const float3 baseColor)
{
    const float3 worldOrigin = WorldRayOrigin() + RayTCurrent() * WorldRayDirection();
    const float3 dirToLight = -gLightDir;

    float3 color = baseColor;

    RayDesc ray;
    ray.Origin = worldOrigin;
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
