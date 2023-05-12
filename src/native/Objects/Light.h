// <copyright file="Light.h" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "SpatialObject.h"

/**
 * \brief Represents the light of the space. It is an directional light.
 */
class Light final : public SpatialObject
{
    DECLARE_OBJECT_SUBCLASS(Light)

public:
    explicit Light(NativeClient& client);

    void SetDirection(const DirectX::XMFLOAT3& direction);
    [[nodiscard]] const DirectX::XMFLOAT3& GetDirection() const;

private:
    DirectX::XMFLOAT3 m_direction = {0.0f, 0.0f, 0.0f};
};
