//  <copyright file="TextureAnimation.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

// ReSharper disable once CppUnusedIncludeDirective

#ifndef VG_SHADER_TEXTURE_ANIMATION_HLSL
#define VG_SHADER_TEXTURE_ANIMATION_HLSL

#include "Decoding.hlsl"

/**
 * \brief Operations required for texture animation.
 */
namespace vg
{
    namespace animation
    {
        /**
         * \brief Get the animated texture index.
         * \param index The base index of the texture.
         * \param primitive The primitive ID.
         * \param time The current time.
         * \param frameCount The number of frames the animation has.
         * \param quadFactor Factor to weight the impact of the primitive ID.
         * \return The animated texture index.
         */
        uint GetAnimatedIndex(const uint index, const uint primitive, const float time, const uint frameCount,
                              const float quadFactor)
        {
            if (index == 0) return 0;

            return index + uint(fmod(time * frameCount + primitive * quadFactor, frameCount));
        }

        /**
         * \brief Get the animated texture index for blocks.
         * \param index The base index of the texture.
         * \param primitive The primitive ID.
         * \param time The current time.
         * \return The animated texture index.
         */
        uint GetAnimatedBlockTextureIndex(const uint index, const uint primitive, const float time)
        {
            return GetAnimatedIndex(index, primitive, time, 8, 0.125f);
        }

        /**
         * \brief Get the animated texture index for fluids.
         * \param index The base index of the texture.
         * \param primitive The primitive ID.
         * \param time The current time.
         * \return The animated texture index.
         */
        uint GetAnimatedFluidTextureIndex(const uint index, const uint primitive, const float time)
        {
            return GetAnimatedIndex(index, primitive, time, 16, 0.000f);
        }

        /**
         * \brief Get the animated texture index.
         * \param data The texture data.
         * \param primitive The primitive ID.
         * \param time The current time.
         * \param isBlock Whether the texture is for a block or not.
         * \return The animated texture index.
         */
        int GetAnimatedTextureIndex(const uint4 data, const uint primitive, const float time, const bool isBlock)
        {
            uint textureIndex = decode::GetTextureIndex(data);

            const bool animated = decode::GetAnimationFlag(data);
            if (animated && isBlock) textureIndex = GetAnimatedBlockTextureIndex(textureIndex, primitive, time);
            if (animated && !isBlock) textureIndex = GetAnimatedFluidTextureIndex(textureIndex, primitive, time);

            return textureIndex;
        }
    }
}

#endif
