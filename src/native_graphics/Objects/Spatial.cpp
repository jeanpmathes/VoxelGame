#include "stdafx.h"
#include "Spatial.hpp"

Spatial::Spatial(NativeClient& client)
    : Object(client)
{
    XMStoreFloat3(&m_position, DirectX::XMVectorZero());
    XMStoreFloat4(&m_rotation, DirectX::XMQuaternionIdentity());
    RecalculateTransform();
}

bool Spatial::ClearTransformDirty()
{
    bool const wasDirty = m_transformDirty;
    m_transformDirty    = false;
    return wasDirty;
}

void Spatial::SetPosition(DirectX::XMFLOAT3 const& position)
{
    m_position = position;
    RecalculateTransform();
}

DirectX::XMFLOAT3 const& Spatial::GetPosition() const { return m_position; }

void Spatial::SetRotation(DirectX::XMFLOAT4 const& rotation)
{
    m_rotation = rotation;
    RecalculateTransform();
}

DirectX::XMFLOAT4 const& Spatial::GetRotation() const { return m_rotation; }

DirectX::XMFLOAT4X4 const& Spatial::GetTransform() const { return m_transform; }

void Spatial::RecalculateTransform()
{
    DirectX::XMVECTOR const position = XMLoadFloat3(&m_position);
    DirectX::XMVECTOR const rotation = XMLoadFloat4(&m_rotation);
    DirectX::XMVECTOR const scale    = DirectX::XMVectorSet(1.0f, 1.0f, 1.0f, 0.0f);

    DirectX::XMMATRIX const transform = DirectX::XMMatrixAffineTransformation(scale, DirectX::XMVectorZero(), rotation, position);

    XMStoreFloat4x4(&m_transform, transform);
    m_transformDirty = true;
}
