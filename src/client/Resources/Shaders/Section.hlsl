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

int4 GetBaseColorIndex(const in Info info, const bool useTextureRepetition, const bool isBlock)
{
    const float2 uv = GetUV(info, useTextureRepetition);
    uint textureIndex = decode::GetTextureIndex(info.data);

    const bool animated = decode::GetAnimationFlag(info.data);
    if (animated && isBlock) textureIndex = GetAnimatedBlockTextureIndex(textureIndex);
    if (animated && !isBlock) textureIndex = GetAnimatedFluidTextureIndex(textureIndex);

    const float2 ts = float2(gTextureSize.x, gTextureSize.y) * frac(uv);
    uint2 texel = uint2(ts.x, ts.y);
    const uint mip = 0;

    return int4(textureIndex, texel.x, texel.y, mip);
}

float4 GetBasicBaseColor(const in Info info)
{
    int4 index = GetBaseColorIndex(info, true, true);
    return gTextureSlotOne[index.x].Load(index.yzw);
}

float4 GetFoliageBaseColor(const in Info info)
{
    int4 index = GetBaseColorIndex(info, false, true);
    return gTextureSlotOne[index.x].Load(index.yzw);
}

float4 GetFluidBaseColor(const in Info info)
{
    int4 index = GetBaseColorIndex(info, true, false);
    return gTextureSlotTwo[index.x].Load(index.yzw);
}

#define SET_HIT_INFO(payload, info, shading) \
    { \
        payload.distance = RayTCurrent(); \
        payload.normal = info.normal; \
        payload.color = shading; \
        payload.alpha = 1.0; \
    } (void)0
