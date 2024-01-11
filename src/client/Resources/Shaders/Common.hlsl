//  <copyright file="Common.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#ifndef VG_SHADER_COMMON_H
#define VG_SHADER_COMMON_H

float3 HUEtoRGB(const in float h)
{
    float r = abs(h * 6 - 3) - 1;
    float g = 2 - abs(h * 6 - 2);
    float b = 2 - abs(h * 6 - 4);
    return saturate(float3(r, g, b));
}

float GetLuminance(const in float3 color)
{
    return dot(color, float3(0.299, 0.587, 0.114));
}

// ReSharper disable CppInconsistentNaming
template
<
typename T
>
T invlerp(const T a, const T b, const T v)
{
    return (v - a) / (b - a);
}

template
<
typename T
>
T remap(const T a, const T b, const T c, const T d, const T v)
{
    return lerp(c, d, invlerp(a, b, v));
}

// ReSharper restore CppInconsistentNaming

static const float3 RED = float3(1.0, 0.0, 0.0);
static const float3 GREEN = float3(0.0, 1.0, 0.0);
static const float3 BLUE = float3(0.0, 0.0, 1.0);

#define POW2(x) ((x) * (x))
#define POW3(x) (POW2(x) * (x))
#define POW4(x) (POW2(x) * POW2(x))
#define POW5(x) (POW3(x) * POW2(x))

#endif
