//  <copyright file="Common.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

struct HitInfo
{
    float4 colorAndDistance;
};

struct ShadowHitInfo
{
    bool isHit;
};

struct Attributes
{
    float2 barycentrics;
};

float3 GetBarycentrics(in Attributes attributes)
{
    return float3(
        1.f - attributes.barycentrics.x - attributes.barycentrics.y,
        attributes.barycentrics.x,
        attributes.barycentrics.y);
}

float3 HUEtoRGB(const in float h)
{
    float r = abs(h * 6 - 3) - 1;
    float g = 2 - abs(h * 6 - 2);
    float b = 2 - abs(h * 6 - 4);
    return saturate(float3(r, g, b));
}

#define VG_RAY_DISTANCE 100000.0f
#define VG_RAY_EPSILON 0.01f
