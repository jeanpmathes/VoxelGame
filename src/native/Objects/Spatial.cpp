#include "stdafx.h"
#include "Spatial.hpp"

Spatial::Spatial(NativeClient& client) : Object(client)
{
    XMStoreFloat3(&m_position, DirectX::XMVectorZero());
    XMStoreFloat4(&m_rotation, DirectX::XMQuaternionIdentity());
    RecalculateTransform();
}

bool Spatial::ClearTransformDirty()
{
    const bool wasDirty = m_transformDirty;
    m_transformDirty = false;
    return wasDirty;
}

void Spatial::SetPosition(const DirectX::XMFLOAT3& position)
{
    m_position = position;
    RecalculateTransform();
}

const DirectX::XMFLOAT3& Spatial::GetPosition() const
{
    return m_position;
}

void Spatial::SetRotation(const DirectX::XMFLOAT4& rotation)
{
    m_rotation = rotation;
    RecalculateTransform();
}

const DirectX::XMFLOAT4& Spatial::GetRotation() const
{
    return m_rotation;
}

const DirectX::XMFLOAT4X4& Spatial::GetTransform() const
{
    return m_transform;
}

void Spatial::RecalculateTransform()
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
