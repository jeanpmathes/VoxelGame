// <copyright file="FoliageAnimation.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "FastNoiseLite.hlsl"

#include "Animation.hlsl"
#include "Decoding.hlsl"

void ApplySway(inout SpatialVertex vertex, float2 uv, const bool isUpperPart, const bool isDoublePlant,
               const in fnl_state noise)
{
    const float amplitude = 0.2;
    const float speed = 0.8;

    const float strength = (uv.y + (isUpperPart ? 1.0 : 0.0)) * (isDoublePlant ? 0.5 : 1.0);
    const float2 position = vertex.position.xz + gWindDir.xz * gTime * speed;

    vertex.position += gWindDir * fnlGetNoise2D(noise, position.x, position.y) * amplitude * strength;
}

[numthreads(16, 4, 1)]
void Main(uint3 groupID : SV_GroupID, uint3 submissionID : SV_GroupThreadID)
{
    Submission submission = threadGroupData[groupID.x].submissions[submissionID.x];

    if (submission.count == 0) return;

    fnl_state noise = fnlCreateState();
    noise.frequency = 0.35;
    noise.domain_warp_type = FNL_DOMAIN_WARP_BASICGRID;

    const uint threadID = submissionID.y;
    const uint offset = submission.offset + (submission.count / 4) * threadID;
    const uint count = (submission.count / 4) + (threadID == 3 ? submission.count % 4 : 0);

    for (uint quadID = offset; quadID < offset + count; quadID++)
    {
        SpatialVertex quad[VG_VERTICES_PER_QUAD];
        uint4 data;

        for (uint index = 0; index < VG_VERTICES_PER_QUAD; index++)
        {
            quad[index] = source[submission.meshIndex][quadID * 4 + index];
            data[index] = quad[index].data;
        }

        const bool isUpperPart = GetFoliageFlag(data, decode::Foliage::IS_UPPER_PART);
        const bool isDoublePlant = GetFoliageFlag(data, decode::Foliage::IS_DOUBLE_PLANT);
        const float4x2 uvs = decode::GetUVs(data);

        for (uint index = 0; index < VG_VERTICES_PER_QUAD; index++)
        {
            ApplySway(quad[index], uvs[index], isUpperPart, isDoublePlant, noise);
            destination[submission.meshIndex][quadID * 4 + index] = quad[index];
        }
    }
}
