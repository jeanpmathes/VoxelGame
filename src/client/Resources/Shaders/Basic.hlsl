#include "Spatial.hlsl"
#include "Decoding.hlsl"

float4 GetBasicBaseColor(const in Info info)
{
    float2 uv = info.uv;
    if (decode::GetTextureRotationFlag(info.data)) uv = RotateUV(uv);
    uv *= decode::GetTextureRepetition(info.data);

    uint textureIndex = decode::GetTextureIndex(info.data);

    const bool animated = decode::GetAnimationFlag(info.data);
    if (animated) textureIndex = GetAnimatedBlockTextureIndex(textureIndex);

    const float2 ts = float2(gTextureSize.x, gTextureSize.y) * frac(uv);
    uint2 texel = uint2(ts.x, ts.y);
    const uint mip = 0;

    return gTextureSlotOne[textureIndex].Load(int3(texel, mip));
}
