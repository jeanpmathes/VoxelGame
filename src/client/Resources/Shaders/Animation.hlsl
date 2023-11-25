// <copyright file="Animation.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Space.hlsl"

struct Submission
{
    uint meshIndex;
    uint instanceIndex;

    uint offset;
    uint count;
};

struct ThreadGroup
{
    Submission submissions[16];
};

StructuredBuffer<ThreadGroup> threadGroupData : register(t0, space3);

StructuredBuffer<SpatialVertex> source[] : register(t1, space3);
RWStructuredBuffer<SpatialVertex> destination[] : register(u0, space3);
