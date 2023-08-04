//  <copyright file="Hit.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Spatial.hlsl"
#include "Decoding.hlsl"

// todo: write data layout descriptions in wiki

[shader("closesthit")]
void SimpleSectionClosestHit(inout HitInfo payload, const Attributes attributes)
{
    const Info info = GetCurrentInfo(attributes);

    float2 uv = info.uv;
    if (decode::GetTextureRotationFlag(info.data)) uv = RotateUV(uv);
    uv *= decode::GetTextureRepetition(info.data);

    uint textureIndex = decode::GetTextureIndex(info.data);
    const float4 tint = decode::GetTintColor(info.data);

    const bool animated = decode::GetAnimationFlag(info.data);
    if (animated) textureIndex = GetAnimatedBlockTextureIndex(textureIndex);

    const float2 ts = float2(gTextureSize.x, gTextureSize.y) * frac(uv);
    uint2 texel = uint2(ts.x, ts.y);
    const uint mip = 0;

    float4 baseColor = gTextureSlotOne[textureIndex].Load(int3(texel, mip));
    if (baseColor.a >= 0.3) baseColor *= tint;
    baseColor.a = 1.0;

    payload.colorAndDistance = float4(CalculateShading(info.normal, baseColor.rgb), RayTCurrent());
}
