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

float3 HUEtoRGB(const in float h)
{
    float r = abs(h * 6 - 3) - 1;
    float g = 2 - abs(h * 6 - 2);
    float b = 2 - abs(h * 6 - 4);
    return saturate(float3(r, g, b));
}
