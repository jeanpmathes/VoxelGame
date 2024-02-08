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
    void                                   SetPosition(DirectX::XMFLOAT3 const& position);
    [[nodiscard]] DirectX::XMFLOAT3 const& GetPosition() const;

    void                                   SetRotation(DirectX::XMFLOAT4 const& rotation);
    [[nodiscard]] DirectX::XMFLOAT4 const& GetRotation() const;

    [[nodiscard]] DirectX::XMFLOAT4X4 const& GetTransform() const;

private:
    void RecalculateTransform();

    DirectX::XMFLOAT3   m_position{};
    DirectX::XMFLOAT4   m_rotation{};
    DirectX::XMFLOAT4X4 m_transform{};

    bool m_transformDirty = true;
};
