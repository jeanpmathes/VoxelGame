//  <copyright file="TextureAnimation.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

// ReSharper disable once CppUnusedIncludeDirective

#include "Decoding.hlsl"

uint GetAnimatedIndex(const uint index, const uint primitive, const float time, const uint frameCount,
                      const float quadFactor)
{
    if (index == 0) return 0;

    return index + uint(fmod(time * frameCount + primitive * quadFactor, frameCount));
}

uint GetAnimatedBlockTextureIndex(const uint index, const uint primitive, const float time)
{
    return GetAnimatedIndex(index, primitive, time, 8, 0.125);
}

uint GetAnimatedFluidTextureIndex(const uint index, const uint primitive, const float time)
{
    return GetAnimatedIndex(index, primitive, time, 16, 0.00);
}

int GetAnimatedTextureIndex(const uint4 data, const uint primitive, const float time, const bool isBlock)
{
    uint textureIndex = decode::GetTextureIndex(data);

    const bool animated = decode::GetAnimationFlag(data);
    if (animated && isBlock) textureIndex = GetAnimatedBlockTextureIndex(textureIndex, primitive, time);
    if (animated && !isBlock) textureIndex = GetAnimatedFluidTextureIndex(textureIndex, primitive, time);

    return textureIndex;
}
