//  <copyright file="Decoding.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

// todo: write data layout descriptions in wiki

#define BITMASK(x) ( (1 << (x)) - 1 )

/**
 * Consider the wiki (https://github.com/jeanpmathes/VoxelGame/wiki/Section-Buffers) for more information.
 */
namespace decode
{
    // todo: write data layout descriptions in wiki

    uint GetTextureIndex(const uint4 data)
    {
        return data[0] & BITMASK(13);
    }

    float4 GetTintColor(const uint4 data)
    {
        uint r = (data[1] >> 29) & BITMASK(3);
        uint g = (data[1] >> 26) & BITMASK(3);
        uint b = (data[1] >> 23) & BITMASK(3);

        return float4(float3(r, g, b) / 7.0f, 1.0f);
    }

    bool GetAnimationFlag(const uint4 data)
    {
        return (data[1] >> 0) & BITMASK(1);
    }

    bool GetTextureRotationFlag(const uint4 data)
    {
        return (data[1] >> 1) & BITMASK(1);
    }

    float2 GetTextureRepetition(const uint4 data)
    {
        const uint x = (data[2] >> 4) & BITMASK(4);
        const uint y = (data[2] >> 0) & BITMASK(4);

        return float2(x + 1, y + 1);
    }
}
