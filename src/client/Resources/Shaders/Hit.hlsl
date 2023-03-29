#include "Common.hlsl"

struct STriVertex
{
    float3 vertex;
    float4 color;
};

StructuredBuffer<STriVertex> vertices : register(t0);
StructuredBuffer<int> indices: register(t1);

cbuffer GlobalCB : register(b0) {
float gTime;
float3 gLightPos;
float minLight;
}

cbuffer InstanceCB : register(b1) {
float4x4 iWorld;
float4x4 iWorldNormal;
}

RaytracingAccelerationStructure SpaceBVH : register(t2);

float3 CalculateShading(int v1, int v2, int v3, Attributes attributes)
{
    const float3 worldOrigin = WorldRayOrigin() + RayTCurrent() * WorldRayDirection();
    const float3 lightDir = normalize(gLightPos - worldOrigin);

    float3 barycentrics = float3(
        1.f - attributes.barycentrics.x - attributes.barycentrics.y,
        attributes.barycentrics.x,
        attributes.barycentrics.y);

    float3 color =
        vertices[v1].color * barycentrics.x +
        vertices[v2].color * barycentrics.y +
        vertices[v3].color * barycentrics.z;

    const float3 e1 = vertices[v2].vertex - vertices[v1].vertex;
    const float3 e2 = vertices[v3].vertex - vertices[v1].vertex;

    float3 normal = normalize(cross(e1, e2));
    normal = mul(iWorldNormal, float4(normal, 0.f)).xyz;

    // Backface culling with CCW winding:
    if (dot(normal, WorldRayDirection()) > 0.f)
    {
        return float3(0.f, 0.f, 0.f);
    }

    RayDesc ray;
    ray.Origin = worldOrigin;
    ray.Direction = lightDir;
    ray.TMin = 0.01;
    ray.TMax = 100000;

    ShadowHitInfo shadowPayload;
    shadowPayload.isHit = false;

    TraceRay(SpaceBVH, RAY_FLAG_NONE, 0xFF, 1, 0, 1, ray, shadowPayload);

    const float visibility = shadowPayload.isHit ? 0.0f : 1.0f;

    const float lightIntensity = clamp(dot(normal, lightDir) * visibility, minLight, 1.0f);
    color *= lightIntensity;

    return color;
}

[shader("closesthit")]
void IndexedClosestHit(inout HitInfo payload, Attributes attributes)
{
    const uint vertId = 3 * PrimitiveIndex();

    float3 hitColor = CalculateShading(
        indices[vertId + 0],
        indices[vertId + 1],
        indices[vertId + 2],
        attributes);

    payload.colorAndDistance = float4(hitColor, RayTCurrent());
}

[shader("closesthit")]
void SequencedClosestHit(inout HitInfo payload, Attributes attributes)
{
    const uint vertId = 3 * PrimitiveIndex();

    float3 hitColor = CalculateShading(
        vertId + 0,
        vertId + 1,
        vertId + 2,
        attributes);

    payload.colorAndDistance = float4(hitColor, RayTCurrent());
}
