// <copyright file="Light.h" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "SpatialObject.h"

/**
 * \brief Represents the light of the space.
 */
class Light : public SpatialObject
{
public:
    ~Light() override = default;

    explicit Light(NativeClient& client);
};
