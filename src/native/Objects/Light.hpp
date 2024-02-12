// <copyright file="Light.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "Spatial.hpp"

/**
 * \brief Represents the light of the space. It is an directional light.
 */
class Light final : public Spatial
{
    DECLARE_OBJECT_SUBCLASS(Light)

public:
    explicit Light(NativeClient& client);

    void                                   SetDirection(DirectX::XMFLOAT3 const& direction);
    [[nodiscard]] DirectX::XMFLOAT3 const& GetDirection() const;

private:
    DirectX::XMFLOAT3 m_direction = {0.0f, 0.0f, 0.0f};
};
