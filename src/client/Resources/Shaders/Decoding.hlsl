//  <copyright file="Decoding.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#ifndef VG_SHADER_DECODING_HLSL
#define VG_SHADER_DECODING_HLSL

#define BITMASK(x) ( (1 << (x)) - 1 )

/**
 * Defines how to decode the data stored in the integer data passed along with the vertices.
 * Consider the wiki (https://github.com/jeanpmathes/VoxelGame/wiki/Section-Buffers) for more information.
 */
namespace vg
{
    namespace decode
    {
        /**
     * \brief Get the texture index.
     * \param data The data to decode.
     * \return The texture index.
     */
        uint GetTextureIndex(const uint4 data)
        {
            return data[0] & BITMASK(13);
        }

        /**
     * \brief Get the color of the tint.
     * \param data The data to decode.
     * \return The color of the tint.
     */
        float4 GetTintColor(const uint4 data)
        {
            uint r = (data[1] >> 29) & BITMASK(3);
            uint g = (data[1] >> 26) & BITMASK(3);
            uint b = (data[1] >> 23) & BITMASK(3);

            return float4(float3(r, g, b) / 7.0f, 1.0f);
        }

        /**
     * \brief Get the animation flag.
     * \param data The data to decode.
     * \return Whether the quad is animated.
     */
        bool GetAnimationFlag(const uint4 data)
        {
            return (data[1] >> 0) & BITMASK(1);
        }

        /**
     * \brief Get the texture rotation flag.
     * \param data The data to decode.
     * \return Whether the quad texture is rotated.
     */
        bool GetTextureRotationFlag(const uint4 data)
        {
            return (data[1] >> 1) & BITMASK(1);
        }

        /**
     * \brief Get the unshaded flag.
     * \param data The data to decode.
     * \return Whether the quad is unshaded.
     */
        bool GetUnshadedFlag(const uint4 data)
        {
            return (data[1] >> 2) & BITMASK(1);
        }

        /**
     * \brief Decode a float4 from a base 17 number.
     * \param value The value to decode.
     * \return The decoded float4.
     */
        float4 DecodeFromBase17(const uint value)
        {
            uint x = value % 17;
            uint y = (value / 17) % 17;
            uint z = (value / 17 / 17) % 17;
            uint w = (value / 17 / 17 / 17) % 17;

            return float4(x, y, z, w) / 16.0f;
        }

        /**
     * \brief Get the UVs.
     * \param data The data to decode.
     * \return The UVs.
     */
        float4x2 GetUVs(const uint4 data)
        {
            float4 u = DecodeFromBase17((data[2] >> 15) & BITMASK(17));
            float4 v = DecodeFromBase17((data[3] >> 15) & BITMASK(17));

            float4x2 uvs;
            uvs[0] = float2(u.x, v.x);
            uvs[1] = float2(u.y, v.y);
            uvs[2] = float2(u.z, v.z);
            uvs[3] = float2(u.w, v.w);

            return uvs;
        }

        /**
     * \brief Get the texture repetition.
     * \param data The data to decode.
     * \return The texture repetition.
     */
        float2 GetTextureRepetition(const uint4 data)
        {
            const uint x = (data[2] >> 4) & BITMASK(4);
            const uint y = (data[2] >> 0) & BITMASK(4);

            return float2(x + 1, y + 1);
        }

        enum Foliage
        {
            IS_UPPER_PART = 0,
            IS_DOUBLE_PLANT = 1,
        };

        /**
     * \brief Get the foliage flags.
     * \param data The data to decode.
     * \param flag The flag to get.
     * \return True if the flag is set.
     */
        bool GetFoliageFlag(const uint4 data, const Foliage flag)
        {
            return (data[2] >> flag) & BITMASK(1);
        }
    }
}

#endif
