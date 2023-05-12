//  <copyright file="Spatial.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Common.hlsl"

struct STriVertex
{
    float3 vertex;
    uint data;
};

StructuredBuffer<STriVertex> vertices : register(t0);
StructuredBuffer<int> indices: register(t1);

cbuffer GlobalCB : register(b0) {
float gTime;
float3 gLightDir;
float gMinLight;
}

cbuffer InstanceCB : register(b1) {
float4x4 iWorld;
float4x4 iWorldNormal;
}

RaytracingAccelerationStructure spaceBVH : register(t2);

/**
 * Read the basic mesh data, meaning the vertex indices, the vertex positions and the normal.
 */
void ReadMeshData(out int3 vi, out float3 posX, out float3 posY, out float3 posZ, out float3 normal)
{
    const uint vertId = 3 * PrimitiveIndex();

    vi = int3(
        indices[vertId + 0],
        indices[vertId + 1],
        indices[vertId + 2]);

    posX = vertices[vi[0]].vertex;
    posY = vertices[vi[1]].vertex;
    posZ = vertices[vi[2]].vertex;

    const float3 e1 = posY - posX;
    const float3 e2 = posZ - posX;

    normal = mul(iWorldNormal, float4(normalize(cross(e1, e2)), 0.f)).xyz;
}

/**
 * Read the additional data associated with the current quad.
 * For this to work, the following conditions must be met:
 * - The mesh must be a quad mesh.
 * - The indices of a quad follow the pattern 0-2-1-|-0-3-2-|...
 * - The vertex order is CW, the index order is CCW.
 */
void ReadQuadData(out uint4 data)
{
    const bool isFirst = (PrimitiveIndex() % 2) == 0;

    const uint2 vertIds = isFirst
                              ? uint2(3 * PrimitiveIndex(), 3 * (PrimitiveIndex() + 1))
                              : uint2(3 * (PrimitiveIndex() - 1), 3 * PrimitiveIndex());

    data = uint4(
        vertices[vertIds[0] + 0].data, // Vertex 0
        vertices[vertIds[0] + 2].data, // Vertex 1
        vertices[vertIds[1] + 2].data, // Vertex 2
        vertices[vertIds[1] + 1].data); // Vertex 3
}

/**
 * Get the interpolation factors for the quad.
 * For this to work, the following conditions must be met:
 *  - The mesh must be a quad mesh.
 *  - The indices of a quad follow the pattern 0-2-1-|-0-3-2-|...
 *  - The vertex order is CW, the index order is CCW.
 */
float2 GetQuadInterpolation(float3 barycentric)
{
    const bool isFirst = (PrimitiveIndex() % 2) == 0;
    // Because the barycentrics are in vertex order instead of index order,
    // the pattern is now: 0-1-2-|-0-2-3-|...

    const float2 a = float2(0.0, 0.0);
    const float2 b = float2(0.0, 1.0);
    const float2 c = float2(1.0, 1.0);
    const float2 d = float2(1.0, 0.0);

    float2 uv;

    if (isFirst)
    {
        uv = barycentric.x * a
            + barycentric.y * b
            + barycentric.z * c;
    }
    else
    {
        uv = barycentric.x * a
            + barycentric.y * c
            + barycentric.z * d;
    }

    return uv;
}

struct Info
{
    /**
     * The three vertex indices of the current triangle.
     */
    int3 vi;

    /**
     * The x position of the current triangle.
     */
    float3 x;

    /**
     * The y position of the current triangle.
     */
    float3 y;

    /**
     * The z position of the current triangle.
     */
    float3 z;

    /**
     * The normal of the current triangle.
     */
    float3 normal;

    /**
     * The interpolation factors of the current triangle.
     */
    float3 barycentric;

    /**
     * The interpolation factors of the current quad.
     */
    float2 uv;

    /**
     * The current quad data.
     */
    uint4 data;
};

Info GetCurrentInfo(const in Attributes attributes)
{
    Info info;

    ReadMeshData(info.vi, info.x, info.y, info.z, info.normal);
    ReadQuadData(info.data);

    info.barycentric = GetBarycentrics(attributes);
    info.uv = GetQuadInterpolation(info.barycentric);

    return info;
}

float3 CalculateShading(const float3 normal, const float3 baseColor)
{
    const float3 worldOrigin = WorldRayOrigin() + RayTCurrent() * WorldRayDirection();
    const float3 dirToLight = -gLightDir;
    
    float3 color = baseColor;

    // Backface culling with CCW winding:
    if (dot(normal, WorldRayDirection()) > 0.f)
    {
        return float3(0.f, 0.f, 0.f);
    }

    RayDesc ray;
    ray.Origin = worldOrigin;
    ray.Direction = dirToLight;
    ray.TMin = VG_RAY_EPSILON;
    ray.TMax = VG_RAY_DISTANCE;

    ShadowHitInfo shadowPayload;
    shadowPayload.isHit = false;

    TraceRay(spaceBVH, RAY_FLAG_NONE, 0xFF, 1, 0, 1, ray, shadowPayload);

    const float visibility = shadowPayload.isHit ? 0.0f : 1.0f;

    const float lightIntensity = clamp(dot(normal, dirToLight) * visibility, gMinLight, 1.0f);
    color *= lightIntensity;

    return color;
}
