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

    const float2 repetition = decode::GetTextureRepetition(info.data);
    float2 uv = info.uv * repetition;
    if (decode::GetTextureRotationFlag(info.data)) uv = uv.yx;

    const uint textureIndex = decode::GetTextureIndex(info.data);
    const float4 tint = decode::GetTintColor(info.data);
    bool animated = decode::GetAnimationFlag(info.data);

    uint2 index;
    SplitTextureIndex(textureIndex, index.x, index.y);

    const float2 ts = float2(gTextureSize.x, gTextureSize.y) * uv;
    uint2 texel = uint2(clamp(ts, 0.0f, float2(gTextureSize.x - 1, gTextureSize.y - 1)));
    const uint mip = 0;

    float4 baseColor = gTextureSlotOne[index.x].Load(int4(texel, index.y, mip));
    if (baseColor.a >= 0.3) baseColor *= tint;
    baseColor.a = 1.0f;

    payload.colorAndDistance = float4(CalculateShading(info.normal, baseColor.rgb), RayTCurrent());
}
