//  <copyright file="Common.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

struct Attributes
{
    float2 barycentrics;
};

float3 GetBarycentrics(in Attributes attributes)
{
    return float3(
        1.0 - attributes.barycentrics.x - attributes.barycentrics.y,
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

#define POW2(x) ((x) * (x))
#define POW3(x) (POW2(x) * (x))
#define POW4(x) (POW2(x) * POW2(x))
#define POW5(x) (POW3(x) * POW2(x))

#define VG_RAY_DISTANCE 100000.0
#define VG_RAY_EPSILON 0.01

#define VG_MASK_VISIBLE (1 << 0)
#define VG_MASK_SHADOW (1 << 1)

#define VG_HIT_ARG(index) index, 0, index
