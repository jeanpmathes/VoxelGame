#include "stdafx.h"
#include "SpatialObject.h"

SpatialObject::SpatialObject(NativeClient& client) : Object(client)
{
    XMStoreFloat3(&m_position, DirectX::XMVectorZero());
    XMStoreFloat4(&m_rotation, DirectX::XMQuaternionIdentity());
    RecalculateTransform();
}

bool SpatialObject::ClearTransformDirty()
{
    const bool wasDirty = m_transformDirty;
    m_transformDirty = false;
    return wasDirty;
}

void SpatialObject::SetPosition(const DirectX::XMFLOAT3& position)
{
    m_position = position;
    RecalculateTransform();
}

const DirectX::XMFLOAT3& SpatialObject::GetPosition() const
{
    return m_position;
}

void SpatialObject::SetRotation(const DirectX::XMFLOAT4& rotation)
{
    m_rotation = rotation;
    RecalculateTransform();
}

const DirectX::XMFLOAT4& SpatialObject::GetRotation() const
{
    return m_rotation;
}

const DirectX::XMFLOAT4X4& SpatialObject::GetTransform() const
{
    return m_transform;
}

void SpatialObject::RecalculateTransform()
{
    const DirectX::XMVECTOR position = XMLoadFloat3(&m_position);
    const DirectX::XMVECTOR rotation = XMLoadFloat4(&m_rotation);
    const DirectX::XMVECTOR scale = DirectX::XMVectorSet(1.0f, 1.0f, 1.0f, 0.0f);

    const DirectX::XMMATRIX transform = DirectX::XMMatrixAffineTransformation(
        scale,
        DirectX::XMVectorZero(),
        rotation,
        position);

    XMStoreFloat4x4(&m_transform, transform);
    m_transformDirty = true;
}
