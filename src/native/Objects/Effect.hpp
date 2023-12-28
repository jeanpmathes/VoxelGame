// <copyright file="Effect.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

/**
 * \brief An effect, rendered in the 3D scene using raster-based techniques.
 */
class Effect final : public Spatial, public Drawable
{
    DECLARE_OBJECT_SUBCLASS(Effect)

public:
    explicit Effect(NativeClient& client);
    void Initialize();

    using Spatial::GetClient;
};
