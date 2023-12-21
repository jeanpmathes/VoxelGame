//  <copyright file="Space.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

struct SpatialVertex
{
    float3 position;
    uint data;
};

cbuffer GlobalCB : register(b2)
{
float gTime;
float3 gWindDir;
float3 gLightDir;
float gMinLight;
float gMinShadow;
uint2 gTextureSize;
}

struct Instance
{
    float4x4 world;
    float4x4 worldNormal;
};

#define VG_VERTICES_PER_QUAD 4
