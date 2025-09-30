// <copyright file="FoliageAnimation.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "FastNoiseLite.hlsl"

#include "Animation.hlsl"
#include "Custom.hlsl"
#include "Decoding.hlsl"

void ApplySway(inout native::spatial::SpatialVertex vertex, float2 uv, bool const isUpperPart, bool const isDoublePlant, in fnl_state const noise)
{
    float const amplitude = 0.2f;
    float const speed     = 0.5f;

    float const  strength = (uv.y + (isUpperPart ? 1.0f : 0.0f)) * (isDoublePlant ? 0.5f : 1.0f);
    float2 const position = vertex.position.xz + vg::custom.windDir.xz * native::spatial::global.time * speed;

    vertex.position += vg::custom.windDir * fnlGetNoise2D(noise, position.x, position.y) * amplitude * strength;
}

[numthreads(32, 1, 1)]void Main(uint3 id : SV_DispatchThreadID)
{
    uint const count  = native::animation::size.value;
    uint const quadID = id.x;

    if (quadID >= count) return;

    fnl_state noise        = fnlCreateState();
    noise.frequency        = 0.35f;
    noise.domain_warp_type = FNL_DOMAIN_WARP_BASICGRID;

    uint const instance = native::animation::index.value;
    uint const vertexID = quadID * 4;

    native::spatial::SpatialVertex quad[native::spatial::VERTICES_PER_QUAD];
    uint4                          data;

    for (uint index = 0; index < native::spatial::VERTICES_PER_QUAD; index++)
    {
        quad[index] = native::animation::source[instance][vertexID + index];
        data[index] = quad[index].data;
    }

    bool const     isUpperPart   = GetFoliageFlag(data, vg::decode::Foliage::IS_UPPER_PART);
    bool const     isDoublePlant = GetFoliageFlag(data, vg::decode::Foliage::IS_DOUBLE_PLANT);
    float4x2 const uvs           = vg::decode::GetUVs(data);

    for (uint index = 0; index < native::spatial::VERTICES_PER_QUAD; index++)
    {
        ApplySway(quad[index], uvs[index], isUpperPart, isDoublePlant, noise);
        native::animation::destination[instance][vertexID + index] = quad[index];
    }
}
