#include "Spatial.hlsl"
#include "Decoding.hlsl"

float2 GetUV(const in Info info, const bool useTextureRepetition)
{
    const float4x2 uvs = decode::GetUVs(info.data);

    const float2 uvX = uvs[info.indices.x];
    const float2 uvY = uvs[info.indices.y];
    const float2 uvZ = uvs[info.indices.z];

    float2 uv = uvX * info.barycentric.x + uvY * info.barycentric.y + uvZ * info.barycentric.z;

    if (decode::GetTextureRotationFlag(info.data))
        uv = RotateUV(uv);

    if (useTextureRepetition)
        uv *= decode::GetTextureRepetition(info.data);

    return uv;
}

float4 GetBaseColor(const in Info info, const bool useTextureRepetition)
{
    const float2 uv = GetUV(info, useTextureRepetition);
    uint textureIndex = decode::GetTextureIndex(info.data);

    const bool animated = decode::GetAnimationFlag(info.data);
    if (animated) textureIndex = GetAnimatedBlockTextureIndex(textureIndex);

    const float2 ts = float2(gTextureSize.x, gTextureSize.y) * frac(uv);
    uint2 texel = uint2(ts.x, ts.y);
    const uint mip = 0;

    return gTextureSlotOne[textureIndex].Load(int3(texel, mip));
}

float4 GetBasicBaseColor(const in Info info)
{
    return GetBaseColor(info, true);
}

float4 GetFoliageBaseColor(const in Info info)
{
    return GetBaseColor(info, false);
}
