// <copyright file="Spatial.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "Object.hpp"

class NativeClient;

struct SpatialData
{
    DirectX::XMFLOAT3 position;
    DirectX::XMFLOAT4 rotation;
};

/**
 * \brief The base class of all objects in the space that can be observed. This explicitly excludes the camera.
 */
class Spatial : public Object
{
    DECLARE_OBJECT_SUBCLASS(Spatial)

protected:
    explicit Spatial(NativeClient& client);

    [[nodiscard]] bool ClearTransformDirty();

public:
    void SetPosition(const DirectX::XMFLOAT3& position);
    [[nodiscard]] const DirectX::XMFLOAT3& GetPosition() const;

    void SetRotation(const DirectX::XMFLOAT4& rotation);
    [[nodiscard]] const DirectX::XMFLOAT4& GetRotation() const;

    [[nodiscard]] const DirectX::XMFLOAT4X4& GetTransform() const;

private:
    void RecalculateTransform();

    DirectX::XMFLOAT3 m_position{};
    DirectX::XMFLOAT4 m_rotation{};
    DirectX::XMFLOAT4X4 m_transform{};

    bool m_transformDirty = true;
};
