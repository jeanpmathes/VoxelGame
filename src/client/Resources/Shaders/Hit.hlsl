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
    float2 uv = info.uv * repetition; // todo: maybe some clamping here, or in GetCurrentInfo?
    if (decode::GetTextureRotationFlag(info.data)) uv = uv.yx;

    uint textureIndex = decode::GetTextureIndex(info.data);
    const float4 tint = decode::GetTintColor(info.data);
    bool animated = decode::GetAnimationFlag(info.data);

    float4 baseColor = float4(1, 1, 1, 1);
    if (baseColor.a >= 0.3) baseColor *= tint;
    baseColor.a = 1.0f;

    payload.colorAndDistance = float4(CalculateShading(info.normal, baseColor.rgb), RayTCurrent());
}
